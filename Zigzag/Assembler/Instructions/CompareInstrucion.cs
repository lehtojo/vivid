public class CompareInstruction : DualParameterInstruction
{
    public CompareInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Build()
    {
        Build(
            "cmp",
            new InstructionParameter(
                First,
                false,
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                false,
                HandleType.REGISTER,
                HandleType.CONSTANT,
                HandleType.STACK_MEMORY_HANDLE
            )
        );
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.COMPARE;
    }

    public override void RedirectTo(Handle handle)
    {
        First.Set(handle, true);
    }

    public override void Weld() {}
}