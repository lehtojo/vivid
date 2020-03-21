public class SubtractionInstruction : DualParameterInstruction
{
    public bool Assigns { get; private set; }

    public SubtractionInstruction(Unit unit, Result first, Result second, bool assigns) : base(unit, first, second) 
    {
        Assigns = assigns;
    }

    public override void Build()
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
                HandleType.MEMORY_HANDLE
            )
        );
    }

    public override Result GetDestinationDepency()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.SUBTRACT;
    }
}