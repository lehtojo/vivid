/// <summary>
/// Multiplicates the specified values together while supporting larger output number
/// This instruction works only on architecture x86-64
/// </summary>
public class LongMultiplicationInstruction : DualParameterInstruction
{
	public bool IsUnsigned { get; set; }

	public LongMultiplicationInstruction(Unit unit, Result first, Result second, bool is_unsigned) : base(unit, first, second, GetSystemFormat(is_unsigned), InstructionType.LONG_MULTIPLICATION)
	{
		IsUnsigned = is_unsigned;
	}

	private Result CorrectDestinationOperandLocation()
	{
		var numerator = Unit.GetNumeratorRegister();
		var location = new RegisterHandle(numerator);

		if (!First.Value.Equals(location))
		{
			Memory.ClearRegister(Unit, location.Register);

			return new MoveInstruction(Unit, new Result(location, GetSystemFormat(Unsigned)), First)
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
		return new Result(new RegisterHandle(numerator), GetSystemFormat(Unsigned));
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
			IsUnsigned ? Instructions.X64.UNSIGNED_MULTIPLY : Instructions.X64.SIGNED_MULTIPLY,
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