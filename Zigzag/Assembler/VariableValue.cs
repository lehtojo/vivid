public class VariableValue : Value
{
	public Variable Variable { get; private set; }

	public VariableValue(Register register, Variable variable) : base(register, Size.Get(variable.Type.Size), ValueType.VARIABLE, true, false, true)
	{
		Variable = variable;
	}

	private VariableValue(VariableValue value) : base(value)
	{
		Variable = value.Variable;
	}

	public override Value Clone(Register register)
	{
		VariableValue clone = new VariableValue(this);
		register.Attach(clone);
		return clone;
	}
}