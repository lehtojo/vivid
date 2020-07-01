using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;

public class InitializeInstruction : Instruction
{
	public int StackMemoryChange { get; private set; }
	public int LocalMemoryTop { get; private set; }

	private static bool IsShadowSpaceRequired => Assembler.IsTargetWindows && Assembler.IsTargetX64;
	private static bool IsStackAligned => Assembler.IsTargetX64;

	public InitializeInstruction(Unit unit) : base(unit) {}

	public override void OnBuild() {}

	private static int GetRequiredCallMemory(IEnumerable<CallInstruction> calls)
	{
		if (!calls.Any())
		{
			return 0;
		}

		// Find all parameter move instructions which move the source value into memory
		var parameter_instructions = calls.SelectMany(c => c.ParameterInstructions)
			.Where(i => i.Type == InstructionType.MOVE).Select(i => (MoveInstruction)i)
			.Where(m => m.Destination?.IsMemoryAddress ?? false);

		if (!parameter_instructions.Any())
		{
			if (IsShadowSpaceRequired)
			{
				// Even though no instruction writes to memory, on Windows x64 there's a requirement to allocate so called 'shadow space' for the first four parameters
				return Calls.SHADOW_SPACE_SIZE;
			}
			else
			{
				return 0;
			}
		}

		// Find the memory handle which has the greatest offset, that tells how much memory should be allocated for calls
		return parameter_instructions.Select(i => i.Destination!.Value!.To<MemoryHandle>().Offset).Max();
	}

	public void Build(List<Register> save_registers, int required_local_memory)
	{
		var builder = new StringBuilder();
		var start = Unit.StackOffset;

		// Save all used non-volatile rgisters
		foreach (var register in save_registers)
		{
			builder.AppendLine($"push {register}");
			Unit.StackOffset += Assembler.Size.Bytes;
		}

		// Local variables in memory start now
		LocalMemoryTop = Unit.StackOffset;

		// Apply the required memory for local variables
		var additional_memory = required_local_memory;

		// Allocate memory for calls
		var calls = Unit.Instructions.FindAll(i => i.Type == InstructionType.CALL).Select(i => (CallInstruction)i);
		additional_memory += GetRequiredCallMemory(calls);

		// Apply the additional memory to the stack and calculate the change from the start
		Unit.StackOffset += additional_memory;
		StackMemoryChange = Unit.StackOffset - start;

		// Microsoft x64 and some other calling conventions need the stack to be aligned
		if (IsStackAligned)
		{
			// If there are calls, it means they will also push the return address to the stack, which must be taken into account when aligning the stack
			var total = StackMemoryChange + (calls.Any() ? Assembler.Size.Bytes : 0);

			if (total != 0)
			{
				// Apply padding to the memory to make it aligned
				var padding = Calls.STACK_ALIGNMENT - (total % Calls.STACK_ALIGNMENT);

				Unit.StackOffset += padding;
				additional_memory += padding;
			}	
		}

		if (additional_memory > 0)
		{
			builder.Append($"sub {Unit.GetStackPointer()}, {additional_memory}");
		}
		else if (save_registers.Count > 0)
		{
			// Remove the last line ending
			builder.Remove(builder.Length - 1, 1);
		}

		StackMemoryChange = Unit.StackOffset - start;
		Unit.StackOffset = start; // Instruction system will take care of the stack offset

		Build(builder.ToString());
	}

	public override int GetStackOffsetChange()
	{
		return StackMemoryChange;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.INITIALIZE;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}
}