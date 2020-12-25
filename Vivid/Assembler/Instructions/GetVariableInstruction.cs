/// <summary>
/// Returns a handle to the specified variable
/// This instruction works on all architectures
/// </summary>
public class GetVariableInstruction : Instruction
{
	public Result? Self { get; private set; }
	public Type? SelfType { get; private set; }
	public Variable Variable { get; private set; }
	public AccessMode Mode { get; private set; }

	public GetVariableInstruction(Unit unit, Variable variable, AccessMode mode) : this(unit, null, null, variable, mode) {}

	public GetVariableInstruction(Unit unit, Result? self, Type? self_type, Variable variable, AccessMode mode) : base(unit)
	{
		Self = self;
		SelfType = self_type;
		Variable = variable;
		Mode = mode;
		Description = $"Get the current handle of variable '{variable.Name}'";

		Result.Value = References.CreateVariableHandle(unit, Self, self_type, variable);
		Result.Format = variable.Type!.Format;
	}

	public override void OnSimulate()
	{
		var current = Unit.GetCurrentVariableHandle(Variable);

		if (current == null)
		{
			return;
		}

		Result.Join(current);
	}

	public override void OnBuild()
	{
		OnSimulate();
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_VARIABLE;
	}

	public override Result[] GetResultReferences()
	{
		if (Self != null)
		{
			return new[] { Result, Self };
		}

		return new[] { Result };
	}
}