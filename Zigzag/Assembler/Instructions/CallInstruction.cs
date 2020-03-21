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
            Result.Value = handle;
        }
        else
        {
            var move = new MoveInstruction(Unit, Result, new Result(handle));
            
            // Configure the move so that this instruction's result is attached to the destination
            move.Loads = true;

            // The result is predefined so the value from the source handle must be moved to the predefined result
            Unit.Build(move);
        }
    }

    public override Result GetDestinationDepency()
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