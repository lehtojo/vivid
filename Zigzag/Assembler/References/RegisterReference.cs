using System.Collections.Generic;

public class RegisterReference : Reference
{
	public Register Register { get; private set; }

	public RegisterReference(Register register, Size size) : base(size)
	{
		Register = register;
	}

	public RegisterReference(Register register) : base(Size.DWORD)
	{
		Register = register;
	}

	public override bool IsRegister()
	{
		return true;
	}

	public override Register GetRegister()
	{
		return Register;
	}

	public override void Lock()
	{
		if (Register.Value != null)
		{
			Register.Value.IsCritical = true;
		}
	}

	public override string Use(Size size)
	{
		return Register.Partitions[size];
	}

	public override string Use()
	{
		return Register.ToString();
	}

	public override bool IsComplex()
	{
		return false;
	}

	public override LocationType GetType()
	{
		return LocationType.REGISTER;
	}

	public override bool Equals(object? obj)
	{
		return obj is RegisterReference reference &&
			   EqualityComparer<Register>.Default.Equals(Register, reference.Register);
	}
}