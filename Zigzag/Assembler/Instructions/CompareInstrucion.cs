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
                HandleType.MEMORY_HANDLE
            )
        );
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.COMPARE;
    }

    public override Result? GetDestination()
    {
        return null;   
    }
}