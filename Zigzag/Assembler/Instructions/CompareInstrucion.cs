public class CompareInstruction : DualParameterInstruction
{
	public const string INSTRUCTION = "cmp";
	public const string ZERO_COMPARISON_INSTRUCTION = "test";

	public CompareInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

	public override void OnBuild()
	{
		if (Second.Value is ConstantHandle constant && constant.Value.Equals(0L))
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
				INSTRUCTION,
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