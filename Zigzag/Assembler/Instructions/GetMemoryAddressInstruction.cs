public class GetMemoryAddressInstruction : Instruction
{    
	public AccessMode Mode { get; private set; }
	public Result Source { get; set; } = new Result();

	public Result Start { get; private set; }
	public Result Offset { get; private set; }
	public int Stride { get; private set; }

	public GetMemoryAddressInstruction(Unit unit, AccessMode mode, Format format, Result start, Result offset, int stride) : base(unit)
	{
		Mode = mode;
		Start = start;
		Offset = offset;
		Stride = stride;

		Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
		Result.Value.Format = format;
		Result.Metadata.Attach(new ComplexMemoryAddressAttribute());

		Source.Value = Result.Value;
		Source.Metadata.Attach(new ComplexMemoryAddressAttribute());
	}

	public override void OnBuild()
	{
		Memory.Convert(Unit, Start, true, HandleType.CONSTANT, HandleType.REGISTER);
		Memory.Convert(Unit, Offset, true, HandleType.CONSTANT, HandleType.REGISTER);

		if (Mode != AccessMode.WRITE && !Result.Equals(Source))
		{
			var move = new MoveInstruction(Unit, Result, Source);
			move.Type = MoveType.LOAD;

			// Since the source is not where it should be, it must be moved to the result 
			Unit.Append(move);
		}
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result, Start, Offset };
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