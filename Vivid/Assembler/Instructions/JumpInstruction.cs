using System.Collections.Generic;
using System;

public class JumpInstruction : Instruction
{
	private static readonly Dictionary<ComparisonOperator, string[]> Instructions = new Dictionary<ComparisonOperator, string[]>();

	static JumpInstruction()
	{
		Instructions.Add(Operators.GREATER_THAN, new[] { "jg", "ja" });
		Instructions.Add(Operators.GREATER_OR_EQUAL, new[] { "jge", "jae" });
		Instructions.Add(Operators.LESS_THAN, new[] { "jl", "jb" });
		Instructions.Add(Operators.LESS_OR_EQUAL, new[] { "jle", "jbe" });
		Instructions.Add(Operators.EQUALS, new[] { "je", "jz" });
		Instructions.Add(Operators.NOT_EQUALS, new[] { "jne", "jnz" });
	}

	public Label Label { get; set; }
	public ComparisonOperator? Comparator { get; private set; }
	public bool IsConditional => Comparator != null;
	public bool IsSigned { get; private set; } = true;

	public JumpInstruction(Unit unit, Label label) : base(unit)
	{
		Label = label;
		Comparator = null;
	}

	public JumpInstruction(Unit unit, ComparisonOperator comparator, bool invert, bool signed, Label label) : base(unit)
	{
		Label = label;
		Comparator = invert ? comparator.Counterpart : comparator;
		IsSigned = signed;
	}

	public void Invert()
	{
		Comparator = Comparator?.Counterpart ?? throw new ApplicationException("Tried to invert unconditional jump instruction");
	}

	public override void OnBuild()
	{
		var instruction = Comparator == null ? "jmp" : Instructions[Comparator][IsSigned ? 0 : 1];

		Build($"{instruction} {Label.GetName()}");
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
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