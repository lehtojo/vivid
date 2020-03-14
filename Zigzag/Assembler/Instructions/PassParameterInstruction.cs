public class PassParameterInstruction : Instruction
{
    public Result Value { get; private set; }
    
    public PassParameterInstruction(Unit unit, Result value) : base(unit)
    {
        Value = value;
    }

    public override void Weld() {}

    public override void Build()
    {
        Build(
            "push",
            new InstructionParameter(
                Value,
                false,
                HandleType.CONSTANT,
                HandleType.REGISTER,
                HandleType.STACK_MEMORY_HANDLE
            )
        );
    }

    public override void RedirectTo(Handle handle)
    {
        Result.Set(handle, true);
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.PASS_PARAMETER;
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result, Value };
    }
}