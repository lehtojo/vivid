using System.Collections.Generic;

/// <summary>
/// Ensures that the lifetimes of the specified variables begin at least at this instruction
/// This instruction works on all architectures
/// </summary>
public class RequireVariablesInstruction : Instruction
{
	public bool IsInputter { get; set; } = false;

	public RequireVariablesInstruction(Unit unit, bool is_inputter) : base(unit, InstructionType.REQUIRE_VARIABLES)
	{
		Dependencies = new List<Result>();
		IsInputter = is_inputter;
		IsAbstract = true;
		Description = is_inputter ? "Inputs variables to the scope" : "Outputs variables to the scope";
	}
}