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

      Result.Value = new ComplexMemoryHandle(Start, Offset, Stride)
      {
         Format = format
      };
		
      Result.Metadata.Attach(new ComplexMemoryAddressAttribute());

		Source.Value = Result.Value;
		Result.Format = format;
		Source.Metadata.Attach(new ComplexMemoryAddressAttribute());
	}

	public override void OnBuild()
	{
		Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Start, Offset };
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