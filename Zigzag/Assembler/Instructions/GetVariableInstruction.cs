
public class GetVariableInstruction : LoadInstruction
{
	public Result? Self { get; private set; }
	public Variable Variable { get; private set; }

	public GetVariableInstruction(Unit unit, Result? self, Variable variable, AccessMode mode) : base(unit, mode)
	{
		Self = self;
		Variable = variable;
		Description = $"Get the current handle of variable '{variable.Name}' with { (mode == AccessMode.WRITE ? "write" : "read") } access";
		Result.Format = variable.Type.Format;

		Configure(References.CreateVariableHandle(unit, Self, variable));
	}

	public override void OnSimulate()
	{
		var current = Unit.GetCurrentVariableHandle(Variable);

		if (current != null && !current.Equals(Source))
		{
			if (!IsRedirected)
			{
				Result.Join(current);
			}

			Source.Join(current);
		}
	}

	public override void OnBuild()
	{
		OnSimulate();
		base.OnBuild();
	}
	
	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_VARIABLE;
	}

	public override Result[] GetResultReferences()
	{
		if (Self != null)
		{
			return new Result[] { Result, Source, Self };
		}
		else
		{
			return new Result[] { Result, Source };
		}
	}
}