using System.Collections.Generic;
using System;

public class JumpInstruction : Instruction
{
	private static readonly Dictionary<ComparisonOperator, string> Instructions = new Dictionary<ComparisonOperator, string>();

	static JumpInstruction()
	{
		Instructions.Add(Operators.GREATER_THAN, "jg");
		Instructions.Add(Operators.GREATER_OR_EQUAL, "jge");
		Instructions.Add(Operators.LESS_THAN, "jl");
		Instructions.Add(Operators.LESS_OR_EQUAL, "jle");
		Instructions.Add(Operators.EQUALS, "je");
		Instructions.Add(Operators.NOT_EQUALS, "jne");
	}

	public Label Label { get; set; }
	public ComparisonOperator? Comparator { get; private set; }
	public bool IsConditional => Comparator != null;

	public JumpInstruction(Unit unit, Label label) : base(unit)
	{
		Label = label;
		Comparator = null;
	}

	public JumpInstruction(Unit unit, ComparisonOperator comparator, bool invert, Label label) : base(unit)
	{
		Label = label;
		Comparator = invert ? comparator.Counterpart : comparator;    
	}

	public void Invert()
	{
		Comparator = Comparator?.Counterpart ?? throw new ApplicationException("Standard jump instruction shouldn't be inverted");
	}

	public override void OnBuild()
	{
		var instruction = Comparator == null ? "jmp" : Instructions[Comparator];
		Build($"{instruction} {Label.GetName()}");
	}

	public override Result[] GetResultReferences()
	{
		return new Result[] { Result };
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.JUMP;
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Jump-Instruction");
	}
}