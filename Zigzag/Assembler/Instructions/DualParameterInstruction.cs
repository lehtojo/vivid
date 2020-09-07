public abstract class DualParameterInstruction : Instruction
{
	public Result First { get; private set; }
	public Result Second { get; private set; }

	public InstructionParameter? Source => Parameters.Find(p => !p.IsDestination);

	public DualParameterInstruction(Unit unit, Result first, Result second, Format format) : base(unit)
	{
		First = first;
		Second = second;
		Result.Format = format;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, First, Second };
	}
}