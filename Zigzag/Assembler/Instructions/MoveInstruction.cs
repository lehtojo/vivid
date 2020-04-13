public enum MoveMode
{
    /// <summary>
    /// The source value is loaded to the destination attaching the source value to the destination and leaving the source untouched
    /// </summary>
    COPY,
    /// <summary>
    /// The source value is loaded to destination attaching the destination value to the destination
    /// </summary>
    LOAD,
    /// <summary>
    /// The source value is loaded to the destination attaching the source value to the destination and updating the source to be equal to the destination
    /// </summary>
    RELOCATE
}

public class MoveInstruction : DualParameterInstruction
{
    public const string INSTRUCTION = "mov";

    public MoveMode Mode { get; set; } = MoveMode.COPY;

    public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Build()
    {
        // Move shouldn't happen if the source is the same as the destination
        if (First.Value.Equals(Second.Value)) return;

        var flags_first = ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS;
        var flags_second = ParameterFlag.NONE;

        switch (Mode)
        {
            case MoveMode.COPY:
            {
                // Source value must be attached to the destination
                flags_second |= ParameterFlag.ATTACH_TO_DESTINATION;
                break;
            }

            case MoveMode.LOAD:
            {
                // Destination value must be attached to the destination
                flags_first |= ParameterFlag.ATTACH_TO_DESTINATION;
                break;
            }

            case MoveMode.RELOCATE:
            {
                // Source value must be attached and relocated to destination
                flags_second |= ParameterFlag.ATTACH_TO_DESTINATION | ParameterFlag.RELOCATE_TO_DESTINATION;
                break;
            }
        }
        
        if (First.Value.Type == HandleType.MEMORY)
        {
            Build(
                INSTRUCTION,
                new InstructionParameter(
                    First,
                    flags_first,
                    HandleType.REGISTER,
                    HandleType.MEMORY
                ),
                new InstructionParameter(
                    Second,
                    flags_second,
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
                    flags_first,
                    HandleType.REGISTER,
                    HandleType.MEMORY
                ),
                new InstructionParameter(
                    Second,
                    flags_second,
                    HandleType.CONSTANT,
                    HandleType.REGISTER,
                    HandleType.MEMORY
                )
            );
        }
    }

    public override Result GetDestinationDependency()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MOVE;
    }
}