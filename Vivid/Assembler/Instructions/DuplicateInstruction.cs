/// <summary>
/// Duplicates the specified value by loading it to another register
/// This instruction is works on all architectures
/// </summary>
public class DuplicateInstruction : DualParameterInstruction
{
	public DuplicateInstruction(Unit unit, Result value) : base(unit, new Result(), value, value.Format, InstructionType.DUPLICATE) 
	{
		IsAbstract = true;
		Description = "Duplicates a value using registers";
	}

	public override void OnBuild()
	{
		Result.Format = Second.Format;
		
		Unit.Append(new MoveInstruction(Unit, Result, Second)
		{
			Type = MoveType.LOAD,
			IsSafe = true
		});
	}
}