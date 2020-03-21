public class ReturnInstruction : Instruction
{
    private const string RETURN = "ret";

    public Result Object { get; private set; }

    public ReturnInstruction(Unit unit, Result value) : base(unit)
    {
        Object = value;
    }

    private bool IsObjectInReturnRegister()
    {
        return Object.Value is RegisterHandle handle && handle.Register.IsReturnRegister;
    }

    private Result GetReturnRegister()
    {
        return new Result(new RegisterHandle(Unit.GetStandardReturnRegister()));
    }

    public override void Build()
    {
        if (!IsObjectInReturnRegister())
        {
            Unit.Build(new MoveInstruction(Unit, GetReturnRegister(), Object));
        }

        Unit.Append(RETURN);
    }

    public override Result GetDestinationDepency()
    {
        return Object;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.RETURN;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Object };
    }
}