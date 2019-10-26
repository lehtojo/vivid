using System.Collections.Generic;

public class Register
{
	public Dictionary<Size, string> Partitions { get; private set; }
	public Value Value { get; private set; }

	public bool IsCritical => Value != null && Value.IsCritical;
	public bool IsAvailable => Value == null;
	public bool IsReserved => Value != null;

	/**
     * Creates a register with partitions
     * @param partitions Partitions of the register (for example: rax, eax, ax, al)
     */
	public Register(Dictionary<Size, string> partitions)
	{
		Partitions = partitions;
	}

	/**
     * Copies the state of the given register
     * @param register Register to copy
     */
	private Register(Register register)
	{
		Partitions = register.Partitions;

		if (register.IsReserved)
		{
			Value = register.Value.Clone(this);
		}
	}

	/**
     * Exchanges values between the given register
     */
	public void Exchange(Register register)
	{
		Value other = register.Value;
		register.Attach(Value);
		Attach(other);
	}

	/**
     * Attaches value to this register
     * @param value Value to attach
     */
	public void Attach(Value value)
	{
		Value = value;
		Value.SetReference(this);
	}

	/**
     * Returns whether this register contains the given variable
     * @param variable Variable to test
     * @return True if this register holds the variable, otherwise false
     */
	public bool Contains(Variable variable)
	{
		if (Value != null && Value.Type == ValueType.VARIABLE)
		{
			return ((VariableValue)Value).Variable == variable;
		}

		return false;
	}

	public void Reset()
	{
		Value = null;
	}

	public override string ToString()
	{
		return Partitions[Size.DWORD];
	}

	public Register Clone()
	{
		return new Register(this);
	}
}