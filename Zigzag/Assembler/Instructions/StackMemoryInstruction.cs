public class StackMemoryInstruction : Instruction
{
    public const string ALLOCATE = "sub";
    public const string SHRINK = "add";

    public int Bytes { get; private set; }
    public bool Hidden { get; private set; }

    private StackMemoryInstruction(Unit unit, int bytes, bool hidden) : base(unit)
    {
        Bytes = bytes;
        Hidden = hidden;
    }

    public static StackMemoryInstruction Allocate(Unit unit, int bytes, bool hidden)
    {
        return new StackMemoryInstruction(unit, bytes, hidden);
    }

    public static StackMemoryInstruction Shrink(Unit unit, int bytes, bool hidden)
    {
        return new StackMemoryInstruction(unit, -bytes, hidden);
    }

    public override void OnBuild()
    {
        if (!Hidden)
        {
            if (Bytes < 0)
            {
                Build($"{SHRINK} {Unit.GetStackPointer()}, {-Bytes}");
            }
            else if (Bytes > 0)
            {
                Build($"{ALLOCATE} {Unit.GetStackPointer()}, {Bytes}");
            }
        }
    }

    public override int GetStackOffsetChange()
    {
        return Bytes;
    }

    public override Result? GetDestinationDependency()
    {
        return null;
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.STACK_MEMORY;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result };
    }
}