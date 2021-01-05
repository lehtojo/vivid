/// <summary>
/// This instruction compares the two specified values together and alters the CPU flags based on the comparison
/// This instruction is works on all architectures
/// </summary>
public class CompareInstruction : DualParameterInstruction
{
	public const string SHARED_COMPARISON_INSTRUCTION = "cmp";

	public const string X64_SINGLE_PRECISION_DECIMAL_COMPARISON_INSTRUCTION = "comiss";
	public const string X64_DOUBLE_PRECISION_DECIMAL_COMPARISON_INSTRUCTION = "comisd";

	public const string ARM64_DECIMAL_COMPARISON_INSTRUCTION = "fcmp";

	public const string X64_ZERO_COMPARISON_INSTRUCTION = "test";

	public CompareInstruction(Unit unit, Result first, Result second) : base(unit, first, second, Assembler.Format, InstructionType.COMPARE) { }

	public override void OnBuild()
	{
		if (First.Format.IsDecimal() || Second.Format.IsDecimal())
		{
			var instruction = Assembler.Is64bit ? X64_DOUBLE_PRECISION_DECIMAL_COMPARISON_INSTRUCTION : X64_SINGLE_PRECISION_DECIMAL_COMPARISON_INSTRUCTION;

			if (Assembler.IsArm64)
			{
				instruction = ARM64_DECIMAL_COMPARISON_INSTRUCTION;
			}

			Build(
				instruction,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.MEDIA_REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.MEDIA_REGISTER
				)
			);
		}
		else if (Assembler.IsX64 && Second.IsConstant && Second.Value.To<ConstantHandle>().Value.Equals(0L))
		{
			Build(
				X64_ZERO_COMPARISON_INSTRUCTION,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				)
			);
		}
		else
		{
			var types = Assembler.IsX64 ? new[] { HandleType.CONSTANT, HandleType.REGISTER, HandleType.MEMORY } : new[] { HandleType.CONSTANT, HandleType.REGISTER };
			var flags_second = ParameterFlag.NONE;

			if (Assembler.IsArm64)
			{
				flags_second |= ParameterFlag.CreateBitLimit(8);
			}

			Build(
				SHARED_COMPARISON_INSTRUCTION,
				Assembler.Size,
				new InstructionParameter(
					First,
					ParameterFlag.NONE,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					flags_second,
					types
				)
			);
		}
	}
}