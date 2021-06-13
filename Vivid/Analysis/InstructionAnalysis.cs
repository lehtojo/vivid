using System;
using System.Collections.Generic;
using System.Linq;

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
	public bool Flipped { get; set; } = false;

	public SequentialPair(Handle low, Handle high, bool flipped)
	{
		Low = low;
		High = high;
		Flipped = flipped;
	}
}

public static class InstructionAnalysis
{
	/// <summary>
	/// Returns if the value of the specified register is used after the specified position
	/// </summary>
	public static bool IsUsedAfterwards(List<Instruction> instructions, Register register, int position, Instruction ignore)
	{
		for (var i = position + 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (instruction == ignore) continue;

			if (instruction.Is(InstructionType.RETURN) && register == instruction.To<ReturnInstruction>().ReturnRegister) return true;

			if (instruction.Is(InstructionType.REORDER) && register.IsVolatile)
			{
				// If any of the destinations is the specified register, the register is used
				if (instruction.To<ReorderInstruction>().Destinations.Where(i => i.Is(HandleInstanceType.REGISTER)).Cast<RegisterHandle>().Any(i => i.Register == register))
				{
					return true;
				}
			}

			if (instruction.Is(InstructionType.CALL) && register.IsVolatile) return false;

			if (Reads(instruction, new RegisterHandle(register)) || instruction.Is(InstructionType.CALL, InstructionType.JUMP, InstructionType.LABEL))
			{
				return true;
			}

			if (Writes(instruction, new RegisterHandle(register))) return false;
		}

		return false;
	}

	/// <summary>
	/// Returns all the instructions which use the specified register starting from the specified position.
	/// This function assumes that the usages are needed for relocating the value inside the specified register.
	/// This function returns null when it determines that modifying the specified register might not be safe.
	/// Example (register = rax):
	/// lea rcx, [rax+1] <- This instruction is added to the result list
	/// call ... <- Analysis is stopped here since the specified register rax is volatile
	/// add rax, 1
	/// Example (register = rbx):
	/// lea rcx, [rbx+1] <- This instruction is added to the result list
	/// call ...
	/// add rbx, 1 <- This instruction is added to the result list
	/// mov rax, rbx <- This instruction is added to the result list
	/// ret
	/// Example (register = rcx):
	/// add rcx, 1
	/// mov rbx, rcx
	/// call ... <- Analysis stopped here because the function uses the specified register as a parameter, therefore it is not considered safe to relocate the register rcx. Returned value is null.
	/// </summary>
	public static List<Instruction>? TryGetUsagesForRelocation(List<Instruction> instructions, Register register, int position)
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

			if (instruction.Is(InstructionType.RETURN))
			{
				// If the specified register is the return register which is used, it should not be redirected
				return instruction.To<ReturnInstruction>().ReturnRegister == register ? null : usages;
			}

			// If the instruction writes to the specified register and is not an intermediate instruction, return all the collected usages
			// Example:
			// add rcx, 1 <- Even though this instruction writes to the register rcx, it should not be considered to be the last usage of the value inside the register rcx
			// lea rdx, [rcx*2] <- Notice that the value of the register rcx is referenced here as well
			if (Writes(instruction, handle) && !IsIntermediate(instruction))
			{
				return usages;
			}

			if (Reads(instruction, handle))
			{
				// If the instruction locks the register, it should not be redirected
				// Example:
				// mov rax, 1
				// div rcx <- Since division instruction locks the register rax, the value inside the register rax should not be relocated
				// mov rdx, rax <- This instruction wants to relocate the value inside the register rax
				if (Locks(instruction, handle))
				{
					return null;
				}

				usages.Add(instruction);
				continue;
			}

			if (instruction.Is(InstructionType.REORDER) && register.IsVolatile)
			{
				// If the reorder instruction contains the current register, value of that register should not be relocated
				if (instruction.To<ReorderInstruction>().Destinations.Contains(handle))
				{
					return null;
				}
			}

