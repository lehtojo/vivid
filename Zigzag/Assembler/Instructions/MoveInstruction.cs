using System;

public enum MoveType
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
    public const string MOVE_INSTRUCTION = "mov";
    public const string CLEAR_INSTRUCTION = "xor";

    public new MoveType Type { get; set; } = MoveType.COPY;
    public bool IsSafe { get; set; } = false;

    public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void Build()
    {
        // Move shouldn't happen if the source is the same as the destination
        if (First.Value.Equals(Second.Value)) return;

        var flags_first = ParameterFlag.DESTINATION | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS);
        var flags_second = ParameterFlag.NONE;

        switch (Type)
        {
            case MoveType.COPY:
            {
                // Source value must be attached to the destination
                flags_second |= ParameterFlag.ATTACH_TO_DESTINATION;
                break;
            }

            case MoveType.LOAD:
            {
                // Destination value must be attached to the destination
                flags_first |= ParameterFlag.ATTACH_TO_DESTINATION;
                break;
            }

            case MoveType.RELOCATE:
            {
                // Source value must be attached and relocated to destination
                flags_second |= ParameterFlag.ATTACH_TO_DESTINATION | ParameterFlag.RELOCATE_TO_DESTINATION;
                break;
            }
        }

        if (First.Value.Type == HandleType.REGISTER && Second.Value is ConstantHandle constant && constant.Value.Equals(0L))
        {
            Build(
                CLEAR_INSTRUCTION,
                Assembler.Size,
                new InstructionParameter(
                    First,
                    flags_first,
                    HandleType.REGISTER
                ),
                new InstructionParameter(
                    First,
                    ParameterFlag.NONE,
                    HandleType.REGISTER
                ),
                new InstructionParameter(
                    Second,
                    flags_second| ParameterFlag.HIDDEN,
                    HandleType.CONSTANT
                )
            );
        }
        else if (First.Value.Type == HandleType.MEMORY)
        {
            Build(
                MOVE_INSTRUCTION,
                Assembler.Size,
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
                MOVE_INSTRUCTION,
                Assembler.Size,
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