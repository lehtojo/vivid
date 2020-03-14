public abstract class LoadInstruction : Instruction
{
    public LoadInstruction(Unit unit) : base(unit) {}

    public void Connect(Result result)
    {
        Result.EntangleTo(result);
        Result.Instruction = result.Instruction;
    }

    public override void Build()
    {
        if (Result.Value is RegisterHandle handle)
        {
            //handle.Register.Value = Result;
        }
    }

    public override void RedirectTo(Handle handle)
    {
        Result.Set(handle, true);
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result };
    }
}