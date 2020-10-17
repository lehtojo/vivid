public class CompareInstruction : DualParameterInstruction
{
	public const string COMPARISON_INSTRUCTION = "cmp";
	public const string SINGLE_PRECISION_DECIMAL_COMPARISON = "comiss";
	public const string DOUBLE_PRECISION_DECIMAL_COMPARISON = "comisd";

	public const string ZERO_COMPARISON_INSTRUCTION = "test";

	public CompareInstruction(Unit unit, Result first, Result second) : base(unit, first, second, Assembler.Format) { }

	public override void OnBuild()
	{
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			Build(
				Assembler.IsTargetX64 ? DOUBLE_PRECISION_DECIMAL_COMPARISON : SINGLE_PRECISION_DECIMAL_COMPARISON,
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
		else if (Second.Value is ConstantHandle constant && constant.Value.Equals(0L))
		{
			Build(
				ZERO_COMPARISON_INSTRUCTION,
				Assembler.Size,
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
			Build(
				COMPARISON_INSTRUCTION,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.REGISTER,
					HandleType.CONSTANT,
					HandleType.MEMORY
				)
			);
		}
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.COMPARE;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}
}