/// <summary>
/// Returns a handle to the specified constant value
/// This instruction works on all architectures
/// </summary>
public class GetConstantInstruction : Instruction
{
	public object Value { get; private set; }
	public Format Format { get; private set; }

	public GetConstantInstruction(Unit unit, object value, bool is_decimal) : base(unit, InstructionType.GET_CONSTANT)
	{
		Value = value;
		Format = is_decimal ? Format.DECIMAL : Assembler.Format;
		Description = $"Get a handle for constant '{value}'";
		IsAbstract = true;

		Result.Value = References.CreateConstantNumber(value);
		Result.Format = Format;
	}

	public override void OnBuild()
	{
		Result.Value = References.CreateConstantNumber(Value);
		Result.Format = Format;
	}
}