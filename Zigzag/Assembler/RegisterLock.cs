using System;

public class RegisterLock : IDisposable
{
    public Register Register { get; private set; }

    public RegisterLock(Register register)
    {
        Register = register;
        Register.IsLocked = true;
    }

    public void Dispose()
    {
        Register.IsLocked = false;
    }
}