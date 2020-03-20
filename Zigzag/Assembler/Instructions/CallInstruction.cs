public class CallInstruction : Instruction
{
    public string Function { get; private set; }

    public CallInstruction(Unit unit, string function) : base(unit)
    {
        Function = function;
    }

    public override void Build()
    {
        Unit.Append($"call {Function}");

        // Returns value is always in the following handle
        var handle = new RegisterHandle(Unit.GetStandardReturnRegister());

        if (Result.Empty)
        {
            // The result is not predefined so the result can just hold the standard return register
            Result.Set(handle);
        }
        else
        {
            var source = new Result(handle);
            source.Lifetime.Start = Result.Lifetime.Start;
            source.Lifetime.End = Result.Lifetime.End;
            
            // The result is predefined so the value from the source handle must be moved to the predefined result
            Unit.Build(new MoveInstruction(Unit, Result, source));
        }
    }

    public override Result GetDestination()
    {
        return Result;   
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.CALL;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}