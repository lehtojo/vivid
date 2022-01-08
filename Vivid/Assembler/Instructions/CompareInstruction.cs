/// <summary>
/// This instruction compares the two specified values together and alters the CPU flags based on the comparison
/// This instruction is works on all architectures
/// </summary>
public class CompareInstruction : DualParameterInstruction
{
	public CompareInstruction(Unit unit, Result first, Result second) : base(unit, first, second, Assembler.Format, InstructionType.COMPARE)
	{
		Description = "Compares two values";
	}

	public override void OnBuild()
	{
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Instructions.X64.DOUBLE_PRECISION_COMPARE;

			if (Assembler.IsArm64)
			{
				instruction = Instructions.Arm64.DECIMAL_COMPARE;
			}

			Build(
				instruction,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.MEDIA_REGISTER
				)
			);
		}
		else if (Assembler.IsX64 && Second.IsConstant && Second.Value.To<ConstantHandle>().Value.Equals(0L))
		{
			Build(
				Instructions.X64.TEST,
				First.Size,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				)
			);
		}
		else
		{
			var types = Assembler.IsX64 ? new[] { HandleType.CONSTANT, HandleType.REGISTER, HandleType.MEMORY } : new[] { HandleType.CONSTANT, HandleType.REGISTER };
			var flags_second = ParameterFlag.NONE;

			if (Assembler.IsArm64)
			{
				flags_second |= ParameterFlag.CreateBitLimit(8);
			}

			Build(
				Instructions.Shared.COMPARE,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second,
					types
				)
			);
		}
	}
}