using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Returns the specified value to the caller by exiting the current function properly
/// This instruction works on all architectures
/// </summary>
public class ReturnInstruction : Instruction
{
	public Register ReturnRegister => (ReturnType != null && ReturnType.Format.IsDecimal()) ? Unit.GetDecimalReturnRegister() : Unit.GetStandardReturnRegister();
	private Handle ReturnRegisterHandle => new RegisterHandle(ReturnRegister);

	public Result? Object { get; private set; }
	public Type? ReturnType { get; private set; }

	public ReturnInstruction(Unit unit, Result? value, Type? return_type) : base(unit, InstructionType.RETURN)
	{
		Object = value;
		ReturnType = return_type;

		if (Object != null) { Dependencies!.Add(Object); }

		Result.Format = (ReturnType != null ? ReturnType.GetRegisterFormat() : Settings.Format);
	}

	/// <summary>
	/// Returns whether the return value is in the wanted return register
	/// </summary>
	private bool IsValueInReturnRegister()
	{
		return Object!.Value.Is(HandleInstanceType.REGISTER) && Object!.Value.To<RegisterHandle>().Register == ReturnRegister;
	}

	public override void OnBuild()
	{
		// 1. Skip if there is no return value
		// 2. Packs are handled separately
		// 3. Ensure the return value is in the correct register
		if (Object == null || (ReturnType != null && ReturnType.IsPack) || IsValueInReturnRegister()) return;

		Unit.Add(new MoveInstruction(Unit, new Result(ReturnRegisterHandle, ReturnType!.GetRegisterFormat()), Object)
		{
			Type = MoveType.RELOCATE
		});
	}

	private void RestoreRegistersArm64(StringBuilder builder, List<Register> registers)
	{
		// Example:
		// stp x0, x1, [sp, #-64]!
		// stp x2, x3, [sp, #16]
		// stp x4, x5, [sp, #32]
		// str x6, [sp, #48]

		// ldr x6, [sp, #48]
		// ldp x4, x5, [sp, #32]
		// ldp x2, x3, [sp, #16]
		// ldp x0, x1, [sp], #64

		if (!registers.Any()) { return; }

		var bytes = (registers.Count + 1) / 2 * 2 * Settings.Bytes;
		var stack_pointer = Unit.GetStackPointer();

		if (registers.Count == 1)
		{
			builder.AppendLine($"{Instructions.Arm64.LOAD} {registers.First()}, [{stack_pointer}], #{bytes}");
			return;
		}

		var standard_registers = registers.Where(i => !i.IsMediaRegister).ToList();
		var media_registers = registers.Where(i => i.IsMediaRegister).ToList();

		var position = registers.Count;

		if (media_registers.Count % 2 != 0 && media_registers.Any())
		{
			position--;

			if (!standard_registers.Any() && media_registers.Count == 1)
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD} {media_registers.First()}, [{stack_pointer}], #{bytes}");
			}
			else
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD} {media_registers.First()}, [{stack_pointer}, #{position * Settings.Bytes}]");
			}

			media_registers.Pop();
		}

		for (var i = 0; i < media_registers.Count;)
		{
			var batch = media_registers.Skip(i).Take(2).ToArray();

			position -= 2;

			if (!standard_registers.Any() && i + 2 == media_registers.Count)
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD_REGISTER_PAIR} {batch[0]}, {batch[1]}, [{stack_pointer}], #{bytes}");
			}
			else
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD_REGISTER_PAIR} {batch[0]}, {batch[1]}, [{stack_pointer}, #{position * Settings.Bytes}]");
			}

			i += 2;
		}

		if (standard_registers.Count % 2 != 0 && standard_registers.Any())
		{
			position--;

			if (standard_registers.Count == 1)
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD} {standard_registers.First()}, [{stack_pointer}], #{bytes}");
			}
			else
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD} {standard_registers.First()}, [{stack_pointer}, #{position * Settings.Bytes}]");
			}

			standard_registers.Pop();
		}

		for (var i = 0; i < standard_registers.Count;)
		{
			var batch = standard_registers.Skip(i).Take(2).ToArray();

			position -= 2;

			if (i + 2 == standard_registers.Count)
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD_REGISTER_PAIR} {batch[1]}, {batch[0]}, [{stack_pointer}], #{bytes}");
			}
			else
			{
				builder.AppendLine($"{Instructions.Arm64.LOAD_REGISTER_PAIR} {batch[1]}, {batch[0]}, [{stack_pointer}, #{position * Settings.Bytes}]");
			}

			i += 2;
		}
	}

	private static void RestoreRegistersX64(StringBuilder builder, List<Register> registers)
	{
		// Save all used non-volatile registers
		foreach (var register in registers)
		{
			builder.AppendLine($"{Instructions.X64.POP} {register}");
		}
	}

	/// <summary>
	/// Removes the return instruction from all the subinstructions
	/// </summary>
	public void RemoveReturnInstruction()
	{
		var i = Operation.LastIndexOf('\n');
		Operation = i == -1 ? string.Empty : Operation[0..i];
	}

	public void Build(List<Register> recover_registers, int local_variables_top)
	{
		var builder = new StringBuilder();
		var allocated_local_memory = Unit.StackOffset - local_variables_top;

		if (allocated_local_memory > 0)
		{
			var stack_pointer = Unit.GetStackPointer();

			if (Settings.IsX64)
			{
				builder.AppendLine($"{Instructions.Shared.ADD} {stack_pointer}, {allocated_local_memory}");
			}
			else
			{
				builder.AppendLine($"{Instructions.Shared.ADD} {stack_pointer}, {stack_pointer}, #{allocated_local_memory}");
			}
		}

		// Restore all used non-volatile registers
		if (Settings.IsX64)
		{
			RestoreRegistersX64(builder, recover_registers);
		}
		else
		{
			RestoreRegistersArm64(builder, recover_registers);
		}

		builder.Append(Instructions.Shared.RETURN);

		Build(builder.ToString());
	}
}