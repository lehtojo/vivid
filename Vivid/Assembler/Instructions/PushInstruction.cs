/// <summary>
/// Allocates memory from the stack and moves the specified variable there
/// This instruction works only on architecture x86-64
/// </summary>
public class PushInstruction : Instruction
{
	public const string INSTRUCTION = "push";

	public Result Value { get; private set; }

	public PushInstruction(Unit unit, Result value) : base(unit)
	{
		Value = value;
		Result.Format = Value.Format;
	}

	public override void OnBuild()
	{
		Build(
			INSTRUCTION,
			Assembler.Size,
			new InstructionParameter(
				Value,
				ParameterFlag.NONE,
				HandleType.CONSTANT,
				HandleType.REGISTER,
				HandleType.MEMORY
			)
		);
	}

	public override int GetStackOffsetChange()
	{
		return Assembler.Size.Bytes;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.PUSH;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Value };
	}
}