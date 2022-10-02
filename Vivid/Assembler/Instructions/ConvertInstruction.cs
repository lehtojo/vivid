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
		Format = format;
		Dependencies!.Add(Number);
		IsAbstract = true;
		Description = "Converts the specified number into the specified format";

		Result.Format = Format.IsDecimal() ? Format.DECIMAL : GetSystemFormat(format);
	}

	public override void OnBuild()
	{
		Memory.GetResultRegisterFor(Unit, Result, Format.IsUnsigned(), Format.IsDecimal());

		Unit.Add(new MoveInstruction(Unit, Result, Number)
		{
			Type = MoveType.LOAD,
			Description = "Loads the specified number into the specified register"
		});
	}
}