using System.Collections.Generic;
using System.Linq;

public static class InstructionAnalysis
{
	public static bool IsVolatile(Handle handle)
	{
		return (handle is RegisterHandle x && x.Register.IsVolatile) || handle.GetRegisterDependentResults().Any(i => IsVolatile(i.Value));
	}

	public static bool Contains(Handle handle, Handle what)
	{
		return handle.GetRegisterDependentResults().Any(i => i.Value.Equals(what) || Contains(i.Value, what));
	}

	public static bool Reads(InstructionParameter parameter, Handle handle)
	{
		return parameter.Value!.Equals(handle) && (!parameter.IsDestination || Flag.Has(parameter.Flags, ParameterFlag.READS)) || Contains(parameter.Value!, handle);
	}

	public static bool Reads(Instruction instruction, Handle handle)
	{
		return instruction.Parameters.Any(i => Reads(i, handle));
	}

	public static bool Writes(InstructionParameter parameter, Handle handle)
	{
		return parameter.Value!.Equals(handle) && parameter.IsDestination;
	}

	public static bool Writes(Instruction instruction, Handle handle)
	{
		return instruction.Parameters.Any(i => Writes(i, handle));
	}

	public static bool IsReturnRegister(Unit unit, Handle handle)
	{
		return handle is RegisterHandle x && (x.Register == unit.GetStandardReturnRegister() || x.Register == unit.GetDecimalReturnRegister());
	}

	public static bool IsDivisionRegister(Unit unit, Handle handle)
	{
		return handle is RegisterHandle x && (x.Register == unit.GetNumeratorRegister() || x.Register == unit.GetRemainderRegister());
	}

	public static void Replace(Result result, Handle what, Handle to)
	{
		if (result.Value!.Equals(what))
		{
			result.Value = to;
		}
		else
		{
			result.Value!.GetRegisterDependentResults().ForEach(i => Replace(i, what, to));
		}
	}

	public static void Replace(InstructionParameter parameter, Handle what, Handle to)
	{
		if (parameter.Value!.Equals(what))
		{
			parameter.Value = to;
		}
		else
		{
			parameter.Value!.GetRegisterDependentResults().ForEach(i => Replace(i, what, to));
		}
	}

	public static void TryInlineMoveInstruction(Unit unit, MoveInstruction move, int i, List<Instruction> instructions, Instruction[] enforced)
	{
		if (move.Destination == null || move.Source == null)
		{
			return;
		}

		var destination = move.Destination.Value!;
		var source = move.Source.Value!;

		if (destination.Equals(source) || destination.Format.IsDecimal() != source.Format.IsDecimal())
		{
			return;
		}

		var volatility_changes = IsVolatile(destination) != IsVolatile(source);
		var is_destination_memory_address = move.Destination.IsMemoryAddress;
		var is_source_return_register = IsReturnRegister(unit, source);

		var is_destination_division_register = Assembler.IsX64 && IsDivisionRegister(unit, destination);
		var is_source_division_register = Assembler.IsX64 && IsDivisionRegister(unit, source);

		var write = (Instruction?)null;
		var intermediates = new List<Instruction>();
		var usages = new List<Instruction>();

		for (var j = i - 1; j >= 0; j--)
		{
			var instruction = instructions[j];

			// If volatility changes or source represents a return register, nothing can be done
			// TODO: Might be good to actually check whether the call actually returns to the same register
			if (instruction is CallInstruction && (volatility_changes || is_source_return_register))
			{
				break;
			}

			// TODO: This condition is safe but is it always necessary?
			if (instruction is DivisionInstruction && (is_destination_division_register || is_source_division_register))
			{
				break;
			}

			// TODO: Allows jumps when they jump to a label which is before the start
			if (instruction.Is(InstructionType.JUMP) || instruction.Is(InstructionType.LABEL))
			{
				break;
			}

			// If this instructions reads from the destination, inlining must be aborted
			// TODO: The instruction could be inlined?
			if (Reads(instruction, destination))
			{
				return;
			}

			var reads = Reads(instruction, source);
			var writes = Writes(instruction, source);

			// If this instructions reads from the source, the objective is to replace the source with the destination
			if (reads)
			{
				// However, if the destination is a memory address, the inlining would increase the amount of memory accesses, which is not good
				if (is_destination_memory_address)
				{
					return;
				}

				if (writes)
				{
					intermediates.Add(instruction);
				}

				// Apply the inlining after it has been confirmed this should be done since this process may be aborted any time
				usages.Add(instruction);
				continue;
			}

			// If this instruction writes to the source, it means the source has been located
			if (writes)
			{
				write = instruction;
				break;
			}
		}

		if (write == null || enforced.Contains(write))
		{
			foreach (var intermediate in intermediates)
			{
				if (intermediate.Redirect(destination))
				{
					usages.TakeWhile(i => i != intermediate).ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
					unit.Instructions.Remove(move);
					return;
				}
			}

			return;
		}

		if (write!.Redirect(destination))
		{
			usages.ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
			unit.Instructions.Remove(move);
		}
	}

	private static Instruction[] FindEnforcedMoveInstructions(List<Instruction> instructions)
	{
		return instructions.FindAll(i => i.Is(InstructionType.CALL)).Cast<CallInstruction>().SelectMany(i => i.ParameterInstructions).ToArray();
	}

	public static void InlineMoveInstructions(Unit unit, List<Instruction> instructions)
	{
		var enforced = FindEnforcedMoveInstructions(instructions);

		for (var i = 0; i < instructions.Count; i++)
		{
			if (instructions[i] is MoveInstruction move && move.Type == MoveType.RELOCATE)
			{
				TryInlineMoveInstruction(unit, move, i, instructions, enforced);
			}
		}
	}

	public static void Optimize(Unit unit)
	{
		InlineMoveInstructions(unit, unit.Instructions);
		unit.Reindex();
	}

	// Inlining:
	//
	// cmp x0, #0
	// b.ne L0
	//
	// => cbnz x0, L0
	// 
	// sub x0, x0, #1
	// cmp x0, #0
	// b.eq L0
	// =>
	// subs x0, x0, x1
	// b.eq L0
	// 
	// 
}