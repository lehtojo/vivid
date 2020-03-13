using System;

public static class RegisterFlag
{
    public const int VOLATILE = 1;
    public const int SPECIALIZED = 2;
    public const int RETURN = 4;
}

public class Register
{
    public String Name { get; private set; }
    public Quantum<Handle>? Value { get; set; } = null;

    public int Flags { get; private set; }
    public bool Volatile => Flag.Has(Flags, RegisterFlag.VOLATILE);
    public bool Specialized => Flag.Has(Flags, RegisterFlag.SPECIALIZED);

    public Register(string name, params int[] flags) 
    {
        Name = name;
        Flags = Flag.Combine(flags);
    }

    public bool IsAvailable(Unit unit)
    {
        return Value == null || !Value.Value.IsAlive(unit.Position);
    }

    public override string ToString()
    {
        return Name;
    }
}