using System.Collections.Generic;

public class GetVariableInstruction : LoadInstruction
{
	public Result? Self { get; private set; }
	public Variable Variable { get; private set; }
	private Result Current { get; set; }

	public GetVariableInstruction(Unit unit, Result? self, Variable variable, AccessMode mode) : base(unit, mode)
	{
		Self = self;
		Variable = variable;
		Description = $"Get the current handle of variable '{variable.Name}' with { (mode == AccessMode.WRITE ? "write" : "read") } access";
		Result.Format = variable.Type!.Format;

		Configure(References.CreateVariableHandle(unit, Self, variable));
	}

	public override void OnSimulate()
	{
		/*var current = Unit.GetCurrentVariableHandle(Variable);

		if (current != null && !current.Equals(Source))
		{
			if (!IsRedirected)
			{
				Result.Join(current);
			}

			Source.Join(current);
		}*/

		var current = Unit.GetCurrentVariableHandle(Variable);

		if (current == null)
		{
			return;
		}

		Current = current;
		Result.Join(Current);
	}

	public override void OnBuild()
	{
		OnSimulate();
		//base.OnBuild();

		/*if (Mode == AccessMode.WRITE && !Variable.IsPredictable)
		{
			Result.Value = Source.Value;
		}*/
	}
	
	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_VARIABLE;
	}

	public override Result[] GetResultReferences()
	{
		var references = new List<Result>() { Result, Source };

		if (Self != null)
		{
			references.Add(Self);
		}

		if (Current != null)
		{
			references.Add(Current);
		}

		return references.ToArray();
	}
}