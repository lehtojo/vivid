using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Ensures that the lifetimes of the specified variables begin at least at this instruction
/// This instruction works on all architectures
/// </summary>
public class RequireVariablesInstruction : Instruction
{
	public List<Variable> Variables { get; private set; }
	public List<Result> References { get; private set; }

	public RequireVariablesInstruction(Unit unit, List<Variable> variables) : base(unit)
	{
		Variables = variables;
		References = new List<Result>();
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.REQUIRE_VARIABLES;
	}

	public override Result[] GetResultReferences()
	{
		// Even though the result of this instruction shouldn't be used, it's still good practice to add it to the list
		return References.Concat(new[] { Result }).ToArray();
	}
}