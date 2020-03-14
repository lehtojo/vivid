
public class GetVariableInstruction : LoadInstruction
{
    public Variable Variable { get; private set; }

    public GetVariableInstruction(Unit unit, Variable variable) : base(unit)
    {
        Variable = variable;
    }
    
    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_VARIABLE;
    }

    public override void Weld() {}
}