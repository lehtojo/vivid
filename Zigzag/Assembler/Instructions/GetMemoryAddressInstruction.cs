public class GetMemoryAddressInstruction : Instruction
{    
    public AccessMode Mode { get; private set; }
    public Result Source { get; set; } = new Result();

    public Result Base { get; private set; }
    public Result Offset { get; private set; }
    public int Stride { get; private set; }

    public GetMemoryAddressInstruction(Unit unit, AccessMode mode, Result @base, Result offset, int stride) : base(unit)
    {
        Mode = mode;
        Base = @base;
        Offset = offset;
        Stride = stride;

        Result.Value = new ComplexMemoryHandle(Base, Offset, Stride);
        Result.Metadata.Attach(new ComplexMemoryAddressAttribute());

        Source.Value = Result.Value;
        Source.Metadata.Attach(new ComplexMemoryAddressAttribute());
    }

    public override void Build()
    {
        Memory.Convert(Unit, Base, true, HandleType.CONSTANT, HandleType.REGISTER);
        Memory.Convert(Unit, Offset, true, HandleType.CONSTANT, HandleType.REGISTER);

        if (Mode != AccessMode.WRITE && !Result.Equals(Source))
        {
            var move = new MoveInstruction(Unit, Result, Source);
            move.Mode = MoveMode.LOAD;

            // Since the source is not where it should be, it must be moved to the result 
            Unit.Append(move);
        }
    }

    public override Result[] GetResultReferences()
    {
        return new Result[] { Result, Base, Offset };
    }

    public override InstructionType GetInstructionType()
    {
        return InstructionType.GET_MEMORY_ADDRESS;
    }

    public override Result? GetDestinationDependency()
    {
        return null;   
    }
}