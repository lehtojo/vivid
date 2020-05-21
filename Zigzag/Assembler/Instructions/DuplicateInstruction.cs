
public class DuplicateInstruction : DualParameterInstruction
{
	public DuplicateInstruction(Unit unit, Result value) : base(unit, new Result(), value) {}

	public override void OnBuild()
	{
		if (Result.Empty)
		{
			Result.Value = new RegisterHandle(Unit.GetNextRegister());
		}

		var move = new MoveInstruction(Unit, Result, Second);
		move.Type = MoveType.LOAD;

		Unit.Append(move);
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