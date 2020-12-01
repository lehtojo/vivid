
public class SingleParameterInstruction : Instruction
{
	private const string NEGATE_INSTRUCTION = "neg";
	private const string NOT_INSTRUCTION = "not";

	public string Instruction { get; private set; }
	public Result Target { get; private set; }

	public static SingleParameterInstruction Negate(Unit unit, Result target)
	{
		return new SingleParameterInstruction(unit, NEGATE_INSTRUCTION, target)
		{
			Description = "Negates the target value"
		};
	}

	public static SingleParameterInstruction Not(Unit unit, Result target)
	{
		return new SingleParameterInstruction(unit, NOT_INSTRUCTION, target)
		{
			Description = "Performs bitwise not operation to the target value"
		};
	}

	private SingleParameterInstruction(Unit unit, string instruction, Result target) : base(unit)
	{
		Instruction = instruction;
		Target = target;
		Result.Format = target.Format;
	}

	public override void OnBuild()
	{
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

	public override Result? GetDestinationDependency()
	{
		return Target;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.SINGLE_PARAMATER;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Target };
	}
}