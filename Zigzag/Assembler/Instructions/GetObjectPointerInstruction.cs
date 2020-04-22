public class GetObjectPointerInstruction : Instruction
{
    public Result Base { get; private set; }
    public int Offset { get; private set; }

    public GetObjectPointerInstruction(Unit unit, Variable variable, Result @base, int offset) : base(unit)
    {
        Result.Metadata.Attach(new VariableAttribute(variable, -1));
        Base = @base;
        Offset = offset;
    }

    public override void Build()
    {
        Memory.Convert(Unit, Base, true, HandleType.CONSTANT, HandleType.REGISTER);
        Result.Value = new MemoryHandle(Unit, Base, Offset);
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Base };
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