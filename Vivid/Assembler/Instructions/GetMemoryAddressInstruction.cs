/// <summary>
/// Returns a handle for accessing raw memory
/// This instruction works on all architectures
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
		if (!Trace.IsLoadingRequired(Unit, Result))
		{
			Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
			Result.Format = Format;
			return;
		}

		if (Mode == AccessMode.READ)
		{	
			Result.Value = new ComplexMemoryHandle(Start, Offset, Stride);
			Result.Format = Format;

			Memory.MoveToRegister(Unit, Result, Assembler.Size, Format.IsDecimal(), Result.GetRecommendation(Unit));
		}
		else
		{
			var address = new Result(ExpressionHandle.CreateMemoryAddress(Start, Offset, Stride), Assembler.Format);
			Memory.MoveToRegister(Unit, address, Assembler.Size, false, Result.GetRecommendation(Unit));

			Result.Value = new MemoryHandle(Unit, address, 0);
			Result.Format = Format;
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