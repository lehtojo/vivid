public class AssignInstruction : DualParameterInstruction
{
    public AssignInstruction(Quantum<Handle> first, Quantum<Handle> second) : base(first, second) 
    {
        Result = second;
    }

    public override void Weld(Unit unit) {}

    public override void Build(Unit unit) {}

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ASSIGN;
    }
}