using System.Collections.Generic;

/// <summary>
/// Ensures that the lifetimes of the specified variables begin at least at this instruction
/// This instruction works on all architectures
/// </summary>
public class RequireVariablesInstruction : Instruction
{
	public List<Variable> Variables { get; private set; }
	public List<Result> References { get; private set; }

	public RequireVariablesInstruction(Unit unit, List<Variable> variables) : base(unit, InstructionType.REQUIRE_VARIABLES)
	{
		Variables = variables;
		References = new List<Result>();
	}
}