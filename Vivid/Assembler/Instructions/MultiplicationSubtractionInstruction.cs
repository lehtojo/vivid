using System.Collections.Generic;

/// <summary>
/// Subtracts the specified values together
/// This instruction is works only on architecture arm64
/// </summary>
public class MultiplicationSubtractionInstruction : Instruction
{
	public Result Multiplicand { get; private set; }
	public Result Multiplier { get; private set; }
	public Result Minuend { get; private set; }
	public Format Format { get; private set; }

	public bool Assigns { get; private set; }

	public MultiplicationSubtractionInstruction(Unit unit, Result multiplicand, Result multiplier, Result minuend, Format format, bool assigns) : base(unit, InstructionType.MULTIPLICATION_SUBTRACTION)
	{
		Multiplicand = multiplicand;
		Multiplier = multiplier;
		Minuend = minuend;
		Assigns = assigns;
		Format = format;
		Dependencies = new List<Result> { Multiplicand, Multiplier, Minuend, Result };
	}

	public override void OnBuild()
	{
		if (Assigns)
		{
			var result = Memory.LoadOperand(Unit, Minuend, false, Assigns);

			Build(
				Instructions.Arm64.MULTIPLY_SUBTRACT,
				Settings.Size,
				new InstructionParameter(
					result,
					ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Multiplicand,
					ParameterFlag.CreateBitLimit(12),
					HandleType.REGISTER
				),
				new InstructionParameter(
					Multiplier,
					ParameterFlag.CreateBitLimit(12),
					HandleType.REGISTER
				),
				new InstructionParameter(
					result,
					ParameterFlag.NONE,
					HandleType.REGISTER
				)
			);

			if (Minuend.IsMemoryAddress)
			{
				Unit.Add(new MoveInstruction(Unit, Minuend, result), true);
			}

			Result.Format = Format;
			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, Format.IsUnsigned(), false);

		Build(
			Instructions.Arm64.MULTIPLY_SUBTRACT,
			Settings.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.WRITE_ACCESS | ParameterFlag.NO_ATTACH,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Multiplicand,
				ParameterFlag.CreateBitLimit(12),
				HandleType.REGISTER
			),
			new InstructionParameter(
				Multiplier,
				ParameterFlag.CreateBitLimit(12),
				HandleType.REGISTER
			),
			new InstructionParameter(
				Minuend,
				ParameterFlag.NONE,
				HandleType.REGISTER
			)
		);

		Result.Format = Format;
	}
}