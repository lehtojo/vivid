public class GetConstantInstruction : LoadInstruction
{
    public object Value { get; private set;}

    public GetConstantInstruction(Unit unit, object value) : base(unit, AccessMode.READ)
    {
        Value = value;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_CONSTANT;
    }

    public override Result? GetDestination()
    {
        return null;   
    }
}