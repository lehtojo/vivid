
public class NegateInstruction : Instruction
{
	public const string INSTRUCTION = "neg";

	public Result Target { get; private set; }

	public NegateInstruction(Unit unit, Result target) : base(unit)
	{
		Target = target;
	}

	public override void OnBuild()
	{
		Build(
			INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				Target,
				ParameterFlag.DESTINATION,
				HandleType.REGISTER,
				HandleType.MEMORY
			)
		);
	}

	public override Result? GetDestinationDependency()
	{
		return Target;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.NEGATE;
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result, Target };
	}
}