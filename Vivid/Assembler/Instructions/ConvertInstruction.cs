using System;
using System.Collections.Generic;
using System.Text;

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
			return;
		}

		Memory.MoveToRegister(Unit, Number, Size.QWORD, !ToInteger, Result.GetRecommendation(Unit));
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