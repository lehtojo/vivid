public class AssignInstruction : DualParameterInstruction
{
    public AssignInstruction(Unit unit, Result first, Result second) : base(unit, first, second) 
    {
        Result.EntangleTo(Second);
    }

    public override void Weld() 
    {
    }

    public override void Build() {}

    public override void RedirectTo(Handle handle)
    {
        Result.Set(handle, true);
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ASSIGN;
    }
}