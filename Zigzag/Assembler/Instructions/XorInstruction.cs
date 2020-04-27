
public class XorInstruction : DualParameterInstruction
{
    public const string INSTRUCTION = "xor";
    public bool IsSafe { get; set; } = true;

    public XorInstruction(Unit unit, Result first, Result second) : base(unit, first, second) {}

    public override void OnBuild()
    {
        Build(
            INSTRUCTION,
            Assembler.Size,
            new InstructionParameter(
                First,
                ParameterFlag.DESTINATION | (IsSafe ? ParameterFlag.NONE : ParameterFlag.WRITE_ACCESS),
                HandleType.REGISTER
            ),
            new InstructionParameter(
                Second,
                ParameterFlag.NONE,
                HandleType.REGISTER
            )
        );
    }

    public override Result GetDestinationDependency()
    {
        return First;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.XOR;
    }
}