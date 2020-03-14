public class GetConstantInstruction : LoadInstruction
{
    public object Value { get; private set;}

    public GetConstantInstruction(Unit unit, object value) : base(unit)
    {
        Value = value;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_CONSTANT;
    }

    public override void Weld() {}
}