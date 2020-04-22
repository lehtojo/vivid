public class PushInstruction : Instruction
{
    public const string INSTRUCTION = "push";

    public Size Size { get; private set; }
    public Result Value { get; private set; }

    public PushInstruction(Unit unit, Result value, Size size) : base(unit)
    {
        Size = size;
        Value = value;
    }

    public override void Build()
    {
        // Check if the value must be converted to the accepted stack element size
        if (Size != Assembler.Size && Value.Value.Type == HandleType.MEMORY)
        {
            Build(
                INSTRUCTION,
                Assembler.Size,
                new InstructionParameter(
                    Value,
                    ParameterFlag.NONE,
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
                    Value,
                    ParameterFlag.NONE,
                    HandleType.CONSTANT,
                    HandleType.REGISTER,
                    HandleType.MEMORY
                )
            );
        }
    }

    public override int GetStackOffsetChange()
    {
        return Assembler.Size.Bytes;
    }

    public override Result? GetDestinationDependency()
    {
        return null;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.PUSH;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Value };
    }
}