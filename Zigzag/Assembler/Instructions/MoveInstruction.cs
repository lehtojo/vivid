public class MoveInstruction : DualParameterInstruction
{
    public const string INSTRUCTION = "mov";

    public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Build()
    {
        // Move shouldn't happen if the source is the same as the destination
        if (First.Value.Equals(Second.Value)) return;

        if (First.Value.Type == HandleType.MEMORY_HANDLE)
        {
            Build(
                INSTRUCTION,
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER,
                    HandleType.MEMORY_HANDLE
                ),
                new InstructionParameter(
                    Second,
                    false,
                    HandleType.CONSTANT,
                    HandleType.REGISTER
                )
            );
        }
        else
        {
            Build(
                INSTRUCTION,
                new InstructionParameter(
                    First,
                    true,
                    HandleType.REGISTER,
                    HandleType.MEMORY_HANDLE
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

        // Attach the moved to the destination register
        if (First.Value is RegisterHandle destination)
        {
            destination.Register.Value = Second;
        }
    }

    public override Result GetDestination()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MOVE;
    }
}