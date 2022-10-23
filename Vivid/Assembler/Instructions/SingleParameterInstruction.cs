using System;
using System.Collections.Generic;

/// <summary>
/// Represents single parameter instructions
/// This instruction works on all architectures
/// </summary>
public class SingleParameterInstruction : Instruction
{
	public string Instruction { get; private set; }
	public Result First { get; private set; }

	public static SingleParameterInstruction Negate(Unit unit, Result first, bool is_decimal)
	{
		if (is_decimal && !Settings.IsArm64)
		{
			throw new InvalidOperationException("Negating decimal value using single parameter instruction on architecture x64 is not allowed");
		}

		return new SingleParameterInstruction(unit, is_decimal ? Instructions.Arm64.DECIMAL_NEGATE : Instructions.Shared.NEGATE, first)
		{
			Description = "Negates the target value"
		};
	}

	public static SingleParameterInstruction Not(Unit unit, Result target)
	{
		return new SingleParameterInstruction(unit, Settings.IsArm64 ? Instructions.Arm64.NOT : Instructions.X64.NOT, target)
		{
			Description = "Performs bitwise not operation to the target value"
		};
	}

	private SingleParameterInstruction(Unit unit, string instruction, Result target) : base(unit, InstructionType.SINGLE_PARAMETER)
	{
		Instruction = instruction;
		First = target;
		Dependencies = new List<Result> { Result, First };

		Result.Format = First.Format;
	}

	public override void OnBuild()
	{
		Result.Format = First.Format;

		if (Settings.IsArm64)
		{
			var is_unsigned = First.Format.IsUnsigned();
			var is_decimal = First.Format.IsDecimal();
			var register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;

			Memory.GetResultRegisterFor(Unit, Result, is_unsigned, is_decimal);

			Build(
				Instruction,
				Settings.Size,
				new InstructionParameter(
					Result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
					register_type
				),
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					register_type
				)
			);

			return;
		}

		Build(
			Instruction,
			Settings.Size,
			new InstructionParameter(
				First,
				ParameterFlag.DESTINATION | ParameterFlag.READS,
				HandleType.REGISTER
			)
		);
	}
}