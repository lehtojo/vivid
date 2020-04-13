using System;
using System.Collections.Generic;

public enum HandleType
{
    MEMORY,
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

    public T? As<T>() where T : Handle 
    {
        return this as T;
    }

    public T To<T>() where T : Handle 
    {
        return (T)this;
    }

    public virtual void Use(int position) {}

    public override string ToString()
    {
        throw new NotImplementedException("Missing text conversion from handle");
    }
}

public class DataSectionHandle : Handle
{
    public string Identifier { get; private set; }

    public DataSectionHandle(string identifier) : base(HandleType.MEMORY)
    {
        Identifier = identifier;
    }

    public override string ToString()
    {
        return $"[{Identifier}]";
    }

    public override bool Equals(object? obj)
    {
        return obj is DataSectionHandle handle &&
               Type == handle.Type &&
               Identifier == handle.Identifier;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Identifier);
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

    public override bool Equals(object? obj)
    {
        return obj is ConstantHandle handle &&
               EqualityComparer<object>.Default.Equals(Value, handle.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value);
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

    public MemoryHandle(Result @base, int offset) : base(HandleType.MEMORY)
    {
        Base = @base;
        Offset = offset;
    }

    public override void Use(int position)
    {
        Base.Use(position);
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

    public override bool Equals(object? obj)
    {
        return obj is MemoryHandle handle &&
               EqualityComparer<Result>.Default.Equals(Base, handle.Base) &&
               Offset == handle.Offset;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Base, Offset);
    }
}

public class ComplexMemoryHandle : Handle
{
    public Result Base { get; private set; }
    public Result Offset { get; private set; }
    public int Stride { get; private set; }

    public ComplexMemoryHandle(Result @base, Result offset, int stride) : base(HandleType.MEMORY)
    {
        Base = @base;
        Offset = offset;
        Stride = stride;
    }

    public override void Use(int position)
    {
        Base.Use(position);
        Offset.Use(position);
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

    public override bool Equals(object? obj)
    {
        return obj is ComplexMemoryHandle handle &&
               EqualityComparer<Result>.Default.Equals(Base, handle.Base) &&
               EqualityComparer<Result>.Default.Equals(Offset, handle.Offset) &&
               Stride == handle.Stride;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Base, Offset, Stride);
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

    public override bool Equals(object? obj)
    {
        return obj is RegisterHandle handle &&
               EqualityComparer<Register>.Default.Equals(Register, handle.Register);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Register);
    }
}