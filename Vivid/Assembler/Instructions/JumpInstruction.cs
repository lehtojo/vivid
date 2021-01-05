using System.Collections.Generic;
using System;

/// <summary>
/// Jumps to the specified label and optionally checks a condition
/// This instruction works on all architectures
/// </summary>
public class JumpInstruction : Instruction
{
	public const string X64_CONDITIONLESS_JUMP = "jmp";
	public const string ARM64_CONDITIONLESS_JUMP = "b";

	private static readonly Dictionary<ComparisonOperator, string[]> Instructions = new Dictionary<ComparisonOperator, string[]>();

	private static void Initialize()
	{
		if (Assembler.IsArm64)
		{
			Instructions.Add(Operators.GREATER_THAN, new[] { "b.gt", "b.gt" });
			Instructions.Add(Operators.GREATER_OR_EQUAL, new[] { "b.ge", "b.ge" });
			Instructions.Add(Operators.LESS_THAN, new[] { "b.lt", "b.lt" });
			Instructions.Add(Operators.LESS_OR_EQUAL, new[] { "b.le", "b.le" });
			Instructions.Add(Operators.EQUALS, new[] { "b.eq", "b.eq" });
			Instructions.Add(Operators.NOT_EQUALS, new[] { "b.ne", "b.ne" });
			return;
		}

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

	public JumpInstruction(Unit unit, Label label) : base(unit, InstructionType.JUMP)
	{
		if (Instructions.Count == 0)
		{
			Initialize();
		}

		Label = label;
		Comparator = null;
	}

	public JumpInstruction(Unit unit, ComparisonOperator comparator, bool invert, bool signed, Label label) : base(unit, InstructionType.JUMP)
	{
		if (Instructions.Count == 0)
		{
			Initialize();
		}

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
		var instruction = Comparator == null ? (Assembler.IsArm64 ? ARM64_CONDITIONLESS_JUMP : X64_CONDITIONLESS_JUMP) : Instructions[Comparator][IsSigned ? 0 : 1];

		Build($"{instruction} {Label.GetName()}");
	}
}