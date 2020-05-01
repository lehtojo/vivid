public class MultiplicationInstruction : DualParameterInstruction
{
    private const string SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION = "imul";
    private const string UNSIGNED_INTEGER_MULTIPLICATION_INSTRUCTION = "mul";

    private const string SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION = "mulss";
    private const string DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION = "mulsd";

    public bool Assigns { get; private set; }
    public new Format Type { get; private set; }

    public MultiplicationInstruction(Unit unit, Result first, Result second, Format type, bool assigns) : base(unit, first, second)
    {
        Type = type;

        if (Assigns = assigns)
        {
            Result.Metadata = First.Metadata;
        }
    }

    public override void OnBuild()
    {
        var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

        // Handle decimal multiplication separately
        if (Type == global::Format.DECIMAL)
        {
            var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_MULTIPLICATION_INSTRUCTION : DOUBLE_PRECISION_MULTIPLICATION_INSTRUCTION;

            Build(
                instruction,
                Assembler.Size,
                new InstructionParameter(
                    First,
                    flags,
                    HandleType.MEDIA_REGISTER
                ),
                new InstructionParameter(
                    Second,
                    ParameterFlag.NONE,
                    HandleType.MEDIA_REGISTER,
                    HandleType.MEMORY
                )
            );
            
            return;
        }

        Build(
            SIGNED_INTEGER_MULTIPLICATION_INSTRUCTION,
            Assembler.Size,
            new InstructionParameter(
                First,
                flags,
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                ParameterFlag.NONE,
                HandleType.CONSTANT,
                HandleType.REGISTER,
                HandleType.MEMORY
            )
        );
    }

    public override Result GetDestinationDependency()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.MULTIPLICATION;
    }
}