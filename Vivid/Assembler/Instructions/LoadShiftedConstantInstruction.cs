using System.Collections.Generic;

/// <summary>
/// Shifts the specified value with the specified amount and loads the value into the specified destination
/// This instruction is works only on architecture arm64
/// </summary>
public class LoadShiftedConstantInstruction : Instruction
{
	public new Result Destination { get; private set; }
	public long Value { get; private set; }
	public int Shift { get; private set; }

	public LoadShiftedConstantInstruction(Unit unit, Result destination, ushort value, int shift) : base(unit, InstructionType.LOAD_SHIFTED_CONSTANT)
	{
		Destination = destination;
		Value = value;
		Shift = shift;
		Dependencies = new List<Result> { Result, Destination };
	}

	public override void OnBuild()
	{
		Build(
			Instructions.Arm64.LOAD_SHIFTED_CONSTANT,
			new InstructionParameter(
				Destination,
				ParameterFlag.DESTINATION | ParameterFlag.NO_ATTACH | ParameterFlag.WRITE_ACCESS | ParameterFlag.READS,
				HandleType.REGISTER
			),
			new InstructionParameter(
				new Result(new ConstantHandle(Value), Settings.Format),
				ParameterFlag.NONE,
				HandleType.CONSTANT
			),
			new InstructionParameter(
				new Result(new ModifierHandle($"{Instructions.Arm64.SHIFT_LEFT} #{Shift}"), Settings.Format),
				ParameterFlag.NONE,
				HandleType.MODIFIER
			)
		);
	}
}