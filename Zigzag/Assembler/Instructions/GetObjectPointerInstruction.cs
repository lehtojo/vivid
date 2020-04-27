public class GetObjectPointerInstruction : Instruction
{
    public Result Start { get; private set; }
    public int Offset { get; private set; }

    public GetObjectPointerInstruction(Unit unit, Variable variable, Result start, int offset) : base(unit)
    {
        Result.Metadata.Attach(new VariableAttribute(variable));
        Start = start;
        Offset = offset;
    }

    public override void OnBuild()
    {
        // Lock the parameters
        Memory.Convert(Unit, Start, true, HandleType.CONSTANT, HandleType.REGISTER);
        Result.Value = new MemoryHandle(Unit, Start, Offset);
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Start };
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_OBJECT_POINTER;
    }

    public override Result? GetDestinationDependency()
    {
        return null;   
    }
}