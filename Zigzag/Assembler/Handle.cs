using System;

public enum HandleType
{
    STACK_MEMORY_HANDLE,
    CONSTANT,
    REGISTER,
    NONE
}

public class Handle
{
    public HandleType Type { get; private set; }
    //public object Metadata { get; set; } = new object();
    //public Lifetime Lifetime { get; private set; } = new Lifetime();

    public Handle()
    {
        Type = HandleType.NONE;
    }

    public Handle(HandleType type)
    {
        Type = type;
    }

    /*public bool IsDying(Unit unit)
    {
        return !Lifetime.IsActive(unit.Position + 1);
    }

    public bool IsAlive(int position)
    {
        return Lifetime.IsActive(position);
    }

    public void AddUsage(int position)
    {
        if (position > Lifetime.End)
        {
            Lifetime.End = position;
        }

        if (position < Lifetime.Start)
        {
            Lifetime.Start = position;
        }
    }*/

    public override string ToString()
    {
        throw new NotImplementedException("Missing text conversion from handle");
    }
}

public class ConstantHandle : Handle
{
    public object Value { get; private set; }

    public ConstantHandle(object value) : base(HandleType.CONSTANT)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? throw new NullReferenceException("Constant value was missing");
    }
}

public class StackMemoryHandle : Handle
{
    public int Offset { get; private set; }

    public StackMemoryHandle(int offset) : base(HandleType.STACK_MEMORY_HANDLE)
    {
        Offset = offset;
    }

    public override string ToString()
    {
        var offset = string.Empty;

        if (Offset > 0)
        {
            offset = $"+{Offset}";
        }
        else if (Offset < 0)
        {
            offset = Offset.ToString();
        }

        return $"[{Assembler.BasePointer}{offset}]";
    }
}

public class RegisterHandle : Handle
{
    public Register Register { get; private set; }

    public RegisterHandle(Register register) : base(HandleType.REGISTER)
    {
        Register = register;
    }

    public override string ToString()
    {
        return Register.Name;
    }
}