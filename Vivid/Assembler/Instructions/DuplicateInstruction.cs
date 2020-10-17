
public class DuplicateInstruction : DualParameterInstruction
{
	public DuplicateInstruction(Unit unit, Result value) : base(unit, new Result(), value, value.Format) { }

	public override void OnBuild()
	{
		Unit.Append(new MoveInstruction(Unit, Result, Second)
		{
			Type = MoveType.LOAD,
			IsSafe = true
		});
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.DUPLICATE;
	}
}