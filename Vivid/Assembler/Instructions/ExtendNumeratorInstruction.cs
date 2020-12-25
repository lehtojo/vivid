using System;

/// <summary>
/// Extends the sign of the quentient register
/// This instruction works only on architecture x86-64
/// </summary>
public class ExtendNumeratorInstruction : Instruction
{
	public const string X64_INSTRUCTION_32BIT_MODE = "cdq";
	public const string X64_INSTRUCTION_64BIT_MODE = "cqo";

	public ExtendNumeratorInstruction(Unit unit) : base(unit) { }

	public override void OnBuild()
	{
		Build(Assembler.Is64bit ? X64_INSTRUCTION_64BIT_MODE : X64_INSTRUCTION_32BIT_MODE);
	}

	public override Result? GetDestinationDependency()
	{
		throw new ApplicationException("Tried to redirect Extend-Numerator-Instruction");
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.EXTEND_NUMERATOR;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}
