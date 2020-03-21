public class CompareInstruction : DualParameterInstruction
{
    public CompareInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Build()
    {
        Build(
            "cmp",
            new InstructionParameter(
                First,
                ParameterFlag.NONE,
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                ParameterFlag.NONE,
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

    public override Result? GetDestinationDepency()
    {
        return null;   
    }
}