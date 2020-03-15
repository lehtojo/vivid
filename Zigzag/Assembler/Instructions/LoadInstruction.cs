using System;

public abstract class LoadInstruction : Instruction
{
    private Result Source { get; set; } = new Result(new Handle());

    public LoadInstruction(Unit unit) : base(unit) {}

    public void Connect(Result result)
    {
        Result.EntangleTo(result);
        Result.Instruction = result.Instruction;
        Source.EntangleTo(result);
    }

    public void SetSource(Handle handle)
    {
        Source.Set(handle);
        Result.Set(handle);
    }

    public override void Build()
    {
        if (Result.Value != Source.Value)
        {
            Unit.Build(new MoveInstruction(Unit, Result, Source));
        }
    }

    public override Result? GetDestination()
    {
        return Result;
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result };
    }
}