public enum AccessMode
{
    WRITE,
    READ
}

public abstract class LoadInstruction : Instruction
{
    public AccessMode Mode { get; private set; }
    public Result Source { get; private set; } = new Result(new Handle());

    public LoadInstruction(Unit unit, AccessMode mode) : base(unit) 
    {
        Mode = mode;
    }

    public void Connect(Result result)
    {
        Result.EntangleTo(result);
        Result.Instruction = result.Instruction;
        Source.EntangleTo(result);
    }

    public void SetSource(Handle handle)
    {
        Source.Value = handle;
        Result.Value = handle;
    }

    public override void Build()
    {
        if (Mode != AccessMode.WRITE && !Result.Value.Equals(Source.Value))
        {
            // Since the source is not where it should be, it must be moved to the result 
            Unit.Build(new MoveInstruction(Unit, Result, Source));
            Source.EntangleTo(Result);
        }
    }

    public override Result? GetDestinationDepency()
    {
        return Result;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}