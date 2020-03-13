public class MultiplicationInstruction : DualParameterInstruction
{
    public MultiplicationInstruction(Quantum<Handle> first, Quantum<Handle> second) : base(first, second) 
    {
        Result = first;
    }

    public override void Weld(Unit unit) {}

    public override void Build(Unit unit)
    {
        Build(
            unit, "imul",
            new InstructionParameter(
                First,
                true,
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                false,
                HandleType.CONSTANT,
                HandleType.REGISTER,
                HandleType.STACK_MEMORY_HANDLE
            )
        );
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MULTIPLICATION;
    }
}