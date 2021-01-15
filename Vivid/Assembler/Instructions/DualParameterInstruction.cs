public abstract class DualParameterInstruction : Instruction
{
	public Result First { get; private set; }
	public Result Second { get; private set; }

	public Format Format => Result.Format;

	public DualParameterInstruction(Unit unit, Result first, Result second, Format format, InstructionType type) : base(unit, type)
	{
		First = first;
		Second = second;
		Result.Format = format;

		Dependencies = new[] { Result, First, Second };
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, First, Second };
	}
}