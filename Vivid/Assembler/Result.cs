using System.Collections.Generic;

public class Result
{
	public Handle Value { get; set; }
	public Format Format { get; set; } = Settings.Size.ToFormat();
	public Lifetime Lifetime { get; set; } = new Lifetime();
	public Size Size => Size.FromFormat(Format);

	public bool IsExpression => Value.Type == HandleType.EXPRESSION;
	public bool IsConstant => Value.Type == HandleType.CONSTANT;
	public bool IsStandardRegister => Value.Type == HandleType.REGISTER;
	public bool IsMediaRegister => Value.Type == HandleType.MEDIA_REGISTER;
	public bool IsAnyRegister => Value.Type == HandleType.REGISTER || Value.Type == HandleType.MEDIA_REGISTER;
	public bool IsMemoryAddress => Value.Type == HandleType.MEMORY;
	public bool IsModifier => Value.Type == HandleType.MODIFIER;
	public bool IsEmpty => Value.Type == HandleType.NONE;
	public bool IsStackVariable => Value.Instance == HandleInstanceType.STACK_VARIABLE;
	public bool IsDataSectionHandle => Value.Instance == HandleInstanceType.DATA_SECTION || Value.Instance == HandleInstanceType.CONSTANT_DATA_SECTION;
	public bool IsStackAllocation => Value.Instance == HandleInstanceType.STACK_ALLOCATION;
	public bool IsUnsigned => Format.IsUnsigned();

	public Register Register => Value.To<RegisterHandle>().Register;

	public Result(Handle value, Format format)
	{
		Value = value;
		Format = format;
	}

	public Result()
	{
		Value = new Handle();
		Format = Settings.Format;
	}

	public bool IsActive()
	{
		return Lifetime.IsActive();
	}

	public bool IsOnlyActive()
	{
		return Lifetime.IsOnlyActive();
	}

	public bool IsDeactivating()
	{
		return Lifetime.IsDeactivating();
	}

	public bool IsReleasable(Unit unit)
	{
		return unit.IsVariableValue(this);
	}

	public void Use(Instruction instruction)
	{
		var contains = false;

		foreach (var usage in Lifetime.Usages)
		{
			if (!ReferenceEquals(usage, instruction)) continue;
			contains = true;
			break;
		}

		if (!contains) { Lifetime.Usages.Add(instruction); }

		Value.Use(instruction);
	}

	public void Use(List<Instruction> instructions)
	{
		foreach (var instruction in instructions) { Use(instruction); }
	}

	public override string ToString()
	{
		return Value.ToString() ?? string.Empty;
	}
}