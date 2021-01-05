/// <summary>
/// Substracts the specified values together
/// This instruction is works only on architecture Arm64
/// </summary>
public class MultiplicationSubtractionInstruction : Instruction
{
	public const string ARM64_MULTIPLICATION_SUBTRACTION_INSTRUCTION = "msub";

	public Result Multiplicand { get; private set; }
	public Result Multiplier { get; private set; }
	public Result Minued { get; private set; }
	public Format Format { get; private set; }

	public bool Assigns { get; private set; }

	public MultiplicationSubtractionInstruction(Unit unit, Result multiplicand, Result multiplier, Result minued, Format format, bool assigns) : base(unit, InstructionType.MULTIPLICATION_SUBTRACTION)
	{
		Multiplicand = multiplicand;
		Multiplier = multiplier;
		Minued = minued;
		Assigns = assigns;
		Format = format;
		Dependencies = new[] { Multiplicand, Multiplier, Minued, Result };
	}

	public override void OnBuild()
	{
		if (Assigns)
		{
			var result = Memory.LoadOperand(Unit, Minued, false, Assigns);

			Build(
				ARM64_MULTIPLICATION_SUBTRACTION_INSTRUCTION,
				Assembler.Size,
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

			if (Minued.IsMemoryAddress)
			{
				Unit.Append(new MoveInstruction(Unit, Minued, result), true);
			}

			Result.Format = Format;
			return;
		}

		Memory.GetResultRegisterFor(Unit, Result, false);

		Build(
			ARM64_MULTIPLICATION_SUBTRACTION_INSTRUCTION,
			Assembler.Size,
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
				Minued,
				ParameterFlag.NONE,
				HandleType.REGISTER
			)
		);
		
		Result.Format = Format;
	}
}