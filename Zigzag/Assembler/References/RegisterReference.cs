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

	public override string Use(Size size)
	{
		return Register.Partitions[size];
	}

	public override LocationType GetType()
	{
		return LocationType.REGISTER;
	}
}