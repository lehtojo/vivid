public class AdditionInstruction : DualParameterInstruction
{
    public const string INSTRUCTION = "add";
    public const string EXTENDED_ADDITION_INSTRUCTION = "lea";

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
    
    public override void OnBuild()
    {
        if (First.IsExpiring(Position) || Assigns)
        {
            if (Assigns)
            {
                Build(
                    INSTRUCTION,
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
                    INSTRUCTION,
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

            Build($"{EXTENDED_ADDITION_INSTRUCTION} {Result}, {calculation}");
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