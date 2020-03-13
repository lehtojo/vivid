public class GetConstantInstruction : LoadInstruction
{
    public object Value { get; private set;}

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_CONSTANT;
    }

    public GetConstantInstruction(object value)
    {
        Value = value;
    }

    public override void Weld(Unit unit) {}
}