using System.Collections.Generic;
using System.Linq;
using System;

public class Pair<T1, T2>
{
	public T1 First { get; set; }
	public T2 Second { get; set; }

	public Pair(T1 first, T2 second)
	{
		First = first;
		Second = second; 
	}
}

public class SequentialPair
{
	public Handle Low { get; set; }
	public Handle High { get; set; }

	public SequentialPair(Handle low, Handle high)
	{
		Low = low;
		High = high;
	}
}

public static class InstructionAnalysis
{
	public static bool IsUsedAfterwards(List<Instruction> instructions, Register register, int position, Instruction ignore)
	{
		for (var i = position + 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (instruction == ignore)
			{
				continue;
			}

			if (Reads(instruction, new RegisterHandle(register)) || instruction.Is(InstructionType.CALL, InstructionType.JUMP, InstructionType.LABEL))
			{
				return true;
			}

			if (Writes(instruction, new RegisterHandle(register)))
			{
				return false;
			}
		}

		return false;
	}

	public static List<Instruction>? TryGetAllUsagesAfterwards(List<Instruction> instructions, Register register, int position)
	{
		var usages = new List<Instruction>();
		var handle = new RegisterHandle(register);

		for (var i = position + 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL))
			{
				return null;
			}

			if (instruction.Is(InstructionType.CALL) && register.IsVolatile)
			{
				return usages;
			}

			if (Reads(instruction, handle))
			{
				// If the instruction locks the register, it should not be redirected
				if (Locks(instruction, handle))
				{
					return null;
				}

				usages.Add(instruction);
				continue;
			}

