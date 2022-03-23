/// <summary>
/// Converts the specified number into the specified format
/// This instruction is works on all architectures
/// </summary>
public class ConvertInstruction : Instruction
{
	public Result Number { get; private set; }
	public Format Format { get; private set; }

	public ConvertInstruction(Unit unit, Result number, Format format) : base(unit, InstructionType.CONVERT)
	{
		Number = number;
		Dependencies = new[] { Number, Result };
		Format = format;
		IsAbstract = true;

		Result.Format = Format.IsDecimal() ? Format.DECIMAL : GetSystemFormat(format);
	}

	public override void OnBuild()
	{
		if (Number.Format.IsDecimal() == Format.IsDecimal())
		{
			// The result must be equal to the value if there is no needed for conversion, since the result is directly used
			Result.Join(Number);
			return;
		}

		Memory.GetRegisterFor(Unit, Result, Format.IsUnsigned(), Format.IsDecimal());

		Unit.Append(new MoveInstruction(Unit, Result, Number)
		{
			Type = MoveType.LOAD
		});
	}
}