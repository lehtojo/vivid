public class ReturnInstruction : Instruction
{
    public Result Object { get; private set; }

    public ReturnInstruction(Unit unit, Result value) : base(unit)
    {
        Object = value;
    }

    public override void Weld() 
    {
        Result.Set(new RegisterHandle(Unit.GetStandardReturnRegister()), true);
    }

    public override void RedirectTo(Handle handle)
    {
        Object.Set(handle, true);
        //Result.Set(handle, true);
    }

    public override void Build()
    {
        if (!(Object.Value is RegisterHandle handle && Flag.Has(handle.Register.Flags, RegisterFlag.RETURN)))
        {
            Unit.Build(new MoveInstruction(Unit, Result, Object));
        }
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.RETURN;
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result, Object };
    }
}