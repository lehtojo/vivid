using System;

/// <summary>
/// Represents single parameter instructions
/// This instruction works on all architectures
/// </summary>
public class SingleParameterInstruction : Instruction
{
	private const string SHARED_NEGATE_INSTRUCTION = "neg";
	private const string ARM64_DECIMAL_NEGATE_INSTRUCTION = "fneg";

	private const string X64_NOT_INSTRUCTION = "not";
	private const string ARM64_NOT_INSTRUCTION = "mvn";

	public string Instruction { get; private set; }
	public Result Target { get; private set; }

	public static SingleParameterInstruction Negate(Unit unit, Result target, bool is_decimal)
	{
		if (is_decimal && !Assembler.IsArm64)
		{
			throw new InvalidOperationException("Negating decimal value using single parameter instruction on architecture x64 is not allowed");
		}

		return new SingleParameterInstruction(unit, is_decimal ? ARM64_DECIMAL_NEGATE_INSTRUCTION : SHARED_NEGATE_INSTRUCTION, target)
		{
			Description = "Negates the target value"
		};
	}

	public static SingleParameterInstruction Not(Unit unit, Result target)
	{
		return new SingleParameterInstruction(unit, Assembler.IsArm64 ? ARM64_NOT_INSTRUCTION : X64_NOT_INSTRUCTION, target)
		{
			Description = "Performs bitwise not operation to the target value"
		};
	}

	private SingleParameterInstruction(Unit unit, string instruction, Result target) : base(unit, InstructionType.SINGLE_PARAMATER)
	{
		Instruction = instruction;
		Target = target;
		Dependencies = new[] { Result, Target };

		Result.Format = Target.Format;
	}

	public override void OnBuild()
	{
		Result.Format = Target.Format;

		if (Assembler.IsArm64)
		{
			var is_decimal = Target.Format.IsDecimal();
			var register_type = is_decimal ? HandleType.MEDIA_REGISTER : HandleType.REGISTER;

			Memory.GetResultRegisterFor(Unit, Result, is_decimal);

			Build(
				Instruction,
				Assembler.Size,
				new InstructionParameter(
					Result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS,
					register_type
				),
				new InstructionParameter(
					Target,
					ParameterFlag.NONE,
					register_type
				)
			);

			return;
		}

		Build(
			Instruction,
			Assembler.Size,
			new InstructionParameter(
				Target,
				ParameterFlag.DESTINATION | ParameterFlag.READS,
				HandleType.REGISTER
			)
		);
	}
}