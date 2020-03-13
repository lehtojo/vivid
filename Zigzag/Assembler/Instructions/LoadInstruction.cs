public abstract class LoadInstruction : Instruction
{
    public override void Build(Unit unit)
    {
        if (Result.Value is RegisterHandle handle)
        {
            handle.Register.Value = Result;
        }
    }

    public override Handle[] GetHandles()
    {
        return new Handle[] { Result.Value };
    }
}