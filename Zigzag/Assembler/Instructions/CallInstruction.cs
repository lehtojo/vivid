public class CallInstruction : Instruction
{
    public string Function { get; private set; }

    public CallInstruction(string function)
    {
        Function = function;
    }

    public override void Weld(Unit unit)
    {
        Result.Set(new RegisterHandle(unit.GetStandardReturnRegister()));
    }

    public override void Build(Unit unit)
    {
        unit.Append($"call {Function}");
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.CALL;
    }

    public override Handle[] GetHandles()
    {
        return new Handle[] { Result.Value };
    }
}