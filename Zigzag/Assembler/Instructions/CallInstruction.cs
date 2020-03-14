public class CallInstruction : Instruction
{
    public string Function { get; private set; }

    public CallInstruction(Unit unit, string function) : base(unit)
    {
        Function = function;
    }

    public override void Weld()
    {
        Result.Set(new RegisterHandle(Unit.GetStandardReturnRegister()));
    }

    public override void Build()
    {
        Unit.Append($"call {Function}");
    }

    public override void RedirectTo(Handle handle)
    {
        Result.Set(handle, true);
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