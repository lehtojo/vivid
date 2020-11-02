using System;

public class ExtendNumeratorInstruction : Instruction
{
	public const string INSTRUCTION_X86 = "cdq";
	public const string INSTRUCTION_X64 = "cqo";

	public ExtendNumeratorInstruction(Unit unit) : base(unit) { }

	public override void OnBuild()
	{
		Build(Assembler.IsTargetX64 ? INSTRUCTION_X64 : INSTRUCTION_X86);
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
