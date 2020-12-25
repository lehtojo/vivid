/// <summary>
/// Multiplicates the specified values together while supporting larger output number
/// This instruction works only on architecture x86-64
/// </summary>
public class LongMultiplicationInstruction : DualParameterInstruction
{
	private const string MULTPLICATION_INSTRUCTION = "mul";
	private const string SIGNED_MULTPLICATION_INSTRUCTION = "imul";

	public string Instruction { get; private set; }

	public LongMultiplicationInstruction(Unit unit, Result first, Result second, Format format) : base(unit, first, second, format)
	{
		Instruction = format.IsUnsigned() ? MULTPLICATION_INSTRUCTION : SIGNED_MULTPLICATION_INSTRUCTION;
	}

	private Result CorrectDestinationOperandLocation()
	{
		var register = Unit.GetNumeratorRegister();
		var location = new RegisterHandle(register);

		if (!First.Value.Equals(location))
		{
			Memory.ClearRegister(Unit, location.Register);

			return new MoveInstruction(Unit, new Result(location, First.Format), First)
			{
				Type = MoveType.COPY

			}.Execute();
		}

		return First;
	}

	public override void OnBuild()
	{
		CorrectDestinationOperandLocation();

		// The remainder register must be empty since it will contain
		var remainder = Unit.GetRemainderRegister();
		Memory.ClearRegister(Unit, remainder);

		using (RegisterLock.Create(remainder))
		{
			Result.Value = new RegisterHandle(remainder);

			Build(
				Instruction,
				Assembler.Size,
				new InstructionParameter(
					Result,
					ParameterFlag.DESTINATION | ParameterFlag.READS | ParameterFlag.HIDDEN,
					HandleType.REGISTER
				),
				new InstructionParameter(
					Second,
					ParameterFlag.NONE,
					HandleType.CONSTANT,
					HandleType.REGISTER,
					HandleType.MEMORY
				)
			);
		}
	}

	public override Result? GetDestinationDependency()
	{
		return First;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.LONG_MULTIPLICATION;
	}
}