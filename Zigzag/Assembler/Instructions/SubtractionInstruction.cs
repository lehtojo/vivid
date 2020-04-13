public class SubtractionInstruction : DualParameterInstruction
{
    public bool Assigns { get; private set; }

    public SubtractionInstruction(Unit unit, Result first, Result second, bool assigns) : base(unit, first, second)
    {
        if (Assigns = assigns)
        {
            Result.Metadata = First.Metadata;
        }
    }

    public override void Build()
    {
        if (First.Metadata.IsComplex)
        {
            Build(
                "sub",
                new InstructionParameter(
                    First,
                    ParameterFlag.DESTINATION,
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