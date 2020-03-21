using System;

public static class RegisterFlag
{
    public const int VOLATILE = 1;
    public const int RESERVED = 2;
    public const int RETURN = 4;
    public const int BASE_POINTER = 8;
    public const int STACK_POINTER = 16;
}

public class Register
{
    public String Name { get; private set; }
    public Result? Value { get; set; } = null;

    public int Flags { get; private set; }
    
    public bool IsVolatile => Flag.Has(Flags, RegisterFlag.VOLATILE);
    public bool IsReserved => Flag.Has(Flags, RegisterFlag.RESERVED);
    public bool IsReturnRegister => Flag.Has(Flags, RegisterFlag.RETURN);
    public bool IsReleasable => Value == null || Value.Relesable;

    public Register(string name, params int[] flags) 
    {
        Name = name;
        Flags = Flag.Combine(flags);
    }

    public bool IsAvailable(Unit unit)
    {
        return Value == null || !Value.IsAlive(unit.Position);
    }

    public override string ToString()
    {
        return Name;
    }
}