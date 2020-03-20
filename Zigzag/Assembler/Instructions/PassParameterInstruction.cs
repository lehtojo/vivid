public class PassParameterInstruction : Instruction
{
    public Result Value { get; private set; }
    
    public PassParameterInstruction(Unit unit, Result value) : base(unit)
    {
        Value = value;
    }

    public override void Build()
    {
        Build(
            "push",
            new InstructionParameter(
                Value,
                false,
                HandleType.CONSTANT,
                HandleType.REGISTER,
                HandleType.MEMORY_HANDLE
            )
        );
    }

    public override Result? GetDestination()
    {
        return null;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.PASS_PARAMETER;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Value };
    }
}