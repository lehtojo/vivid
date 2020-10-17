public enum AccessMode
{
	WRITE,
	READ
}

public abstract class LoadInstruction : Instruction
{
	public AccessMode Mode { get; private set; }

	public LoadInstruction(Unit unit, AccessMode mode) : base(unit)
	{
		Mode = mode;
	}

	/*public void Connect(Result result)
	{
		Result.Join(result);
	}

	public void Configure(Handle handle, MetadataAttribute? attribute = null)
	{
		Result.Value = handle;

		if (attribute != null)
		{
			Result.Metadata.Attach(attribute);
		}
	}*/

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}