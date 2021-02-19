using System.Collections.Generic;
using System;

/// <summary>
/// Jumps to the specified label and optionally checks a condition
/// This instruction works on all architectures
/// </summary>
public class JumpInstruction : Instruction
{
	private static readonly Dictionary<ComparisonOperator, string[]> Instructions = new Dictionary<ComparisonOperator, string[]>();

	public static void Initialize()
	{
		if (Assembler.IsArm64)
		{
			Instructions.Add(Operators.GREATER_THAN,		new[] { global::Instructions.Arm64.JUMP_GREATER_THAN,					global::Instructions.Arm64.JUMP_GREATER_THAN });
			Instructions.Add(Operators.GREATER_OR_EQUAL,	new[] { global::Instructions.Arm64.JUMP_GREATER_THAN_OR_EQUALS,	global::Instructions.Arm64.JUMP_GREATER_THAN_OR_EQUALS });
			Instructions.Add(Operators.LESS_THAN,			new[] { global::Instructions.Arm64.JUMP_LESS_THAN,						global::Instructions.Arm64.JUMP_LESS_THAN });
			Instructions.Add(Operators.LESS_OR_EQUAL,		new[] { global::Instructions.Arm64.JUMP_LESS_THAN_OR_EQUALS,		global::Instructions.Arm64.JUMP_LESS_THAN_OR_EQUALS });
			Instructions.Add(Operators.EQUALS,				new[] { global::Instructions.Arm64.JUMP_EQUALS,							global::Instructions.Arm64.JUMP_EQUALS });
			Instructions.Add(Operators.NOT_EQUALS,			new[] { global::Instructions.Arm64.JUMP_NOT_EQUALS,					global::Instructions.Arm64.JUMP_NOT_EQUALS });
			return;
		}

		Instructions.Add(Operators.GREATER_THAN,		new[] { global::Instructions.X64.JUMP_GREATER_THAN,				global::Instructions.X64.JUMP_ABOVE });
		Instructions.Add(Operators.GREATER_OR_EQUAL, new[] { global::Instructions.X64.JUMP_GREATER_THAN_OR_EQUALS,	global::Instructions.X64.JUMP_ABOVE_OR_EQUALS });
		Instructions.Add(Operators.LESS_THAN,			new[] { global::Instructions.X64.JUMP_LESS_THAN,							global::Instructions.X64.JUMP_BELOW });
		Instructions.Add(Operators.LESS_OR_EQUAL,		new[] { global::Instructions.X64.JUMP_LESS_THAN_OR_EQUALS,				global::Instructions.X64.JUMP_BELOW_OR_EQUALS });
		Instructions.Add(Operators.EQUALS,				new[] { global::Instructions.X64.JUMP_EQUALS,						global::Instructions.X64.JUMP_ZERO });
		Instructions.Add(Operators.NOT_EQUALS,			new[] { global::Instructions.X64.JUMP_NOT_EQUALS,					global::Instructions.X64.JUMP_NOT_ZERO });
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
		var instruction = Comparator == null ? (Assembler.IsArm64 ? global::Instructions.Arm64.JUMP : global::Instructions.X64.JUMP) : Instructions[Comparator][IsSigned ? 0 : 1];

		Build($"{instruction} {Label.GetName()}");
	}
}