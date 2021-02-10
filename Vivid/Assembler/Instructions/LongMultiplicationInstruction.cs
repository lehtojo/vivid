/// <summary>
/// Multiplicates the specified values together while supporting larger output number
/// This instruction works only on architecture x86-64
/// </summary>
public class LongMultiplicationInstruction : DualParameterInstruction
{
	private const string X64_LONG_MULTPLICATION_INSTRUCTION = "mul";

	public string Instruction { get; private set; }

	public LongMultiplicationInstruction(Unit unit, Result first, Result second, Format format) : base(unit, first, second, format, InstructionType.LONG_MULTIPLICATION)
	{
		Instruction = X64_LONG_MULTPLICATION_INSTRUCTION;
	}

	private Result CorrectDestinationOperandLocation()
	{
		var numerator = Unit.GetNumeratorRegister();
		var location = new RegisterHandle(numerator);

		if (!First.Value.Equals(location))
		{
			Memory.ClearRegister(Unit, location.Register);

			return new MoveInstruction(Unit, new Result(location, Assembler.Format), First)
			{
				Type = MoveType.COPY

			}.Execute();
		}

		using var numerator_lock = new RegisterLock(numerator);

		// Get next register for the numerator where it will be relocated since it should not be edited
		var register = Memory.GetNextRegister(Unit, false, Trace.GetDirectives(Unit, First));

		Unit.Append(new MoveInstruction(Unit, new Result(new RegisterHandle(register), First.Format), First)
		{
			Type = MoveType.RELOCATE
		});

		// Even though the numerator is relocated the value is still in the register
		return new Result(new RegisterHandle(numerator), Assembler.Format);
	}

	private Register ClearRemainderRegister()
	{
		var numerator = Unit.GetNumeratorRegister();
		var remainder = Unit.GetRemainderRegister();

		using var numerator_lock = new RegisterLock(numerator);

		Memory.ClearRegister(Unit, remainder);

		return remainder;
	}

	public override void OnBuild()
	{
		var numerator = CorrectDestinationOperandLocation();
		var remainder = ClearRemainderRegister();

		using var remainder_lock = new RegisterLock(remainder);
		
		Result.Value = new RegisterHandle(remainder);

		Build(
			Instruction,
			Assembler.Size,
			new InstructionParameter(
				Result,
				ParameterFlag.DESTINATION | ParameterFlag.READS | ParameterFlag.HIDDEN | ParameterFlag.LOCKED,
				HandleType.REGISTER
			),
			new InstructionParameter(
				numerator,
				ParameterFlag.HIDDEN | ParameterFlag.LOCKED | ParameterFlag.WRITES,
				HandleType.REGISTER
			),
			new InstructionParameter(
				Second,
				ParameterFlag.NONE,
				HandleType.REGISTER,
				HandleType.MEMORY
			)
		);
	}
}