public class GetObjectPointerInstruction : Instruction
{
    public Result Base { get; private set; }
    public int Offset { get; private set; }

    public GetObjectPointerInstruction(Unit unit, Variable variable, Result @base, int offset) : base(unit)
    {
        Result.Metadata = variable;
        Base = @base;
        Offset = offset;
    }

    public override void Build()
    {
        Memory.MoveToRegister(Unit, Base);
        Result.Set(new MemoryHandle(Base, Offset));
    }

    public override Result[] GetHandles()
    {
        return new Result[] { Result, Base };
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_OBJECT_POINTER;
    }

    public override Result? GetDestination()
    {
        return null;   
    }
}