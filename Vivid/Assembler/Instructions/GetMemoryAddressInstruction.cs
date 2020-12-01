/// <summary>
/// Specialized instruction for accessing raw memory
/// </summary>
public class GetMemoryAddressInstruction : Instruction
{
	public AccessMode Mode { get; private set; }
	public Format Format { get; private set; }

	public Result Start { get; private set; }
	public Result Offset { get; private set; }
	public int Stride { get; private set; }

	public GetMemoryAddressInstruction(Unit unit, AccessMode mode, Format format, Result start, Result offset, int stride) : base(unit)
	{
		Mode = mode;
		Start = start;
		Offset = offset;
		Stride = stride;
		Format = format;

		Result.Value = new ComplexMemoryHandle(Start, Offset, Stride) { Format = format };
		Result.Metadata.Attach(new ComplexMemoryAddressAttribute());
		Result.Format = Format;
	}

	public override void OnBuild()
	{
		Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);

		if (Mode == AccessMode.READ)
		{
			Memory.MoveToRegister(Unit, Result, Assembler.Size, Format.IsDecimal(), Result.GetRecommendation(Unit));
		}
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