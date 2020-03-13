
public class GetVariableInstruction : LoadInstruction
{
    public Variable Variable { get; private set; }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_VARIABLE;
    }

    public GetVariableInstruction(Variable variable)
    {
        Variable = variable;
    }
    
    public override void Weld(Unit unit) {}
}