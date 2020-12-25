/// <summary>
/// Ensures that the specified variable has a location in the current scope
/// This instruction is works in all architectures
/// </summary>
public class DeclareInstruction : Instruction
{
	public Variable Variable { get; }

	public DeclareInstruction(Unit unit, Variable variable) : base(unit)
	{
		Variable = variable;
	}

	public override void OnBuild()
	{
		var media_register = Variable.GetRegisterFormat().IsDecimal();
		var register = Memory.GetNextRegister(Unit, media_register, Result.GetRecommendation(Unit));

		Result.Value = new RegisterHandle(register);
		Result.Format = Variable.GetRegisterFormat();

		Build(
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION,
				media_register ? HandleType.MEDIA_REGISTER : HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(),
				ParameterFlag.NONE,
				HandleType.NONE
			)
		);
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.DECLARE;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}
}