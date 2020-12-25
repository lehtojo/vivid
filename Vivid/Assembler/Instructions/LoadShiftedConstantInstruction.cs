/// <summary>
/// Shifts the specified value with the specified amount and loads the value into the specified destination
/// This instruction is works only on architecture Arm64
/// </summary>
public class LoadShiftedConstantInstruction : Instruction
{
	public const string ARM64_LOGICAL_SHIFT_LEFT = "lsl";
	public const string ARM64_LOAD_SHIFTED_CONSTANT = "movk";

	public new Result Destination { get; private set; }
	public long Value { get; private set; }
	public int Shift { get; private set; }

	public LoadShiftedConstantInstruction(Unit unit, Result destination, ushort value, int shift) : base(unit)
	{
		Destination = destination;
		Value = value;
		Shift = shift;
	}

	public override void OnBuild()
	{
		Build(
			ARM64_LOAD_SHIFTED_CONSTANT,
			new InstructionParameter(
				Destination,
				ParameterFlag.DESTINATION | ParameterFlag.NO_ATTACH | ParameterFlag.WRITE_ACCESS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(new ConstantHandle(Value), Assembler.Format),
				ParameterFlag.NONE,
				HandleType.CONSTANT
			),
			new InstructionParameter(
				new Result(new ModifierHandle($"{ARM64_LOGICAL_SHIFT_LEFT} #{Shift}"), Assembler.Format),
				ParameterFlag.NONE,
				HandleType.MODIFIER
			)
		);
	}

	public override Result? GetDestinationDependency()
	{
		return Result;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.LOAD_SHIFTED_CONSTANT;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result, Destination };
	}
}