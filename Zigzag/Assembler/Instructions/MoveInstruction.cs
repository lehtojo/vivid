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
    public const string UNSIGNED_CONVERSION = "movzx";
    public const string SIGNED_CONVERSION = "movsx";
    public const string SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE = "movsxd";
    
    public const string SINGLE_PRECISION_MOVE = "movss";
    public const string DOUBLE_PRECISION_MOVE = "movsd";

    public const string CONVERT_SINGLE_PRECISION_TO_INTEGER = "cvtss2si";
    public const string CONVERT_DOUBLE_PRECISION_TO_INTEGER = "cvtsd2si";

    public const string CONVERT_INTEGER_TO_SINGLE_PRECISION = "cvtsi2ss";
    public const string CONVERT_INTEGER_TO_DOUBLE_PRECISION = "cvtsi2sd";

    public const string CLEAR_INSTRUCTION = "xor";

    public new MoveType Type { get; set; } = MoveType.COPY;
    public bool IsSafe { get; set; } = false;

    public MoveInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    private bool IsDecimalConversionNeeded()
    {
        return First.Value.Format != Second.Value.Format;
    }

    private void OnBuildDecimalConversion(int flags_first, int flags_second)
    {
        var is_destination_media_register = First.Value is RegisterHandle handle && handle.Register.IsMediaRegister;
        var is_destination_register = First.Value is RegisterHandle;
        var is_destination_memory_address = First!.Value?.Type == HandleType.MEMORY;

        if (is_destination_media_register)
        {
            var instruction = Assembler.Size.Bits == 32 ? CONVERT_INTEGER_TO_SINGLE_PRECISION : CONVERT_INTEGER_TO_DOUBLE_PRECISION;

            Build(
                instruction,
                new InstructionParameter(
                    First,
                    flags_first,
                    HandleType.MEDIA_REGISTER
                ),
                new InstructionParameter(
                    Second,
                    flags_second,
                    HandleType.REGISTER,
                    HandleType.MEMORY
                )
            );
        }
        else if (is_destination_register)
        {
            var instruction = Assembler.Size.Bits == 32 ? CONVERT_SINGLE_PRECISION_TO_INTEGER : CONVERT_DOUBLE_PRECISION_TO_INTEGER;

            Build(
                instruction,
                new InstructionParameter(
                    First,
                    flags_first,
                    HandleType.REGISTER
                ),
                new InstructionParameter(
                    Second,
                    flags_second,
                    HandleType.MEDIA_REGISTER,
                    HandleType.MEMORY
                )
            );
        }
        else if (is_destination_memory_address)
        {
            var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;
            
            Build(
                instruction,
                new InstructionParameter(
                    First,
                    flags_first,
                    HandleType.MEMORY
                ),
                new InstructionParameter(
                    Second,
                    flags_second,
                    HandleType.MEDIA_REGISTER
                )
            );
        }
    }

    private void OnBuildDecimalMoves(int flags_first, int flags_second)
    {
        if (IsDecimalConversionNeeded())
        {
            OnBuildDecimalConversion(flags_first, flags_second);
        }
        else
        {
            var size = First.Value.Size;
            var instruction = size.Bits == 32 ? SINGLE_PRECISION_MOVE : DOUBLE_PRECISION_MOVE;

            Build(
                instruction,
                new InstructionParameter(
                    First,
                    flags_first,
                    HandleType.MEDIA_REGISTER,
                    HandleType.MEMORY
                ),
                new InstructionParameter(
                    Second,
                    flags_second,
                    HandleType.MEDIA_REGISTER,
                    HandleType.MEMORY
                )
            );
        }
    }

    public override void OnBuild()
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

        if (First.Value.Format == global::Format.DECIMAL || Second.Value.Format == global::Format.DECIMAL)
        {
            // Decimals have their own handler
            OnBuildDecimalMoves(flags_first, flags_second);
            return;
        }

        if (First.Value.Type == HandleType.REGISTER && Second.Value is ConstantHandle constant && constant.Value.Equals(0L))
        {
            Build(
                CLEAR_INSTRUCTION,
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

    public override void OnPostBuild()
    {
        var is_source_memory_address = Source!.Value?.Type == HandleType.MEMORY;
        var is_destination_memory_address = Destination!.Value?.Type == HandleType.MEMORY;

        if (is_source_memory_address)
        {
            if (is_destination_memory_address)
            {
                throw new ApplicationException("Both destination and source were memory handles at the same time in move instruction");
            }

            // Now the destination parameter must be a register

            // Check if a conversion is needed
            if (Source!.Value!.Size != Destination!.Value!.Size)
            {
                // In 32-bit mode or lower there's only one conversion instruction type needed
                if (Assembler.Size.Bits <= 32)
                {
                    Operation = Destination.Value!.IsUnsigned ? UNSIGNED_CONVERSION : SIGNED_CONVERSION;
                }
                else if (Assembler.Size.Bits == 64)
                {
                    if (Destination.Value!.Size.Bits != 64)
                    {
                        throw new ApplicationException("Destination register should be 64-bit in 64-bit mode always");
                    }

                    if (Source.Value!.Size.Bits == 32)
                    {
                        if (Destination.Value!.IsUnsigned)
                        {
                            // In 64-bit mode if you move data from 32-bit register to another 32-bit register it zeroes out the high half of the destination 64-bit register
                            Destination.Value!.Format = global::Format.UINT32;
                        }
                        else
                        {
                            Operation = SIGNED_CONVERSION_FROM_DWORD_IN_64_BIT_MODE;
                        }
                    }
                    else
                    {
                        Operation = Destination.Value!.IsUnsigned ? UNSIGNED_CONVERSION : SIGNED_CONVERSION;
                    }
                }
                else
                {
                    throw new ApplicationException("Conversion needed in move instruction but current bitmode is unsupported");
                }
            }
        }
        else if (is_destination_memory_address)
        {
            Source.Value!.Format = Destination.Value!.Format;
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