using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ensures that the lifetimes of the specified variables begin at least at this instruction
/// This instruction works on all architectures
/// </summary>
public class RequireVariablesInstruction : Instruction
{
	public List<Variable> Variables { get; private set; }
	public List<Result> Results { get; private set; }

	public RequireVariablesInstruction(Unit unit, List<Variable> variables) : base(unit, InstructionType.REQUIRE_VARIABLES)
	{
		Variables = new List<Variable>();
		Results = new List<Result>();
		IsAbstract = true;

		Extract(unit, variables);
	}

	private void Extract(Unit unit, List<Variable> variables)
	{
		// If the variable is a pack, its members must be extracted, otherwise the variable can be added only
		foreach (var variable in variables.ToArray())
		{
			if (!variable.Type!.IsPack)
			{
				Variables.Add(variable);
				continue;
			}

			Extract(unit, References.CreateVariableHandle(unit, variable).To<PackHandle>().Variables.Values.ToList());
		}
	}
}