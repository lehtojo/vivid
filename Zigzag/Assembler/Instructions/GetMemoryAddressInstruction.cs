public class GetMemoryAddressInstruction : Instruction
{
    public const string METADATA = "Complex Memory Address";

    public Result Base { get; private set; }
    public Result Offset { get; private set; }
    public int Stride { get; private set; }

    public GetMemoryAddressInstruction(Unit unit, Result @base, Result offset, int stride) : base(unit)
    {
        Base = @base;
        Offset = offset;
        Stride = stride;
    }

    public override void Build()
    {
        Memory.Convert(Unit, Base, true, HandleType.CONSTANT, HandleType.REGISTER);
        Memory.Convert(Unit, Offset, true, HandleType.CONSTANT, HandleType.REGISTER);
        
        Result.Value = new ComplexMemoryHandle(Base, Offset, Stride);
        Result.Metadata = METADATA;
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Base, Offset };
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_MEMORY_ADDRESS;
    }

    public override Result? GetDestinationDepency()
    {
        return null;   
    }
}