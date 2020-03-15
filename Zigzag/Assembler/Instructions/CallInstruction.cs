public class CallInstruction : Instruction
{
    public string Function { get; private set; }

    public CallInstruction(Unit unit, string function) : base(unit)
    {
        Function = function;
        Result.Set(new RegisterHandle(Unit.GetStandardReturnRegister()));
    }

    public override void Build()
    {
        Unit.Append($"call {Function}");
    }

    public override Result? GetDestination()
    {
        return null;   
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.CALL;
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result };
    }
}