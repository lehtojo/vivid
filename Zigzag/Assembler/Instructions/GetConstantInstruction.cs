public class GetConstantInstruction : LoadInstruction
{
	public object Value { get; private set;}

	public GetConstantInstruction(Unit unit, object value, Format format) : base(unit, AccessMode.READ)
	{
		Value = value;
		Description = $"Get a handle for constant '{value}'";

		Configure(References.CreateConstantNumber(value));

		Result.Format = format;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_CONSTANT;
	}

	public override Result? GetDestinationDependency()
	{
		return null;   
	}
}