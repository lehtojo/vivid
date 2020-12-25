/// <summary>
/// Returns a handle to the specified constant value
/// This instruction works on all architectures
/// </summary>
public class GetConstantInstruction : Instruction
{
	public object Value { get; private set; }
	public Format Format { get; private set; }

	public GetConstantInstruction(Unit unit, object value, bool is_decimal) : base(unit)
	{
		Value = value;
		Format = is_decimal ? Format.DECIMAL : Assembler.Format;
		Description = $"Get a handle for constant '{value}'";

		Result.Value = References.CreateConstantNumber(value);
		Result.Format = Format;
	}

	public override void OnBuild()
	{
		Result.Value = References.CreateConstantNumber(Value);
		Result.Format = Format;
	}

	public override InstructionType GetInstructionType()
	{
		return InstructionType.GET_CONSTANT;
	}

	public override Result? GetDestinationDependency()
	{
		return null;
	}

	public override Result[] GetResultReferences()
	{
		return new[] { Result };
	}
}