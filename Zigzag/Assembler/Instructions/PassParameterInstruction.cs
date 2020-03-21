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
                ParameterFlag.NONE,
                HandleType.CONSTANT,
                HandleType.REGISTER,
                HandleType.MEMORY_HANDLE
            )
        );
    }

    public override Result? GetDestinationDepency()
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