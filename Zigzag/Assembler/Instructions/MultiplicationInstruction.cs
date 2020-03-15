public class MultiplicationInstruction : DualParameterInstruction
{
    public MultiplicationInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Weld() {}

    public override void Build()
    {
        Build(
            "imul",
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
                HandleType.MEMORY_HANDLE
            )
        );
    }

    public override void RedirectTo(Handle handle)
    {
        First.Set(handle, true);
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MULTIPLICATION;
    }
}