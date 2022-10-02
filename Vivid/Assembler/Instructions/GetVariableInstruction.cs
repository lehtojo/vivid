/// <summary>
/// Returns a handle to the specified variable
/// This instruction works on all architectures
/// </summary>
public class GetVariableInstruction : Instruction
{
	public Variable Variable { get; private set; }
	public AccessMode Mode { get; private set; }

	public GetVariableInstruction(Unit unit, Variable variable, AccessMode mode) : base(unit, InstructionType.GET_VARIABLE)
	{
		Variable = variable;
		Mode = mode;
		IsAbstract = true;
		Description = $"Get the current handle of variable '{variable.Name}'";

		Result.Value = References.CreateVariableHandle(unit, variable);
		Result.Format = variable.Type!.Format;
	}

	public override void OnBuild()
	{
		// If the result represents a static variable, it might be needed to load it into a register
		if (Variable.IsStatic && Mode == AccessMode.READ)
		{
			Memory.MoveToRegister(Unit, Result, Assembler.Size, Result.Format.IsDecimal(), Trace.For(Unit, Result));
		}
	}
}