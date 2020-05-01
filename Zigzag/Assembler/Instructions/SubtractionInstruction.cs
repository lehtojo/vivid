public class SubtractionInstruction : DualParameterInstruction
{
    private const string SINGLE_PRECISION_SUBTRACTION_INSTRUCTION = "subss";
    private const string DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION = "subsd";

    public bool Assigns { get; private set; }
    public new Format Type { get; private set; }
    
    public SubtractionInstruction(Unit unit, Result first, Result second, Format type, bool assigns) : base(unit, first, second)
    {
        if (Assigns = assigns)
        {
            Result.Metadata = First.Metadata;
        }
    }

    public override void OnBuild()
    {
        // Handle decimal division separately
        if (Type == global::Format.DECIMAL)
        {
            var instruction = Assembler.Size.Bits == 32 ? SINGLE_PRECISION_SUBTRACTION_INSTRUCTION : DOUBLE_PRECISION_SUBTRACTION_INSTRUCTION;
            var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

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

        if (Assigns)
        {
            Build(
                "sub",
                Assembler.Size,
                new InstructionParameter(
                    First,
                    ParameterFlag.WRITE_ACCESS,
                    HandleType.REGISTER,
                    HandleType.MEMORY
                ),
                new InstructionParameter(
                    Second,
                    ParameterFlag.NONE,
                    HandleType.CONSTANT,
                    HandleType.REGISTER
                )
            );
        }
        else
        {
            Build(
                "sub",
                Assembler.Size,
                new InstructionParameter(
                    First,
                    ParameterFlag.DESTINATION,
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
    }

    public override Result GetDestinationDependency()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.SUBTRACT;
    }
}