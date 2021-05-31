using System;
using System.Collections.Generic;

/// <summary>
/// Jumps to the specified label and optionally checks a condition
/// This instruction works on all architectures
/// </summary>
public class JumpInstruction : Instruction
{
	private static readonly Dictionary<ComparisonOperator, string[]> Jumps = new();

	public static void Initialize()
	{
		if (Assembler.IsArm64)
		{
			Jumps.Add(Operators.GREATER_THAN,     new[] { Instructions.Arm64.JUMP_GREATER_THAN,           Instructions.Arm64.JUMP_GREATER_THAN });
			Jumps.Add(Operators.GREATER_OR_EQUAL, new[] { Instructions.Arm64.JUMP_GREATER_THAN_OR_EQUALS, Instructions.Arm64.JUMP_GREATER_THAN_OR_EQUALS });
			Jumps.Add(Operators.LESS_THAN,        new[] { Instructions.Arm64.JUMP_LESS_THAN,              Instructions.Arm64.JUMP_LESS_THAN });
			Jumps.Add(Operators.LESS_OR_EQUAL,    new[] { Instructions.Arm64.JUMP_LESS_THAN_OR_EQUALS,    Instructions.Arm64.JUMP_LESS_THAN_OR_EQUALS });
			Jumps.Add(Operators.EQUALS,           new[] { Instructions.Arm64.JUMP_EQUALS,                 Instructions.Arm64.JUMP_EQUALS });
			Jumps.Add(Operators.NOT_EQUALS,       new[] { Instructions.Arm64.JUMP_NOT_EQUALS,             Instructions.Arm64.JUMP_NOT_EQUALS });
			return;
		}

		Jumps.Add(Operators.GREATER_THAN,     new[] { Instructions.X64.JUMP_GREATER_THAN,           Instructions.X64.JUMP_ABOVE });
		Jumps.Add(Operators.GREATER_OR_EQUAL, new[] { Instructions.X64.JUMP_GREATER_THAN_OR_EQUALS, Instructions.X64.JUMP_ABOVE_OR_EQUALS });
		Jumps.Add(Operators.LESS_THAN,        new[] { Instructions.X64.JUMP_LESS_THAN,              Instructions.X64.JUMP_BELOW });
		Jumps.Add(Operators.LESS_OR_EQUAL,    new[] { Instructions.X64.JUMP_LESS_THAN_OR_EQUALS,    Instructions.X64.JUMP_BELOW_OR_EQUALS });
		Jumps.Add(Operators.EQUALS,           new[] { Instructions.X64.JUMP_EQUALS,                 Instructions.X64.JUMP_ZERO });
		Jumps.Add(Operators.NOT_EQUALS,       new[] { Instructions.X64.JUMP_NOT_EQUALS,             Instructions.X64.JUMP_NOT_ZERO });
	}

	public Label Label { get; set; }
	public ComparisonOperator? Comparator { get; private set; }
	public bool IsConditional => Comparator != null;
	public bool IsSigned { get; private set; } = true;

	public JumpInstruction(Unit unit, Label label) : base(unit, InstructionType.JUMP)
	{
		if (Jumps.Count == 0)
		{
			Initialize();
		}

		Label = label;
		Comparator = null;
	}

	public JumpInstruction(Unit unit, ComparisonOperator comparator, bool invert, bool signed, Label label) : base(unit, InstructionType.JUMP)
	{
		if (Jumps.Count == 0)
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
		var instruction = Comparator == null ? (Assembler.IsArm64 ? Instructions.Arm64.JUMP_LABEL : Instructions.X64.JUMP) : Jumps[Comparator][IsSigned ? 0 : 1];

		Build($"{instruction} {Label.GetName()}");
	}
}