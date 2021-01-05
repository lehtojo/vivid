/// <summary>
/// Converts the specified number into the specified format
/// This instruction is works on all architectures
/// </summary>
public class ConvertInstruction : Instruction
{
	public Result Number { get; private set; }
	public bool Integer { get; private set; }

	public ConvertInstruction(Unit unit, Result number, bool to_integer) : base(unit, InstructionType.CONVERT)
	{
		Number = number;
		Dependencies = new[] { Number, Result };
		Integer = to_integer;

		Result.Format = Integer ? Assembler.Format : Format.DECIMAL;
	}

	public override void OnBuild()
	{
		if (Number.Format.IsDecimal() == !Integer)
		{
			// The result must be equal to the value if there is no needed for conversion, since the result is directly used
			Result.Join(Number);
			return;
		}

		Memory.GetRegisterFor(Unit, Result, !Integer);

		Unit.Append(new MoveInstruction(Unit, Result, Number)
		{
			Type = MoveType.LOAD
		});
	}
}