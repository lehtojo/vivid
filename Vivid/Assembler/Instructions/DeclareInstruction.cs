/// <summary>
/// Ensures that the specified variable has a location in the current scope
/// This instruction is works on all architectures
/// </summary>
public class DeclareInstruction : Instruction
{
	public Variable Variable { get; }

	public DeclareInstruction(Unit unit, Variable variable) : base(unit, InstructionType.DECLARE)
	{
		Variable = variable;
	}

	public override void OnBuild()
	{
		var media_register = Variable.GetRegisterFormat().IsDecimal();
		var register = Memory.GetNextRegister(Unit, media_register, Trace.GetDirectives(Unit, Result));

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
}