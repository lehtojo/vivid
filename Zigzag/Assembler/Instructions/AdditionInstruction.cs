public class AdditionInstruction : DualParameterInstruction
{
    public bool Assigns { get; private set; }

    public AdditionInstruction(Unit unit, Result first, Result second, bool assigns) : base(unit, first, second) 
    {
        if (Assigns = assigns)
        {
            Result.Metadata = First.Metadata;
        }
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.ADDITION;
    }
    
    public override void Build()
    {
        if (First.IsExpiring(Position) || Assigns)
        {
            var flags = ParameterFlag.DESTINATION | (Assigns ? ParameterFlag.WRITE_ACCESS : ParameterFlag.NONE);

            Build(
                "add",
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
        else
        {
            // Form the calculation parameter
            var calculation = Format(
                "[{0}+{1}]",
                new InstructionParameter(
                    First,
                    ParameterFlag.NONE,
                    HandleType.CONSTANT,
                    HandleType.REGISTER
                ),
                new InstructionParameter(
                    Second,
                    ParameterFlag.NONE,
                    HandleType.CONSTANT,
                    HandleType.REGISTER
                )
            );

            if (Result.Value.Type != HandleType.REGISTER)
            {
                // Get a new register for the result
                Memory.GetRegisterFor(Unit, Result);
            }

            Build($"lea {Result}, {calculation}");
        }
    }

    public override Result? GetDestinationDependency()
    {
        if (First.IsExpiring(Position))
        {
            return First;
        }
        else
        {
            return Result;
        }
    }
}