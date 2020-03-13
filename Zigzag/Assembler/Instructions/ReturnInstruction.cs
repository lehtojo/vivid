public class ReturnInstruction : Instruction
{
    public Quantum<Handle> Object { get; private set; }

    public ReturnInstruction(Quantum<Handle> value)
    {
        Object = value;
    }

    public override void Weld(Unit unit) 
    {
        Result.Set(new RegisterHandle(unit.GetStandardReturnRegister()));
    }

    public override void Build(Unit unit)
    {
        if (!(Object.Value is RegisterHandle handle && Flag.Has(handle.Register.Flags, RegisterFlag.RETURN)))
        {
            unit.Build(new MoveInstruction(Result, Object));
        }
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.RETURN;
    }

    public override Handle[] GetHandles()
    {
        return new Handle[] { Result.Value, Object.Value };
    }
}