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

	public GetVariableInstruction(Unit unit, Variable variable, AccessMode mode) : this(unit, null, null, variable, mode) { }

	public GetVariableInstruction(Unit unit, Result? self, Type? self_type, Variable variable, AccessMode mode) : base(unit, InstructionType.GET_VARIABLE)
	{
		Self = self;
		SelfType = self_type;
		Variable = variable;
		Mode = mode;
		IsAbstract = true;
		Description = $"Get the current handle of variable '{variable.Name}'";

		if (Self != null)
		{
			Dependencies = new[] { Result, Self };
		}

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

		// If the result represents a static variable, it might be needed to load it into a register
		if (Variable.IsStatic && Mode == AccessMode.READ && Trace.IsLoadingRequired(Unit, Result))
		{
			Memory.MoveToRegister(Unit, Result, Assembler.Size, Result.Format.IsDecimal(), Trace.GetDirectives(Unit, Result));
		}
	}
}