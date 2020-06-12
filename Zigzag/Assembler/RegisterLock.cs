using System;

public class RegisterLock : IDisposable
{
	public Register Register { get; private set; }

	/// <summary>
	/// Locks the register until this lock is destroyed
	/// </summary>    
	public static RegisterLock Create(Result result)
	{
		return new RegisterLock(result.Value.To<RegisterHandle>().Register);
	}

	/// <summary>
	/// Locks the register until this lock is destroyed
	/// </summary>    
	public static RegisterLock Create(Register register)
	{
		return new RegisterLock(register);
	}

	/// <summary>
	/// Locks the register until this lock is destroyed
	/// </summary>
	public RegisterLock(Register register)
	{
		Register = register;
		Register.IsLocked = true;
	}

	/// <summary>
	/// Releases the register after using-statements
	/// </summary>
	public void Dispose()
	{
		Register.IsLocked = false;
	}
}