			if (instruction.Is(InstructionType.CALL) && register.IsVolatile)
			{
				// Since the value of the current register does not represent an argument of the call, its lifetime ends here, if the current register is volatile
				if (register.IsVolatile)
				{
					return usages;
				}
			}
		}

		return usages;
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
		return parameter.Value!.Equals(handle) && Flag.Has(parameter.Flags, ParameterFlag.WRITES);
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
		return Flag.Has(instruction.Destination!.Flags, ParameterFlag.READS);
	}

	public static void Replace(Result result, Handle what, Handle to)
	{
		if (result.Value!.Equals(what))
		{
			var format = result.Value.Format;

			result.Value = to.Finalize();
			result.Value.Format = format;
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
			var format = parameter.Value.Format;

			parameter.Value = to.Finalize();
			parameter.Value.Format = format;
		}
		else
		{
			parameter.Value!.GetRegisterDependentResults().ForEach(i => Replace(i, what, to));
		}
	}

	/// <summary>
	/// Returns whether the specified handle is used between the specified range
	/// </summary>
	private static bool IsUsedBetween(List<Instruction> instructions, int start, int end, Handle handle, Instruction? ignore, bool writes, bool reads)
	{
		for (var i = start; i <= end; i++)
		{
			var instruction = instructions[i];
			if (instruction == ignore) continue;

			if (instruction.Is(InstructionType.CALL))
			{
				// If the handle represents a volatile register, its contents might change during function call
				if (handle.Is(HandleInstanceType.REGISTER) && handle.To<RegisterHandle>().Register.IsVolatile) return true;

				// Contents of memory handles can change during function call
				if (handle.Is(HandleType.MEMORY)) return true;
			}

			if (writes && Writes(instruction, handle)) return true;
			if (reads && Reads(instruction, handle)) return true;

			if (!handle.Is(HandleType.MEMORY)) continue; // Require the specified handle to be a memory handle

			// Check if any of the parameters can intersect with the specified handle
			foreach (var parameter in instruction.Parameters)
			{
				if (!parameter.IsMemoryAddress || !IsIntersectionPossible(parameter.Value!, handle)) continue;
				if ((writes && parameter.IsDestination) || (reads && parameter.IsSource)) return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Tries execute the move by inlining it to other instructions
	/// </summary>
	private static bool TryInlineMoveInstruction(Unit unit, MoveInstruction pivot, int i, List<Instruction> instructions, bool reordered)
	{
		// Ensure the move has a destination and a source
		if (pivot.Destination == null || pivot.Source == null)
		{
			return false;
		}

		var destination = pivot.Destination.Value!;
		var source = pivot.Source.Value!;

		// 1. Skip moves which are redundant
		// 2. If the specified move is a conversion, skip the move
		// 3. If the source is not any register, skip the move
		if (destination.Equals(source) || destination.Format.IsDecimal() != source.Format.IsDecimal() || !source.Is(HandleInstanceType.REGISTER))
		{
			return false;
		}

		var dependencies = GetAllInputRegisters(destination);

		var volatility_changes = IsVolatile(destination) != IsVolatile(source);

		var is_destination_memory_address = pivot.Destination.IsMemoryAddress;
		var is_source_return_register = IsReturnRegister(unit, source);
		var is_source_register = source.Is(HandleInstanceType.REGISTER);

		var is_destination_division_register = Assembler.IsX64 && IsDivisionRegister(unit, destination);
		var is_source_division_register = Assembler.IsX64 && IsDivisionRegister(unit, source);

		var root = (Instruction?)null;
		var intermediates = new List<Instruction>();
		var usages = new List<Instruction>();

		for (var j = i - 1; j >= 0; j--)
		{
			var instruction = instructions[j];

			// 1. If the volatility changes between the source and the destination, the call might influence the execution
			// 2. If the source is the return register of the call, nothing can be done
			// 3. If the destination is a memory address, the call might read from it, so inlining should not be done
			if (instruction.Is(InstructionType.CALL) && (volatility_changes || is_source_return_register || is_destination_memory_address))
			{
				break;
			}

			// If either the destination or the source is related to division and a division instruction is encountered, inlining must be aborted
			if (instruction.Is(InstructionType.DIVISION) && (is_destination_division_register || is_source_division_register))
			{
				break;
			}

			// Do not analyze conditional execution
			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL, InstructionType.RETURN))
			{
				break;
			}

			// Prevents illegal reordering of move instructions
			// Example:
			// lea rax, [rdx*2]
			// mov rcx, rbx
			// mov rcx, rax
			// If the last instruction was inlined, the result would be the following
			// lea rcx, [rdx*2]
			// mov rcx, rbx
			// Notice that the value will be different inside the register rcx at the end
			if (Writes(instruction, destination))
			{
				return false;
			}

			var reads = Reads(instruction, source);
			var writes = Writes(instruction, source);

			// If this instruction writes to the source without reading from it, it means the root instruction has been located
			/// NOTE: Checking whether the instruction is an intermediate instruction ensures it does not depend on the value inside the source
			// Example:
			// mov rcx, 10
			// add rcx, 1 <- Inlining here would break things because this is not the root instruction and this instruction is dependent on the value of the register rcx
			// mov rdx, rcx
			if (writes && !IsIntermediate(instruction))
			{
				root = instruction;
				break;
			}

			// If the current instruction writes to any of the dependencies, inlining must be aborted
			// Example:
			// mov rdx, rax <- Inlining would break things because the next instruction writes to the register rcx
			// add rcx, 1
			// mov qword ptr [rcx], rdx
			if (dependencies.Any(i => Writes(instruction, new RegisterHandle(i))))
			{
				break;
			}

			// If this instructions reads from the destination, inlining must be aborted
			// Example:
			// mov rdx, rbx
			// add rdx, 1
			// add rdx, rcx <- Inlining would break things because the root has not been found and the destination is used here
			// mov rcx, rdx
			if (Reads(instruction, destination))
			{
				return false;
			}

			// If this instructions reads from the source, the objective is to replace the source with the destination
			if (reads)
			{
				// 1. If the destination is a memory address, the inlining would increase the amount of memory accesses, which is not good
				// 2. If the instruction locks the source handle, the source should not be redirected
				if (is_destination_memory_address || Locks(instruction, source))
				{
					return false;
				}

				// If the instruction both reads and writes to the source, it means it is an intermediate instruction
				if (writes)
				{
					intermediates.Add(instruction);
				}

				// Apply the inlining after it has been confirmed this should be done since this process may be aborted any time
				usages.Add(instruction);
			}
		}

		/// NOTE: The root can not be a conditional move since it is an intermediate instruction

		// Determine whether the root instruction is a copy move instruction
		// 1. If the root instruction is a move instruction and it is not a relocation move, it is considered to be a copy move
		// 2. If the instructions have been reordered, the relocation move is considered to be a copy move as well
		var is_root_copy_move = root != null && root.Is(InstructionType.MOVE) && (root.To<MoveInstruction>().Type != MoveType.RELOCATE || reordered);

		// 1. If no root can be found, try to use the captured intermediates
		// 2. If the root is a copy, do not redirect it, instead try to use the captured intermediates
		if (root == null || is_root_copy_move)
		{
			// Here it is assumed that intermediate instructions can not have memory addresses as their destinations
			if (is_destination_memory_address) return false;

			foreach (var intermediate in intermediates)
			{
				// Example:
				// add rax, 1
				// mov rbp, rax
				// lea rcx, [rdi+rax]
				// Here the second instruction will be inlined and the result should be the following
				// lea rbp, [rax+1]
				// lea rcx, [rdi+rbp] <- Notice that the second operand has changed to the register rbp

				/// NOTE: The usage list defined above is not enough since it only captures the usages between the root instruction and the move instruction
				/// If you look at the example above, you can see that there can be usages of the source register even after the move instruction

				// Get all of the instructions which use the source register starting from the current intermediate instruction
				var start = instructions.IndexOf(intermediate);
				usages = TryGetUsagesForRelocation(instructions, source.To<RegisterHandle>().Register, start);

				if (usages == null) continue;

				// If the destination is used between the pivot and the intermediate, the redirection might be invalid
				if (usages.Any() && IsUsedBetween(instructions, start, instructions.IndexOf(usages.Last()), destination, pivot, true, true))
				{
					return false;
				}

				// Try to redirect the intermediate to the destination
				if (intermediate.Redirect(destination))
				{
					usages.ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
					intermediate.OnPostBuild();
					return true;
				}
			}

			if (root != null)
			{
				var start = instructions.IndexOf(root);
				usages = TryGetUsagesForRelocation(instructions, source.To<RegisterHandle>().Register, start);
				if (usages == null) return false;

				// If the destination is used between the pivot and the root, the redirection might be invalid
				/// NOTE: Why the code ensures there are no reads from the destination between the root and the pivot?
				// Before:                  After:
				// mov rax, rcx <- Root     mov rdx, rcx
				// mov rbx, rdx             mov rbx, rdx <- Notice the value might be different here
				// mov rdx, rax <- Pivot
				/// NOTE: Why the code ensures there are no writes to the destination between the root and the pivot?
				// Before:                  After:
				// mov rax, rcx <- Root     mov rcx, rcx
				// add rcx, 1               add rcx, 1 <- Notice the value might be different here
				// mov rcx, rax <- Pivot
				if (usages.Any() && IsUsedBetween(instructions, start + 1, instructions.IndexOf(usages.Last()), destination, pivot, true, true)) return false;

				// Try to redirect the intermediate to the destination
				if (!root.Redirect(destination)) return false;

				usages.ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
				root.OnPostBuild();
				return true;
			}

			return false;
		}

		if (reordered && !is_source_register)
		{
			// Since the instructions have been reordered and the source is not a register, inlining should be aborted
			// NOTE: Usages of the sources can not be loaded if it is a memory address
			return false;
		}

		// Example:
		// mov rdi, rdx (relocate)
		// mov rcx, rdi (copy)
		// ...
		// call ...
		// ...
		// mov rcx, [rdi]

		// Determine whether the pivot move instruction is a copy move instruction
		// 1. If the move is not a relocation move, it is considered to be a copy move
		// 2. If the instructions have been reordered, the relocation move is considered to be a copy move as well
		var is_pivot_copy_move = pivot.Type != MoveType.RELOCATE || reordered;

		if (is_pivot_copy_move && is_source_register)
		{
			/// NOTE: The usage list defined above is not enough since it only captures the usages between the root instruction and the move instruction
			var start = instructions.IndexOf(root);

			// Get all of the instructions which use the source register starting from the root instruction
			usages = TryGetUsagesForRelocation(instructions, source.To<RegisterHandle>().Register, start);

			// If the usages of the root instruction could not be collected, inlining should be aborted
			if (usages == null || is_destination_memory_address)
			{
				return false;
			}

			// The following situation is theoretical, but still should be checked for
			// Example:
			// add rcx, 1
			// add rdx, 1
			// mov rdx, rcx
			// Notice that the value inside the register rdx will be different in some cases
			// lea rdx, [rcx+1]
			// add rdx, 1
			if (usages.Any() && IsUsedBetween(instructions, start, instructions.IndexOf(usages.Last()), destination, pivot, true, true))
			{
				return false;
			}
		}

		// Try to redirect the root to the destination
		if (root.Redirect(destination))
		{
			usages.ForEach(i => i.Parameters.ForEach(j => Replace(j, source, destination)));
			root.OnPostBuild();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Tries execute moves by inlining them to another instructions
	/// </summary>
	private static void InlineMoveInstructions(Unit unit, List<Instruction> instructions, bool relocations, bool reordered)
	{
		for (var i = 0; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (!instruction.Is(InstructionType.MOVE) || instruction.To<MoveInstruction>().Condition != null)
			{
				continue;
			}

			if (relocations && instruction.To<MoveInstruction>().Type != MoveType.RELOCATE)
			{
				continue;
			}

			if (TryInlineMoveInstruction(unit, instructions[i].To<MoveInstruction>(), i, instructions, reordered))
			{
				instructions.RemoveAt(i);
				i--;
			}
		}
	}

	public static void Optimize(Unit unit, List<Instruction> instructions)
	{
		// Inline only the relocating moves
		InlineMoveInstructions(unit, instructions, false, true);

		//Remove all moves whose values are not read
		RemoveRedundantMoves(unit, instructions);

		// Try to inline moves for the first time
		InlineMoveInstructions(unit, instructions, false, true);

		// Reorder the instructions
		Reorder(instructions);

		// Remove all moves whose values are not read
		RemoveRedundantMoves(unit, instructions);

		InlineMoveInstructions(unit, instructions, false, true);

		unit.Reindex();

		Combine(unit, instructions);

		// Remove all redundant move instructions
		for (var i = instructions.Count - 1; i >= 0; i--)
		{
			var instruction = instructions[i];

			if (!instruction.Is(InstructionType.MOVE) || !instruction.Parameters.Any())
			{
				continue;
			}

			var destination = instruction.Destination!.Value!;
			var source = instruction.Source!.Value!;

			if (Equals(destination, source) && destination.Format == source.Format && instruction.Parameters.Count == 2)
			{
				instructions.RemoveAt(i);
			}
		}

		unit.Reindex();
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

		// Intersections are not possible with constant data section handles because they are read-only
		if (i.Is(HandleInstanceType.CONSTANT_DATA_SECTION) && j.Is(HandleInstanceType.CONSTANT_DATA_SECTION))
		{
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

			if (!instruction.Parameters.Any() || instruction.Destination == null) continue;

			var dependencies = instruction.Parameters.Select(i => i.Value!).Concat(GetAllInputRegisters(instruction).Select(i => new RegisterHandle(i))).ToArray();
			var destinations = instruction.Parameters.Where(i => i.Writes).ToArray();

			var position = i;

			while (true)
			{
				var obstacle = instructions[position - 1];

				// If the current instruction is dependent on the CPU-flags and the obstacle modifies them, the current instruction can not be placed above the obstacle
				if (Instructions.IsConditional(instruction) && Instructions.ModifiesFlags(obstacle))
				{
					break;
				}

				// If the obstacle represents an instruction which affects the execution heavily, this instruction should not be moved above it
				if (obstacle.Is(InstructionType.CALL, InstructionType.INITIALIZE, InstructionType.JUMP, InstructionType.LABEL, InstructionType.DIVISION, InstructionType.RETURN, InstructionType.REORDER))
				{
					break;
				}

				// 1. If this instruction writes to a destination which the obstacle uses, this instruction should not be moved above it
				// 2. If any of the dependencies of the current instruction are edited by the obstacle, this instruction should not be moved above it
				if (destinations.Any(i => Reads(obstacle, i.Value!)) || dependencies.Any(i => Writes(obstacle, i)))
				{
					break;
				}

				// Collect all memory addresses from the two instructions
				var instruction_memory_addresses = instruction.Parameters.Where(i => i.IsMemoryAddress).ToArray();
				var obstacle_memory_addresses = obstacle.Parameters.Where(i => i.IsMemoryAddress).ToArray();

				if (instruction_memory_addresses.Any() && obstacle_memory_addresses.Any())
				{
					// If any of the destination memory addresses can intersect with the memory addresses of the obstacle, this instruction should not be moved above the obstacle
					if (instruction_memory_addresses.Where(i => i.Writes).Select(i => i.Value!).Any(i => obstacle_memory_addresses.Any(j => IsIntersectionPossible(i, j.Value!))))
					{
						break;
					}

					// If any of the destination memory addresses of the obstacle can intersect with the memory addresses, this instruction should not be moved above the obstacle
					if (obstacle_memory_addresses.Where(i => i.Writes).Select(i => i.Value!).Any(i => instruction_memory_addresses.Any(j => IsIntersectionPossible(i, j.Value!))))
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
			return new Pair<Handle, Instruction?>(start.Source!.Value!, null);
		}

		var source = start.Source!.Value!;

		for (var i = position - 1; i >= 0; i--)
		{
			var instruction = instructions[i];

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

				var usages = TryGetUsagesForRelocation(instructions, source.To<RegisterHandle>().Register, i);

				if (usages == null || usages.Any(i => i != start))
				{
					return null;
				}

				return new Pair<Handle, Instruction?>(root, instruction);
			}
		}

		return null;
	}

	private static bool IsRegisterAvailable(List<Instruction> instructions, Register register, int position)
	{
		var handle = new RegisterHandle(register);

		for (var i = position; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (instruction.Is(InstructionType.CALL) && register.IsVolatile)
			{
				return !instruction.To<CallInstruction>().Destinations.Any(i => i.Equals(handle));
			}

			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL)) return false;

			if (Reads(instruction, handle)) return false;

			/// NOTE: It is important write-check comes after the read-check since intermediates can both read and write
			if (Writes(instruction, handle)) return true;
		}

		return true;
	}

	public static Register? TryGetNextMediaRegister(Unit unit, List<Instruction> instructions, int position)
	{
		return unit.VolatileMediaRegisters.Concat(unit.NonVolatileMediaRegisters).Where(i => IsRegisterAvailable(instructions, i, position)).FirstOrDefault();
	}

	public static Register? TryGetNextStandardRegister(Unit unit, List<Instruction> instructions, int position)
	{
		return unit.VolatileStandardRegisters.Concat(unit.NonVolatileStandardRegisters).Where(i => IsRegisterAvailable(instructions, i, position)).FirstOrDefault();
	}

	public static SequentialPair? TryGetSequentialPair(Handle first, Handle second)
	{
		// Ensure both of the handles are either memory addresses or constants
		if (first.Instance != second.Instance) return null;

		if (first.Is(HandleType.CONSTANT))
		{
			return new SequentialPair(first, second, false);
		}

		// Ensure both of the handles are memory addresses
		if (!first.Is(HandleType.MEMORY)) return null;

		var low = (Handle?)null;
		var high = (Handle?)null;
		var low_offset = 0L;
		var high_offset = 0L;

		if (first.Is(HandleInstanceType.MEMORY))
		{
			var a = first.To<MemoryHandle>();
			var b = second.To<MemoryHandle>();

			low = a;
			high = b;

			// Require that the handles have the same starting address
			if (!a.Start.Value.Equals(b.Start.Value))
			{
				return null;
			}

			low_offset = a.Offset;
			high_offset = b.Offset;
		}
		else if (first.Is(HandleInstanceType.STACK_MEMORY))
		{
			var a = first.To<StackMemoryHandle>();
			var b = second.To<StackMemoryHandle>();

			low = a;
			high = b;

			low_offset = a.Offset;
			high_offset = b.Offset;
		}
		else if (first.Is(HandleInstanceType.STACK_VARIABLE))
		{
			var a = first.To<StackVariableHandle>();
			var b = second.To<StackVariableHandle>();

			if ((a.Offset == 0 && a.IsAbsolute) || (b.Offset == 0 && b.IsAbsolute)) return null;

			low = a;
			high = b;

			low_offset = a.Offset;
			high_offset = b.Offset;
		}
		else if (first.Is(HandleInstanceType.COMPLEX_MEMORY))
		{
			var a = first.To<ComplexMemoryHandle>();
			var b = second.To<ComplexMemoryHandle>();

			low = a;
			high = b;

			// Require that the handles have the same starting address and stride
			if (!Equals(a.Start.Value, b.Start.Value) || a.Stride != b.Stride || !a.Index.IsConstant || !b.Index.IsConstant)
			{
				return null;
			}

			var x = a.Index.Value.To<ConstantHandle>();
			var y = b.Index.Value.To<ConstantHandle>();

			if (x.Format.IsDecimal() || y.Format.IsDecimal())
			{
				return null;
			}

			low_offset = (long)x.Value;
			high_offset = (long)y.Value;
		}
		else if (first.Is(HandleInstanceType.DATA_SECTION))
		{
			var a = first.To<DataSectionHandle>();
			var b = second.To<DataSectionHandle>();

			low = a;
			high = b;

			// Require that the handles have the same starting address
			if (a.Identifier != b.Identifier)
			{
				return null;
			}

			low_offset = a.Offset;
			high_offset = b.Offset;
		}
		else
		{
			return null;
		}

		var flipped = low_offset > high_offset;

		// Exchange the instruction so that the low actually represents the lower address
		if (flipped)
		{
			var temporary_handle = low;
			low = high;
			high = temporary_handle;

			var temporary_offset = low_offset;
			low_offset = high_offset;
			high_offset = temporary_offset;
		}

		// Ensure the moves are sequential
		if (low_offset + low.Size.Bytes != high_offset)
		{
			return null;
		}

		return new SequentialPair(low, high, flipped);
	}

	public static void Combine(Unit unit, List<Instruction> instructions)
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
			if (!first_move.Parameters.Any() || !first_move.Destination!.IsMemoryAddress)
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
				if (instructions[j].Is(InstructionType.CALL, InstructionType.JUMP, InstructionType.LABEL)) break;

				// Try to find the next move instruction
				if (!instructions[j].Is(InstructionType.MOVE)) continue;

				var second_move = instructions[j].To<MoveInstruction>();

				// The second move is found but its destination must be a memory address
				if (!second_move.Parameters.Any() || !second_move.Destination!.IsMemoryAddress) continue;

				var second_source = TryGetIndependentSource(instructions, second_move, j);

				// Skip this move if its source is used in multiple locations or if it is a register
				if (second_source == null || second_source.First.Is(HandleType.REGISTER)) continue;

				var destination = TryGetSequentialPair(first_move.Destination!.Value!, second_move.Destination!.Value!);
				var source = TryGetSequentialPair(first_source.First, second_source.First);

				// Ensure sequential destinations and sources were found and ensure the sizes match each other
				if (destination == null || source == null || destination.High.Size != destination.Low.Size || source.High.Size != source.Low.Size)
				{
					// If the moves might intersect even a little bit, both the first and the second move should be left alone
					if (IsIntersectionPossible(first_move.Destination.Value!, second_move.Destination.Value!)) break;

					continue;
				}

				// Ensure the order stays the same
				if (destination.Flipped != source.Flipped)
				{
					// Constants still can be flipped over
					if (source.Low.Type != HandleType.CONSTANT) continue;

					var temporary = source.High;
					source.High = source.Low;
					source.Low = temporary;
				}

				// Find all registers which the moves use since if one of them is edited the moves might become unsequential
				var dependencies = GetAllInputRegisters(first_move.Destination).Concat(GetAllInputRegisters(first_source.First));
				var start = first_source.Second == null ? i : instructions.IndexOf(first_source.Second);
				var end = j;

				if (dependencies.Any(i => IsUsedBetween(instructions, start, end, new RegisterHandle(i), null, true, false))) continue;

				var inline_destination_size = destination.Low.Size.Bytes * 2;
				var inline_source_size = source.Low.Size.Bytes * 2;

				// If the low part of the source is a constant, it means that the high is also a constant
				if (source.Low.Is(HandleType.CONSTANT))
				{
					if (inline_destination_size > 8) continue;

					var low_constant_value = source.Low.To<ConstantHandle>().Value as long?;
					var high_constant_value = source.High.To<ConstantHandle>().Value as long?;

					// Require both values to be integers
					if (low_constant_value == null || high_constant_value == null) continue;

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

					var load = (Instruction?)null;
					var store = (Instruction?)null;

					if (Assembler.IsArm64)
					{
						// Allow constants between 0-65535
						if (constant < 0 || constant > ushort.MaxValue) continue;

						// Both of the moves use registers to move the constant to the destination memory
						// Ensure both of the registers are not used after the moves
						var first_register = first_move.Source!.Value!.To<RegisterHandle>().Register;
						var second_register = second_move.Source!.Value!.To<RegisterHandle>().Register;

						if (IsUsedAfterwards(instructions, first_register, instructions.IndexOf(first_source.Second!), first_move) || IsUsedAfterwards(instructions, second_register, instructions.IndexOf(second_source.Second!), second_move))
						{
							continue;
						}

						if (first_source.Second == null || second_source.Second == null)
						{
							throw new ApplicationException("Constants were not loaded with instructions");
						}

						load = second_source.Second!;
						store = second_move;

						load.Destination!.Value = new RegisterHandle(first_register) { Format = Assembler.Format };
						load.Source!.Value = new ConstantHandle(constant) { Format = Assembler.Format };

						store.Source!.Value = new RegisterHandle(first_register) { Format = Size.FromBytes(inline_destination_size).ToFormat() };

						instructions.Remove(first_source.Second);
						instructions.Remove(first_move);
					}
					else
					{
						load = first_move;
						store = second_move;

						if (inline_destination_size == 8)
						{
							var register = TryGetNextStandardRegister(unit, instructions, i);

							if (register == null)
							{
								continue;
							}

							load.Destination!.Value = new RegisterHandle(register) { Format = Assembler.Format };
							load.Source!.Value = new ConstantHandle(constant) { Format = Assembler.Format };

							store.Source!.Value = new RegisterHandle(register) { Format = Size.FromBytes(inline_destination_size).ToFormat() };
						}
						else
						{
							store.Source!.Value = new ConstantHandle(constant) { Format = Size.FromBytes(inline_destination_size).ToFormat() };

							instructions.RemoveAt(i);
						}
					}

					load.OnPostBuild();

					store.Destination!.Value = destination.Low;
					store.Destination!.Value.Format = Size.FromBytes(inline_destination_size).ToFormat();

					store.OnPostBuild();

					/// NOTE: There will not be source instructions, so no need to remove them
					/// NOTE: Index i is incremented always after the following assignment
					i = -1;
					break;
				}
				else
				{
					/// NOTE: Both of the sources must be registers since the destination is a memory address
					var first_register = first_move.Source!.Value!.To<RegisterHandle>().Register;
					var second_register = second_move.Source!.Value!.To<RegisterHandle>().Register;

					if (IsUsedAfterwards(instructions, first_register, instructions.IndexOf(first_source.Second!), first_move) || IsUsedAfterwards(instructions, second_register, instructions.IndexOf(second_source.Second!), second_move))
					{
						continue;
					}

					var load = first_source.Second;
					var store = second_move;

					if (load == null || inline_source_size > 32 || inline_destination_size > 32)
					{
						continue;
					}

					var load_register = (Register?)null;
					var load_format = Assembler.Format;

					if (load.Destination!.IsAnyRegister)
					{
						load_register = load.Destination.Value!.To<RegisterHandle>().Register;

						// If the source size has grown past eight bytes, a media register is required
						if (inline_source_size > 8)
						{
							load_register = TryGetNextMediaRegister(unit, instructions, i);

							if (load_register != null)
							{
								load_format = Size.FromBytes(inline_source_size).ToFormat();
							}
						}
					}

					if (load_register == null) continue;

					// Arm does not have 256-bit registers
					if (Assembler.IsArm64 && inline_source_size == Size.YMMWORD.Bytes) continue;

					load.Destination.Value = new RegisterHandle(load_register) { Format = load_format };
					load.Destination.Size = Size.FromFormat(load_format);

					load.Source!.Value!.Format = Size.FromBytes(inline_source_size).ToFormat();
					load.Source!.Size = Size.FromBytes(inline_source_size);

					store.Destination!.Value = destination.Low;
					store.Destination!.Value.Format = Size.FromBytes(inline_destination_size).ToFormat();
					store.Destination!.Size = Size.FromBytes(inline_destination_size);

					store.Source!.Value = new RegisterHandle(load_register);
					store.Source!.Value!.Format = load_format;
					store.Source!.Size = Size.FromFormat(load_format);

					load.OnPostBuild();
					store.OnPostBuild();

					// Do not remove the first source
					first_source.Second = null;
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

				/// NOTE: Index i is incremented always after the following assignment
				i = -1;
				break;
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
		var dependencies = GetAllInputRegisters(destination).Select(i => new RegisterHandle(i)).ToArray();

		for (var i = position + 1; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			// If the instruction reads from the destination of the specified move instruction, the move instruction can not be redundant
			if (Reads(instruction, destination.Value!) || dependencies.Any(i => Writes(instruction, i))) return false;

			// If the instruction writes to the same destination as the specified move, the move must be redundant
			if (Writes(instruction, destination.Value!)) return true;

			// If any of the memory addresses in the instruction can intersect with the destination, the move instruction might not be redundant
			var instruction_memory_addresses = instruction.Parameters.Where(i => i.IsMemoryAddress).ToArray();

			if (destination.IsMemoryAddress && instruction_memory_addresses.Any(i => IsIntersectionPossible(i.Value!, destination.Value!)))
			{
				return false;
			}

			if (instruction.Is(InstructionType.REORDER))
			{
				// If the destination is a memory address, do not try to determine whether the move is redundant at this point
				if (!destination.IsAnyRegister) return false;

				// If any of the destinations is the destination register, the register is needed, therefore the move is not redundant
				if (instruction.To<ReorderInstruction>().Destinations.Where(i => i.Is(HandleInstanceType.REGISTER)).Cast<RegisterHandle>().Any(i => i.Register == destination.Value!.To<RegisterHandle>().Register)) return false;
			}

			if (instruction.Is(InstructionType.CALL))
			{
				if (destination.IsAnyRegister && destination.Value!.To<RegisterHandle>().Register.IsVolatile) return false;
			}

			// Do not analyze conditional execution
			if (instruction.Is(InstructionType.JUMP, InstructionType.LABEL))
			{
				return false;
			}
		}

		// The move can only be valid if its destination is either a memory address or a return register
		return !destination.IsMemoryAddress && (destination.IsAnyRegister && !IsReturnRegister(unit, destination.Value!));
	}

	/// <summary>
	/// Finds all the call instructions which have a return instruction after them.
	/// This function also ensures the calls do not use stack.
	/// </summary>
	private static int[] FindTailCalls(List<Instruction> instructions)
	{
		#warning Support reorder instruction
		var indices = new List<int>();

		for (var i = 0; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			// Firstly, require that the current instruction is a return instruction
			if (!instruction.Is(InstructionType.RETURN) || i - 1 < 0)
			{
				continue;
			}

			var previous = instructions[i - 1];

			// Require that the previous instruction is a call instruction
			if (!previous.Is(InstructionType.CALL)) continue;

			// Tail calls are not allowed to have parameters which are placed into stack
			/// NOTE: This is possible, but it would require adjusting the stack pointer of the parameter instructions
			if (previous.To<CallInstruction>().Destinations.Any(i => i.Is(HandleType.MEMORY)))
			{
				continue;
			}

			indices.Add(i - 1);
		}

		return indices.ToArray();
	}

	/// <summary>
	/// Finds all the tail calls in the specified instruction list and creates them.
	/// </summary>
	private static void CreateTailCalls(Unit unit, List<Instruction> instructions, List<Register> registers, int required_local_memory)
	{
		var initialization = instructions.Find(i => i.Is(InstructionType.INITIALIZE))!.To<InitializeInstruction>();
		var indices = FindTailCalls(instructions);

		// If there are any calls other than the tail calls, the tail calls can not be created
		for (var i = 0; i < instructions.Count; i++)
		{
			if (instructions[i].Is(InstructionType.CALL) && !indices.Contains(i)) return;
		}

		foreach (var i in indices)
		{
			var call = instructions[i];

			// Convert the call into a jump instruction
			if (Assembler.IsArm64)
			{
				call.Operation = call.Operation == Instructions.Arm64.CALL_LABEL ? Instructions.Arm64.JUMP_LABEL : Instructions.Arm64.JUMP_REGISTER;
			}
			else
			{
				call.Operation = Instructions.X64.JUMP;
			}

			// Swap the two instructions
			instructions.RemoveAt(i);
			instructions.Insert(i + 1, call);
		}

		// Reset the stack offset before building the initialization instruction
		unit.StackOffset = 0;

		// Rebuild the initialization instruction
		initialization.Build(registers, required_local_memory);
		
		var local_memory_top = initialization.LocalMemoryTop;

		// Reverse the saved registers since they must be recovered from stack when approaching the tail calls
		registers.Reverse();

		// Rebuild the return instructions
		for (var i = 0; i < instructions.Count; i++)
		{
			var instruction = instructions[i];

			if (!instruction.Is(InstructionType.RETURN)) continue;

			// Save the local memory size for later use
			unit.Function.SizeOfLocals = unit.StackOffset - local_memory_top;
			unit.Function.SizeOfLocalMemory = unit.Function.SizeOfLocals + registers.Count * Assembler.Size.Bytes;

			instruction.To<ReturnInstruction>().Build(registers, local_memory_top);

			// The actual return instruction can be removed from the return instructions which are linked to the tail calls, but not from other return instructions
			// NOTE: Now the indices point to the return instructions since the tail calls were created above
			if (indices.Contains(i))
			{
				instruction.To<ReturnInstruction>().RemoveReturnInstruction();
			}
		}
	}

	public static void RemoveRedundantJumps(Unit unit, List<Instruction> instructions)
	{
		// Remove jump instructions, which jump to labels directly in front of them
		// Example:
		// ...
		// jump L0
		// L0:
		// ...
		for (var i = instructions.Count - 2; i >= 0; i--)
		{
			if (!instructions[i].Is(InstructionType.JUMP)) continue;

			var label = instructions[i].To<JumpInstruction>().Label;
			if (!instructions[i + 1].Is(InstructionType.LABEL) || instructions[i + 1].To<LabelInstruction>().Label != label) continue;

			instructions.RemoveAt(i);
		}
	}

	public static void Finish(Unit unit, List<Instruction> instructions, List<Register> registers, int required_local_memory)
	{
		CreateTailCalls(unit, instructions, registers, required_local_memory);
		RemoveRedundantJumps(unit, instructions);
	}
}