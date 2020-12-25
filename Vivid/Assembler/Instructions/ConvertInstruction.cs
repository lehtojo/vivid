/// <summary>
/// Converts the specified number into the specified format
/// This instruction is works on all architectures
/// </summary>
public class ConvertInstruction : Instruction
{
	public Result Number { get; private set; }
	public bool ToInteger { get; private set; }

	public ConvertInstruction(Unit unit, Result number, bool to_integer) : base(unit)
	{
		Number = number;
		ToInteger = to_integer;
		Result.Format = ToInteger ? Assembler.Format : Format.DECIMAL;
	}

	public override void OnBuild()
	{
		if (Number.Format.IsDecimal() == !ToInteger)
		{
			// The result must be equal to the value if there is no needed for conversion, since the result is directly used
			Result.Join(Number);
			return;
		}

		Memory.GetRegisterFor(Unit, Result, !ToInteger);

		Unit.Append(new MoveInstruction(Unit, Result, Number)
		{
			Type = MoveType.LOAD
		});
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Number, Result };
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.CONVERT;
	}
}