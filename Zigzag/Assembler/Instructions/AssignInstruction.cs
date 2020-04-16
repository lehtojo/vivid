public class AssignInstruction : DualParameterInstruction
{
    public AssignInstruction(Unit unit, Result first, Result second) : base(unit, first, second) 
    {
        Result.Join(Second);
    }

    public override void Build() 
    {
        if (!Unit.Optimize ||
            First.Metadata.IsComplexMemoryAddress || 
            First.Metadata.PrimaryAttribute is VariableAttribute attribute && 
            !attribute.Variable.IsPredictable)
        {
            Unit.Append(new MoveInstruction(Unit, First, Second));
        }
    }

    public override Result? GetDestinationDependency()
    {
        return null;   
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ASSIGN;
    }
}