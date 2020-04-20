public enum AccessMode
{
    WRITE,
    READ
}

public abstract class LoadInstruction : Instruction
{
    public AccessMode Mode { get; private set; }
    public Result Source { get; set; } = new Result();

    public LoadInstruction(Unit unit, AccessMode mode) : base(unit) 
    {
        Mode = mode;
    }

    public void Connect(Result result)
    {
        Result.Join(result);
        Source.Join(result);
    }

    public void SetSource(Handle handle, MetadataAttribute? attribute = null)
    {
        Source.Value = handle;
        Result.Value = handle;

        if (attribute != null)
        {
            Source.Metadata.Attach(attribute);
            Result.Metadata.Attach(attribute);
        }
    }

    public override void Build()
    {
        if (Mode != AccessMode.WRITE && !Result.Value.Equals(Source.Value))
        {
            var move = new MoveInstruction(Unit, Result, Source);
            move.Mode = MoveMode.LOAD;

            // Since the source is not where it should be, it must be moved to the result 
            Unit.Append(move);
        }
    }

    public override Result? GetDestinationDependency()
    {
        return Result;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}