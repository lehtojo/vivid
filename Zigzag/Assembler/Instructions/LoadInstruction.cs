public enum AccessMode
{
	WRITE,
	READ
}

public abstract class LoadInstruction : Instruction
{
	public AccessMode Mode { get; private set; }
	public Result Source { get; set; } = new Result();
	public bool IsRedirected => !Result.Value.Equals(Source.Value);

	public LoadInstruction(Unit unit, AccessMode mode) : base(unit) 
	{
		Mode = mode;
	}

	public void Connect(Result result)
	{
		Result.Join(result);
		Source.Join(result);
	}

	public void Configure(Handle handle, MetadataAttribute? attribute = null)
	{
		Source.Value = handle;
		Result.Value = handle;

		if (attribute != null)
		{
			Source.Metadata.Attach(attribute);
			Result.Metadata.Attach(attribute);
		}
	}

	public override void OnBuild() {}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Source };
	}
}