public class PassParameterInstruction : Instruction
{
    public Quantum<Handle> Value { get; private set; }
    
    public PassParameterInstruction(Quantum<Handle> value)
    {
        Value = value;
    }

    public override void Weld(Unit unit) {}

    public override void Build(Unit unit)
    {
        Build(
            unit, "push",
            new InstructionParameter(
                Value,
                false,
                HandleType.CONSTANT,
                HandleType.REGISTER,
                HandleType.STACK_MEMORY_HANDLE
            )
        );
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.PASS_PARAMETER;
    }

    public override Handle[] GetHandles()
    {
        return new Handle[] { Result.Value, Value.Value };
    }
}