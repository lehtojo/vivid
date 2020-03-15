using System;

public static class RegisterFlag
{
    public const int VOLATILE = 1;
    public const int SPECIALIZED = 2;
    public const int RETURN = 4;
    public const int BASE_POINTER = 8;
    public const int STACK_POINTER = 16;
}

public class Register
{
    public String Name { get; private set; }
    public Result? Value { get; set; } = null;

    public int Flags { get; private set; }
    public bool Volatile => Flag.Has(Flags, RegisterFlag.VOLATILE);
    public bool Specialized => Flag.Has(Flags, RegisterFlag.SPECIALIZED);
    public bool Releasable => Value == null || Value.Relesable;

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