			if (Writes(instruction, handle))
			{
				return usages;
			}
		}

		return usages;
	}

	public static bool IsEditedBeforeRelocation(List<Instruction> instructions, Register register, int position)
	{
		var handle = new RegisterHandle(register);

		for (var i = position + 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			// Do not analyze conditional execution so return that it will be edited
			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL))
			{
				return true;
			}

			// If the instruction is a call and the register is volatile, the call might edit it
			if (instruction.Is(InstructionType.CALL) && register.IsVolatile)
			{
				return false;
			}

			// Check if the instruction edits the register
			if (Writes(instruction, handle))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Collect all registers which the instruction parameters reads
	/// </summary>
	public static IEnumerable<Register> GetAllInputRegisters(InstructionParameter parameter)
	{
		return parameter.IsAnyRegister && (!parameter.IsDestination || Flag.Has(parameter.Flags, ParameterFlag.READS))
			? new[] { parameter.Value!.To<RegisterHandle>().Register }
			: parameter.Value!.GetRegisterDependentResults().Select(i => i.Value.To<RegisterHandle>().Register);
	}

	/// <summary>
	/// Collect all registers which the instruction reads
	/// </summary>
	public static Register[] GetAllInputRegisters(Instruction instruction)
	{
		return instruction.Parameters.SelectMany(i => GetAllInputRegisters(i)).ToArray();
	}

	/// <summary>
	/// Collect all registers which the handle reads
	/// </summary>
	public static Register[] GetAllInputRegisters(Handle handle)
	{
		return handle.Is(HandleType.MEMORY) ? handle.GetRegisterDependentResults().Select(i => i.Value.To<RegisterHandle>().Register).ToArray() : Array.Empty<Register>();
	}

	public static bool IsVolatile(Handle handle)
	{
		if (handle.Is(HandleInstanceType.REGISTER))
		{
			return handle.To<RegisterHandle>().Register.IsVolatile;
		}

		return handle.GetRegisterDependentResults().Any(i => IsVolatile(i.Value));
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

	public static bool Locks(Instruction instruction, Handle handle)
	{
		return instruction.Parameters.Any(i => i.Value!.Equals(handle) && Flag.Has(i.Flags, ParameterFlag.LOCKED));
	}

	public static bool IsReturnRegister(Unit unit, Handle handle)
	{
		if (!handle.Is(HandleInstanceType.REGISTER))
		{
			return false;
		}

		var register = handle.To<RegisterHandle>().Register;

		return register == unit.GetStandardReturnRegister() || register == unit.GetDecimalReturnRegister();
	}

	public static bool IsDivisionRegister(Unit unit, Handle handle)
	{
		if (!handle.Is(HandleInstanceType.REGISTER))
		{
			return false;
		}

		var register = handle.To<RegisterHandle>().Register;

		return register == unit.GetNumeratorRegister() || register == unit.GetRemainderRegister();
	}

	public static bool IsIntermediate(Instruction instruction)
	{
		return Flag.Has(instruction.Parameters.Find(i => i.IsDestination)!.Flags, ParameterFlag.READS);
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

	private static bool IsEditedBetween(List<Instruction> instructions, int start, int end, Handle handle, Instruction ignore)
	{
		for (var i = start; i <= end; i++)
		{
			var instruction = instructions[i];

			if (string.IsNullOrEmpty(instruction.Operation) || instruction == ignore)
			{
				continue;
			}

			var destination = instruction.Destination;

			if (destination == null)
			{
				continue;
			}

			if (instruction.Is(InstructionType.CALL))
			{
				if ((handle.Is(HandleType.REGISTER) || handle.Is(HandleType.MEDIA_REGISTER)) && handle.To<RegisterHandle>().Register.IsVolatile)
				{
					return true;
				}

				if (handle.Is(HandleType.MEMORY))
				{
					return true;
				}
			}

			if (Writes(instruction, handle))
			{
				return true;
			}

			if (destination.IsMemoryAddress && handle.Is(HandleType.MEMORY) && IsIntersectionPossible(destination.Value!, handle))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Tries execute the move by inlining it to other instructions
	/// </summary>
	private static void TryInlineMoveInstruction(Unit unit, MoveInstruction move, int i, List<Instruction> instructions, Instruction[] enforced, bool reordered)
	{
		// Ensure the move has a destination and a source
		if (move.Destination == null || move.Source == null)
		{
			return;
		}

		var destination = move.Destination.Value!;
		var source = move.Source.Value!;

		// 1. Skip moves which are redundant
		// 2. If the specified move is a conversion, skip the move
		// 3. If the source is not any register, skip the move
		if (destination.Equals(source) || destination.Format.IsDecimal() != source.Format.IsDecimal() || (!source.Is(HandleType.REGISTER) && !source.Is(HandleType.MEDIA_REGISTER)))
		{
			return;
		}

		var dependencies = GetAllInputRegisters(destination);

		var volatility_changes = IsVolatile(destination) != IsVolatile(source);

		var is_destination_memory_address = move.Destination.IsMemoryAddress;
		var is_source_return_register = IsReturnRegister(unit, source);
		var is_source_register = source.Is(HandleType.REGISTER) || source.Is(HandleType.MEDIA_REGISTER);

		var is_destination_division_register = Assembler.IsX64 && IsDivisionRegister(unit, destination);
		var is_source_division_register = Assembler.IsX64 && IsDivisionRegister(unit, source);

		var root = (Instruction?)null;
		var intermediates = new List<Instruction>();
		var usages = new List<Instruction>();

		for (var j = i - 1; j >= 0; j--)
		{
			var instruction = instructions[j];

			// Skip instructions which will not be in the output
			if (string.IsNullOrWhiteSpace(instruction.Operation))
			{
				continue;
			}

			// 1. If the volatility changes between the source and the destination, the call might influence the execution
			// 2. If the source is the return register of the call, nothing can be done
			// 3. If the destination is a memory address, the call might read from it, so inlining should not be done
			if (instruction.Is(InstructionType.CALL) && (volatility_changes || is_source_return_register || is_destination_memory_address))
			{
				break;
			}
			
			// If either the destination or the source is a division related register, inlining should be skipped just in case
			if (instruction.Is(InstructionType.DIVISION) && (is_destination_division_register || is_source_division_register))
			{
				break;
			}

			// Do not analyze conditional execution
			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL, InstructionType.RETURN))
			{
				break;
			}

			// This prevents the following situation from happening:
			// lea rax, [rdx*2]
			// mov rcx, rbx
			// mov rcx, rax
			// =>
			// lea rcx, [rdx*2]
			// mov rcx, rbx
			// This situation usually happens when the compiler makes a redundant copy
			if (Writes(instruction, destination))
			{
				return;
			}

			var reads = Reads(instruction, source);
			var writes = Writes(instruction, source);

			// If this instruction writes to the source, it means the source instruction has been located
			if (writes && !IsIntermediate(instruction))
			{
				root = instruction;
				break;
			}

			// If the current instruction writes to any of the dependencies, this instruction can not be moved above it
			if (dependencies.Any(i => Writes(instruction, new RegisterHandle(i))))
			{
				break;
			}

			// If this instructions reads from the destination, inlining must be aborted
			if (Reads(instruction, destination))
			{
				return;
			}

			// If this instructions reads from the source, the objective is to replace the source with the destination
			if (reads)
			{
				// 1. If the destination is a memory address, the inlining would increase the amount of memory accesses, which is not good
				// 2. If the instruction locks the source handle, the source should not be redirected
				if (is_destination_memory_address || Locks(instruction, source))
				{
					return;
				}

				// If the instruction both reads and writes to the source, it means it is an intermediate instruction
				if (writes)
				{
					intermediates.Add(instruction);
				}

				// Apply the inlining after it has been confirmed this should be done since this process may be aborted any time
				usages.Add(instruction);
				continue;
			}
		}

		// 1. If no root can be found, try to use the captured intermediates
		// 2. If the root is a copy, do not redirect it, instead try to use the intermediates
		// 3. If the root is one of the enforced instructions, it should not be disturbed
		if (root == null || (root.Is(InstructionType.MOVE) && root.To<MoveInstruction>().Type != MoveType.RELOCATE) || enforced.Contains(root))
		{
			// It is a default assumption that intermediates can not hold memory addresses
			if (is_destination_memory_address)
			{
				return;
			}

			foreach (var intermediate in intermediates)
			{
				// Example:
				// add rax, 1
				// mov rbp, rax
				// lea rcx, [rdi+rax]
				// =>
				// lea rbp, [rax+1]
				// lea rcx, [rdi+rbp]

				// NOTE: The usages list is not enough since it does not record instruction below the move which use the source register
				var start = instructions.IndexOf(intermediate);
				var intermediate_usages = TryGetAllUsagesAfterwards(instructions, source.To<RegisterHandle>().Register, start);

				if (intermediate_usages == null)
				{
					continue;
				}

				if (intermediate_usages.Any() && IsEditedBetween(instructions, start, instructions.IndexOf(intermediate_usages.Last()), destination, move))
				{
					return;
				}

				// Try to redirect the intermediate to the destination
				if (intermediate.Redirect(destination))
				{
					intermediate_usages.ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
					instructions.Remove(move);
					return;
				}
			}

			return;
		}

		if (reordered && !is_source_register)
		{
			// Since the instructions have been reordered and the source is not a register, inlining should be aborted
			// NOTE: Usages of the sources can not be loaded if it is a memory address
			return;
		}

		// Example:
		// mov rdi, rdx (relocate)
		// mov rcx, rdi (copy)
		// ...
		// call ...
		// ...
		// mov rcx, [rdi]
		
		if ((reordered || move.Type != MoveType.RELOCATE) && is_source_register)
		{
			// NOTE: The usages list is not enough since it does not record instruction below the move which use the source register
			var position = instructions.IndexOf(root);
			usages = TryGetAllUsagesAfterwards(instructions, source.To<RegisterHandle>().Register, position);
			
			// If the usages of the root instruction could not be collected, inlining should not be done
			if (usages == null || is_destination_memory_address)
			{
				return;
			}

			if (usages.Any() && IsEditedBetween(instructions, position, instructions.IndexOf(usages.Last()), destination, move))
			{
				return;
			}

			// If any of the usages writes to the copy, inlining should be aborted
			if (IsEditedBeforeRelocation(instructions, destination.To<RegisterHandle>().Register, position))
			{
				return;
			}
		}

		// Try to redirect the root to the destination
		if (root.Redirect(destination))
		{
			usages.ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
			instructions.Remove(move);
		}
	}

	/// <summary>
	/// Finds all instrcutions which should not be modified
	/// </summary>
	private static Instruction[] FindEnforcedMoveInstructions(List<Instruction> instructions)
	{
		return instructions.FindAll(i => i.Is(InstructionType.CALL)).Cast<CallInstruction>().SelectMany(i => i.ParameterInstructions).ToArray();
	}

	/// <summary>
	/// Tries execute moves by inlining them to another instructions
	/// </summary>
	private static void InlineMoveInstructions(Unit unit, List<Instruction> instructions, bool reordered)
	{
		var enforced = FindEnforcedMoveInstructions(instructions);

		for (var i = 0; i < instructions.Count; i++)
		{
			if (instructions[i].Is(InstructionType.MOVE))
			{
				TryInlineMoveInstruction(unit, instructions[i].To<MoveInstruction>(), i, instructions, enforced, reordered);
			}
		}
	}

	public static void Optimize(Unit unit)
	{
		//Remove all moves whose values are not read
		RemoveRedundantMoves(unit, unit.Instructions);

		// Try to inline moves for the first time
		InlineMoveInstructions(unit, unit.Instructions, false);

		// Reorder the instructions
		Reorder(unit.Instructions);

		// Remove all moves whose values are not read
		RemoveRedundantMoves(unit, unit.Instructions);

		// Try to inline moves for the second time since the instructions have been reordered
		InlineMoveInstructions(unit, unit.Instructions, false);

		unit.Reindex();

		//Combine(unit.Instructions);
	}

	public static bool IsIntersectionPossible(Handle i, Handle j)
	{
		if (i.Is(HandleInstanceType.DATA_SECTION) && j.Is(HandleInstanceType.DATA_SECTION))
		{
			var first = i.To<DataSectionHandle>();
			var second = j.To<DataSectionHandle>();

			if (first.Identifier != second.Identifier)
			{
				return true;
			}

			// Check whether the memory addresses intersect provided that they have the same starting address
			var a = first.Offset;
			var b = first.Offset + first.Size.Bytes;

			var x = second.Offset;
			var y = second.Offset + second.Size.Bytes;

			return (a >= x && a < y) || (b > x && b <= y);
		}

		if (i.Is(HandleInstanceType.CONSTANT_DATA_SECTION) && j.Is(HandleInstanceType.CONSTANT_DATA_SECTION))
		{
			/// TODO: Investigate this more since constant data section handles might intersect in the future since duplications should be removed
			return false;
		}

		if (i.Is(HandleInstanceType.COMPLEX_MEMORY) && j.Is(HandleInstanceType.COMPLEX_MEMORY))
		{
			return true;
		}
		
		if (i.Is(HandleInstanceType.MEMORY) && j.Is(HandleInstanceType.MEMORY))
		{
			var first = i.To<MemoryHandle>();
			var second = j.To<MemoryHandle>();

			// If both of the memory handles have different starting address objects, it can not be known for certain with the current information whether they will intersect, so return that they can by default
			if (!first.Start.Equals(second.Start))
			{
				return true;
			}

			// Check whether the memory addresses intersect provided that they have the same starting address
			var a = first.Offset;
			var b = first.Offset + first.Size.Bytes;

			var x = second.Offset;
			var y = second.Offset + second.Size.Bytes;

			return (a >= x && a < y) || (b > x && b <= y);
		}

		return true;
	}

	/// <summary>
	/// Reorder instructions by moving every instruction upwards in the list until they hit an instruction which they are dependent on
	/// </summary>
	public static void Reorder(List<Instruction> instructions)
	{
		for (var i = 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (string.IsNullOrEmpty(instruction.Operation) || !instruction.Parameters.Any() || instruction.Destination == null)
			{
				continue;
			}

			var dependencies = instruction.Parameters.Select(i => i.Value!).Concat(GetAllInputRegisters(instruction).Select(i => new RegisterHandle(i)));
			var destination = instruction.Destination.Value!;

			var position = i;

			while (true)
			{
				var obstacle = instructions[position - 1];

				// If the obstacle represents an instruction which affects the execution heavily, this instruction should not be moved above it
				if (obstacle.Is(InstructionType.CALL, InstructionType.INITIALIZE, InstructionType.JUMP, InstructionType.LABEL, InstructionType.DIVISION, InstructionType.RETURN))
				{
					break;
				}

				// 1. If this instruction writes to a destination which the obstacle uses, this instruction should not be moved above it
				// 2. If any of the dependencies of the current instruction are edited by the obstacle, this instruction should not be moved above it
				if (Reads(obstacle, destination) || dependencies.Any(i => Writes(obstacle, i)))
				{
					break;
				}

				// Collect all memory addresses from the two instructions
				var instruction_memory_addresses = instruction.Parameters.Where(i => i.IsMemoryAddress).ToArray();
				var obstacle_memory_addresses = obstacle.Parameters.Where(i => i.IsMemoryAddress).ToArray();

				if (instruction_memory_addresses.Any() && obstacle_memory_addresses.Any())
				{
					// If any of the destination memory addresses can intersect with the memory addresses of the obstacle, this instruction should not be moved above the obstacle
					if (instruction_memory_addresses.Where(i => i.IsDestination).Select(i => i.Value!).Any(i => obstacle_memory_addresses.Any(j => IsIntersectionPossible(i, j.Value!))))
					{
						break;
					}

					// If any of the destination memory addresses of the obstacle can intersect with the memory addresses, this instruction should not be moved above the obstacle
					if (obstacle_memory_addresses.Where(i => i.IsDestination).Select(i => i.Value!).Any(i => instruction_memory_addresses.Any(j => IsIntersectionPossible(i, j.Value!))))
					{
						break;
					}
				}

				position--;
			}

			if (i == position)
			{
				continue;
			}
			
			instructions.RemoveAt(i);
			instructions.Insert(position, instruction);
		}
	}

	public static Pair<Handle, Instruction?>? TryGetIndependentSource(List<Instruction> instructions, MoveInstruction start, int position)
	{
		if (!start.Source!.IsAnyRegister)
		{
			return new Pair<Handle, Instruction?>(start.Second!.Value, null);
		}

		var source = start.Source!.Value!;

		for (var i = position - 1; i >= 0; i--)
		{
			var instruction = instructions[i];

			if (string.IsNullOrEmpty(instruction.Operation))
			{
				continue;
			}

			if (instruction.Is(InstructionType.CALL, InstructionType.JUMP, InstructionType.LABEL))
			{
				return null;
			}

			if (Writes(instruction, source) && !IsIntermediate(instruction))
			{
				var root = instruction.Is(InstructionType.MOVE) ? instruction.To<MoveInstruction>().Source!.Value! : null;

				if (root == null)
				{
					return null;
				}

				var usages = TryGetAllUsagesAfterwards(instructions, source.To<RegisterHandle>().Register, i);
				
				if (usages.Any(i => i != start))
				{
					return null;
				}

				return new Pair<Handle, Instruction?>(root, instruction);
			}
		}

		return null;
	}

	public static SequentialPair? TryGetSequentialPair(Handle first, Handle second)
	{
		// Ensure both of the handles are either memory addresses or constants
		if (first.Type != second.Type)
		{
			return null;
		}

		if (first.Is(HandleType.CONSTANT))
		{
			return new SequentialPair(first, second);
		}
		
		// Ensure both of the handles are memory addresses
		if (!first.Is(HandleType.MEMORY))
		{
			return null;
		}

		var low = first.To<MemoryHandle>();
		var high = second.To<MemoryHandle>();

		// Ensure the moves have the same starting address
		if (!low.Start.Value.Equals(high.Start.Value))
		{
			return null;
		}

		// Exchange the instruction so that the low actually represents the lower address
		if (low.Offset > high.Offset)
		{
			var temporary = low;
			low = high;
			high = temporary;
		}

		// Ensure the moves are sequential
		if (low.Offset + low.Size.Bytes != high.Offset)
		{
			return null;
		}

		return new SequentialPair(low, high);
	}

	public static void Combine(List<Instruction> instructions)
	{
		for (var i = 0; i < instructions.Count; i++)
		{
			// Try to find two sequential move instructions
			if (!instructions[i].Is(InstructionType.MOVE))
			{
				continue;
			}

			var first_move = instructions[i].To<MoveInstruction>();
			
			// The first move is found but its destination must be a memory address
			if (!first_move.Destination!.IsMemoryAddress)
			{
				continue;
			}

			var first_source = TryGetIndependentSource(instructions, first_move, i);

			// Skip this move if its source is used in multiple locations or if it is a register
			if (first_source == null || first_source.First.Is(HandleType.REGISTER))
			{
				continue;
			}

			for (var j = i + 1; j < instructions.Count; j++)
			{
				/// TODO: Dependencies
				if (instructions[j].Is(InstructionType.CALL, InstructionType.JUMP, InstructionType.LABEL))
				{
					break;
				}

				// Try to find the next move instruction
				if (!instructions[j].Is(InstructionType.MOVE))
				{
					continue;
				}

				var second_move = instructions[j].To<MoveInstruction>();

				// The second move is found but its destination must be a memory address
				if (!second_move.Destination!.IsMemoryAddress)
				{
					continue;
				}

				var second_source = TryGetIndependentSource(instructions, second_move, i);

				// Skip this move if its source is used in multiple locations or if it is a register
				if (second_source == null || second_source.First.Is(HandleType.REGISTER))
				{
					continue;
				}

				var destination = TryGetSequentialPair(first_move.Destination!.Value!, second_move.Destination!.Value!);
				var source = TryGetSequentialPair(first_source.First, second_source.First);

				// Ensure sequential destinations and sources were found and ensure the sizes match each other
				if (destination == null || source == null || destination.High.Size != destination.Low.Size || source.High.Size != source.High.Size)
				{
					continue;
				}

				var inline_destination_size = destination.Low.Size.Bytes * 2;
				var inline_source_size = source.Low.Size.Bytes * 2;

				// If the low part of the source is a constant, it means that the high is also a constant
				if (source.Low.Is(HandleType.CONSTANT))
				{
					/// TODO: Support this
					if (inline_destination_size >= 8)
					{
						continue;
					}

					var low_constant_value = source.Low.To<ConstantHandle>().Value as long?;
					var high_constant_value = source.High.To<ConstantHandle>().Value as long?;

					// Require both values to be integers
					/// TODO: Investigate possibility of decimal values
					if (low_constant_value == null || high_constant_value == null)
					{
						continue;
					}

					low_constant_value &= destination.Low.Size.Bytes switch
					{
						1 => 0xFF,
						2 => 0xFFFF,
						4 => 0xFFFFFFFF,
						_ => long.MinValue // 0xFFFFFFFFFFFFFFFF
					};

					high_constant_value &= destination.Low.Size.Bytes switch
					{
						1 => 0xFF,
						2 => 0xFFFF,
						4 => 0xFFFFFFFF,
						_ => long.MinValue // 0xFFFFFFFFFFFFFFFF
					};

					var constant = (high_constant_value << destination.Low.Size.Bits) | low_constant_value;

					second_move.Destination!.Value = destination.Low;
					second_move.Destination!.Value.Format = Size.FromBytes(inline_destination_size).ToFormat();
					
					second_move.Source!.Value = new ConstantHandle(constant);
					second_move.Source!.Value.Format = Size.FromBytes(inline_destination_size).ToFormat();
					
					second_move.OnPostBuild();
				}
				else
				{
					/// TODO: Support this
					if (inline_source_size > 8)
					{
						continue;
					}

					second_move.Destination!.Value = destination.Low;
					second_move.Destination!.Size = Size.FromBytes(inline_destination_size);
					
					second_move.Source!.Value = source.Low;
					second_move.Source!.Size = Size.FromBytes(inline_source_size);
					
					second_move.OnPostBuild();
				}

				instructions.RemoveAt(i);

				if (first_source.Second != null)
				{
					instructions.Remove(first_source.Second);
				}
				
				if (second_source.Second != null)
				{
					instructions.Remove(second_source.Second);
				}
			}
		}
	}

	private static void RemoveRedundantMoves(Unit unit, List<Instruction> instructions)
	{
		for (var i = 0; i < instructions.Count;)
		{
			var instruction = instructions[i];

			// Skip instructions which will not be in the output
			if (string.IsNullOrEmpty(instruction.Operation))
			{
				i++;
				continue;
			}

			if (instruction.Is(InstructionType.MOVE) && IsRedundantMove(unit, instructions, instruction.To<MoveInstruction>(), i))
			{
				instructions.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private static bool IsRedundantMove(Unit unit, List<Instruction> instructions, MoveInstruction move, int position)
	{
		var destination = move.Destination!;

		for (var i = position + 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			// If the instruction reads from the destination of the specified move instruction, the move instruction can not be redundant
			if (Reads(instruction, destination.Value!))
			{
				return false;
			}
			
			// If any of the memory addresses in the instruction can intersect with the destination, the move instruction might not be redundant
			var instruction_memory_addresses = instruction.Parameters.Where(i => i.IsMemoryAddress).ToArray();

			if (destination.IsMemoryAddress && instruction_memory_addresses.Any(i => IsIntersectionPossible(i.Value!, destination.Value!)))
			{
				return false;
			}

			if (instruction.Is(InstructionType.CALL))
			{
				if (destination.IsMemoryAddress)
				{
					return false;
				}

				if (destination.IsAnyRegister && destination.Value!.To<RegisterHandle>().Register.IsVolatile)
				{
					// If any of the call parameter instructions interact with the destination of the specified move instruction, the move instruction is not redundant
					return instruction.To<CallInstruction>().ParameterInstructions.Any(i => i.Parameters.Any(i => i.Equals(destination.Value!)));
				}
			}

			// Do not analyze conditional execution
			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL))
			{
				return false;
			}

			// Finally, if the instruction writes to the same destination as the specified move, the move must be redundant
			if (Writes(instruction, destination.Value!))
			{
				return true;
			}
		}

		// The move can only be valid if its destination is either a memory address or a return register
		return !destination.IsMemoryAddress && (destination.IsAnyRegister && !IsReturnRegister(unit, destination.Value!));
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