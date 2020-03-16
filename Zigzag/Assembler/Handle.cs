using System;

public enum HandleType
{
    MEMORY_HANDLE,
    CONSTANT,
    REGISTER,
    NONE
}

public class Handle
{
    public HandleType Type { get; private set; }

    public Handle()
    {
        Type = HandleType.NONE;
    }

    public Handle(HandleType type)
    {
        Type = type;
    }

    public virtual void AddUsage(int position) {}

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

public class MemoryHandle : Handle
{
    public Result Base { get; private set; }
    public int Offset { get; private set; }

    public static MemoryHandle FromStack(Unit unit, int offset)
    {
        return new MemoryHandle(new Result(new RegisterHandle(unit.GetBasePointer())), offset);
    }

    public MemoryHandle(Result @base, int offset) : base(HandleType.MEMORY_HANDLE)
    {
        Base = @base;
        Offset = offset;
    }

    public override void AddUsage(int position)
    {
        Base.AddUsage(position);
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

        if (Base.Value.Type == HandleType.REGISTER ||
            Base.Value.Type == HandleType.CONSTANT)
        {
            return $"[{Base.Value}{offset}]";
        }
        
        throw new ApplicationException("Base of the memory handle was no longer in register");
    }
}

public class ComplexMemoryHandle : Handle
{
    public Result Base { get; private set; }
    public Result Offset { get; private set; }
    public int Stride { get; private set; }

    public ComplexMemoryHandle(Result @base, Result offset, int stride) : base(HandleType.MEMORY_HANDLE)
    {
        Base = @base;
        Offset = offset;
        Stride = stride;
    }

    public override void AddUsage(int position)
    {
        Base.AddUsage(position);
        Offset.AddUsage(position);
    }

    public override string ToString()
    {
        var offset = string.Empty;

        if (Offset.Value.Type == HandleType.REGISTER)
        {
            offset = "+" + Offset.ToString() + (Stride == 1 ? string.Empty : $"*{Stride}");
        }
        else if (Offset.Value is ConstantHandle constant)
        {
            var index = (Int64)constant.Value;
            var value = index * Stride;

            if (value > 0)
            {
                offset = $"+{value}";
            }
            else if (value < 0)
            {
                offset = value.ToString();
            }
        }
        else
        {
            throw new ApplicationException("Complex memory address's offset wasn't a constant or in a register");
        }

        if (Base.Value.Type == HandleType.REGISTER ||
            Base.Value.Type == HandleType.CONSTANT)
        {
            return $"[{Base.Value}{offset}]";
        }
        
        throw new ApplicationException("Base of the memory handle was no longer in register");
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