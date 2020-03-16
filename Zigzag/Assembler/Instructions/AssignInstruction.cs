public class AssignInstruction : DualParameterInstruction
{
    public AssignInstruction(Unit unit, Result first, Result second) : base(unit, first, second) 
    {
        Result.EntangleTo(Second);
    }

    public override void Build() 
    {
        if (First.Metadata is Variable variable && variable.Category == VariableCategory.MEMBER)
        {
            Unit.Build(new MoveInstruction(Unit, First, Second));
        }
    }

    public override Result? GetDestination()
    {
        return null;   
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ASSIGN;
    }
}