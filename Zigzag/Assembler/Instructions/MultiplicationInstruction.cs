public class MultiplicationInstruction : DualParameterInstruction
{
    public bool Assigns { get; private set; }

    public MultiplicationInstruction(Unit unit, Result first, Result second, bool assigns) : base(unit, first, second)
    {
        if (Assigns = assigns)
        {
            Result.Metadata = First.Metadata;
        }
    }

    public override void Build()
    {
        var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

        if (Assigns)
        {
            Build(
                "imul",
                Assembler.Size,
                new InstructionParameter(
                    First,
                    flags,
                    HandleType.REGISTER,
                    HandleType.MEMORY
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
        else
        {
            Build(
                "imul",
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