using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

/// <summary>
/// Initializes the functions by handling the stack properly
/// This instruction works on all architectures
/// </summary>
public class InitializeInstruction : Instruction
{
	public const string DEBUG_CANOCICAL_FRAME_ADDRESS_OFFSET = ".cfi_def_cfa_offset ";

	public int LocalMemoryTop { get; private set; }
	private static bool IsShadowSpaceRequired => Assembler.IsTargetWindows;

	public InitializeInstruction(Unit unit) : base(unit, InstructionType.INITIALIZE) { }

	private static int GetRequiredCallMemory(CallInstruction[] calls)
	{
		if (!calls.Any()) return 0;

		// Find all parameter move instructions which move the source value into memory
		var parameter_memory_offsets = calls.SelectMany(i => i.Destinations).Where(i => i.Is(HandleType.MEMORY)).Select(i => i.To<MemoryHandle>().Offset).ToArray();

		if (!parameter_memory_offsets.Any())
		{
			// Even though no instruction writes to memory, on Windows x64 there is a requirement to allocate so called 'shadow space' for the first four parameters
			if (IsShadowSpaceRequired) return Calls.SHADOW_SPACE_SIZE;
			return 0;
		}

		return parameter_memory_offsets.Max() + Assembler.Size.Bytes;
	}

	private void SaveRegistersArm64(StringBuilder builder, List<Register> registers)
	{
		if (!registers.Any()) return;

		var stack_pointer = Unit.GetStackPointer();
		var bytes = (registers.Count + 1) / 2 * 2 * Assembler.Size.Bytes;

		Unit.StackOffset += bytes;

		if (registers.Count == 1)
		{
			builder.AppendLine($"{Instructions.Arm64.STORE} {registers.First()}, [{stack_pointer}, #{-bytes}]!");
			return;
		}

		var standard_registers = registers.Where(i => !i.IsMediaRegister).ToArray();
		var media_registers = registers.Where(i => i.IsMediaRegister).ToArray();

		var position = 0;
		var allocated = false;

		while (position < standard_registers.Length)
		{
			var batch = standard_registers.Skip(position).Take(2).ToArray();

			if (batch.Length == 1)
			{
				if (!allocated)
				{
					builder.AppendLine($"{Instructions.Arm64.STORE} {batch.First()}, [{stack_pointer}, #{-bytes}]!");
					allocated = true;
				}
				else
				{
					builder.AppendLine($"{Instructions.Arm64.STORE} {batch.First()}, [{stack_pointer}, #{position * Assembler.Size.Bytes}]");
				}

				position++;
				continue;
			}

			if (!allocated)
			{
				builder.AppendLine($"{Instructions.Arm64.STORE_REGISTER_PAIR} {batch[0]}, {batch[1]}, [{stack_pointer}, #{-bytes}]!");
				allocated = true;
			}
			else
			{
				builder.AppendLine($"{Instructions.Arm64.STORE_REGISTER_PAIR} {batch[0]}, {batch[1]}, [{stack_pointer}, #{position * Assembler.Size.Bytes}]");
			}

			position += 2;
		}

		for (var i = 0; i < media_registers.Length;)
		{
			var batch = media_registers.Skip(i).Take(2).ToArray();

			if (batch.Length == 1)
			{
				if (!allocated)
				{
					builder.AppendLine($"{Instructions.Arm64.STORE} {batch.First()}, [{stack_pointer}, #{-bytes}]!");
					allocated = true;
				}
				else
				{
					builder.AppendLine($"{Instructions.Arm64.STORE} {batch.First()}, [{stack_pointer}, #{position * Assembler.Size.Bytes}]");
				}

				i++;
				position++;
				continue;
			}

			if (!allocated)
			{
				builder.AppendLine($"{Instructions.Arm64.STORE_REGISTER_PAIR} {batch[0]}, {batch[1]}, [{stack_pointer}, #{-bytes}]!");
				allocated = true;
			}
			else
			{
				builder.AppendLine($"{Instructions.Arm64.STORE_REGISTER_PAIR} {batch[0]}, {batch[1]}, [{stack_pointer}, #{position * Assembler.Size.Bytes}]");
			}

			i += 2;
			position += 2;
		}
	}

	private void SaveRegistersX64(StringBuilder builder, List<Register> registers)
	{
		// Save all used non-volatile registers
		foreach (var register in registers)
		{
			builder.AppendLine($"{Instructions.X64.PUSH} {register}");
			Unit.StackOffset += Assembler.Size.Bytes;
		}
	}

	public void Build(List<Register> save_registers, int required_local_memory)
	{
		// Collect all normal call instructions
		var calls = Unit.Instructions.FindAll(i => i.Is(InstructionType.CALL)).Cast<CallInstruction>().Where(i => !i.IsTailCall).ToArray();

		var builder = new StringBuilder();

		if (Assembler.IsDebuggingEnabled)
		{
			builder.Append(Assembler.DebugFunctionStartDirective);

			if (!Assembler.IsLegacyAssemblyEnabled)
			{
				// If the legacy assembly is not enabled, write the symbol, which represents the start of the current function
				builder.Append(' ');
				builder.Append(Unit.Function.GetFullname());
			}

			builder.AppendLine();
			builder.AppendLine(AppendPositionInstruction.GetPositionInstruction(Unit.Function.Metadata!.Start!));
		}

		if (Assembler.IsArm64 && calls.Any())
		{
			save_registers.Add(Unit.GetReturnAddressRegister());
		}

		// Save all used non-volatile registers
		if (Assembler.IsX64)
		{
			SaveRegistersX64(builder, save_registers);
		}
		else
		{
			SaveRegistersArm64(builder, save_registers);
		}

		// Local variables in memory start now
		LocalMemoryTop = Unit.StackOffset;

		// Apply the required memory for local variables
		var additional_memory = required_local_memory;

		// Allocate memory for calls
		additional_memory += GetRequiredCallMemory(calls);

		// Apply the additional memory to the stack and calculate the change from the start
		Unit.StackOffset += additional_memory;

		// If there are calls, it means they will also push the return address to the stack, which must be taken into account when aligning the stack
		var total = Unit.StackOffset + (Assembler.IsX64 && calls.Any() ? Assembler.Size.Bytes : 0);

		if (total != 0 && total % Calls.STACK_ALIGNMENT != 0)
		{
			// Apply padding to the memory to make it aligned
			var padding = Calls.STACK_ALIGNMENT - (total % Calls.STACK_ALIGNMENT);

			Unit.StackOffset += padding;
			additional_memory += padding;
		}

		if (additional_memory > 0)
		{
			var stack_pointer = Unit.GetStackPointer();

			if (Assembler.IsX64)
			{
				builder.AppendLine($"{Instructions.Shared.SUBTRACT} {stack_pointer}, {additional_memory}");
			}
			else
			{
				builder.AppendLine($"{Instructions.Shared.SUBTRACT} {stack_pointer}, {stack_pointer}, #{additional_memory}");
			}
		}

		// When debugging mode is enabled, the current stack pointer should be saved to the base pointer
		if (Assembler.IsDebuggingEnabled)
		{
			builder.Append(Assembler.DebugFrameOffsetDirective);
			builder.Append(' ');
			builder.AppendLine((Unit.StackOffset + Assembler.Size.Bytes).ToString(CultureInfo.InvariantCulture));
		}

		Build(builder.ToString().TrimEnd());
	}
}