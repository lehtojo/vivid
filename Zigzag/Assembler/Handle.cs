using System;
using System.Collections.Generic;

public enum HandleType
{
	MEMORY,
	CONSTANT,
	REGISTER,
	MEDIA_REGISTER,
	NONE
}

public class Handle
{
	public HandleType Type { get; protected set; }
	public Size Size => Size.FromFormat(Format);
	public bool IsUnsigned => Format.IsUnsigned();
	public Format Format { get; set; }
	public bool IsSizeVisible { get; set; } = false;

	public Handle()
	{
		Type = HandleType.NONE;
		Format = Assembler.Size.ToFormat();
	}

	public Handle(HandleType type)
	{
		Type = type;
		Format = Assembler.Size.ToFormat();
	}

	public T To<T>() where T: Handle
	{
		return (T)this;
	}

	public virtual void Use(int position) { }
	public virtual Handle Freeze() 
	{
		return (Handle)this.MemberwiseClone();
	}

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
		if (IsSizeVisible)
		{
			return $"{Size} [{Identifier}]";
		}
		else
		{
			return $"[{Identifier}]";
		}
	}

	public override Handle Freeze() 
	{
		return (Handle)this.MemberwiseClone();
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

	public override Handle Freeze() 
	{
		return (Handle)this.MemberwiseClone();
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Value);
	}
}

public class VariableMemoryHandle : MemoryHandle
{
	public Variable Variable { get; private set; }

	public VariableMemoryHandle(Unit unit, Variable variable) : base(unit, new Result(new RegisterHandle(unit.GetStackPointer())), variable.Alignment ?? 0)
	{
		Variable = variable;
	}

	public override string ToString() 
	{
		if (Variable.Alignment == null)
		{
			return $"[{Variable.Name}]";
		}

		Offset = (int)Variable.Alignment;

		return base.ToString();
	}

	public override Handle Freeze() 
	{
		return (Handle)this.MemberwiseClone();
	}

	public override bool Equals(object? obj)
	{
		return obj is VariableMemoryHandle handle &&
			   base.Equals(obj) &&
			   Type == handle.Type &&
			   EqualityComparer<Size>.Default.Equals(Size, handle.Size) &&
			   EqualityComparer<Unit>.Default.Equals(Unit, handle.Unit) &&
			   EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
			   Offset == handle.Offset &&
			   EqualityComparer<Variable>.Default.Equals(Variable, handle.Variable);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Type, Size, Unit, Start, Offset, Variable);
	}
}

public class MemoryHandle : Handle
{
	public Unit Unit { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; set; }
	
	private bool IsStackMemoryPointer => Start.Value is RegisterHandle handle && handle.Register == Unit.GetStackPointer();
	private int Position => (IsStackMemoryPointer ? Unit.StackOffset : 0) + Offset;

	public static MemoryHandle FromStack(Unit unit, int offset)
    {
        return new MemoryHandle(unit, new Result(new RegisterHandle(unit.GetStackPointer())), offset);
    }

	public MemoryHandle(Unit unit, Result start, int offset) : base(HandleType.MEMORY)
	{
		Unit = unit;
		Start = start;
		Offset = offset;
	}

	public override void Use(int position)
	{
		Start.Use(position);
	}

	public override string ToString()
	{
		var offset = string.Empty;

		if (Position > 0)
		{
			offset = $"+{Position}";
		}
		else if (Position < 0)
		{
			offset = Position.ToString();
		}

		if (Start.Value.Type == HandleType.REGISTER ||
			Start.Value.Type == HandleType.CONSTANT)
		{
			var address = $"[{Start.Value}{offset}]";

			if (IsSizeVisible)
			{
				return $"{Size} {address}";
			}
			else
			{
				return $"{address}";
			}
		}

		throw new ApplicationException("Start of the memory handle was no longer in register");
	}

	public override Handle Freeze() 
	{
		if (Start.Value.Type == HandleType.REGISTER ||
			Start.Value.Type == HandleType.CONSTANT)
		{            
			return new MemoryHandle(Unit, new Result(Start.Value), Offset)
			{
				Format = Format
			};
		}

		throw new ApplicationException("Start of the memory handle was in invalid format for freeze operation");
	}

	public override bool Equals(object? obj)
	{
		return obj is MemoryHandle handle &&
			   EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
			   Offset == handle.Offset;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Offset);
	}
}

public class ComplexMemoryHandle : Handle
{
	public Result Start { get; private set; }
	public Result Offset { get; private set; }
	public int Stride { get; private set; }

	public ComplexMemoryHandle(Result start, Result offset, int stride) : base(HandleType.MEMORY)
	{
		Start = start;
		Offset = offset;
		Stride = stride;
		Format = Size.FromBytes(stride).ToFormat();
	}

	public override void Use(int position)
	{
		Start.Use(position);
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

		if (Start.Value.Type == HandleType.REGISTER ||
			Start.Value.Type == HandleType.CONSTANT)
		{
			var address = $"[{Start.Value}{offset}]";

			if (IsSizeVisible)
			{
				return $"{Size} {address}";
			}
			else
			{
				return $"{address}";
			}
		}

		throw new ApplicationException("Base of the memory handle was no longer in register");
	}

	public override Handle Freeze() 
	{
		if ((Start.Value.Type == HandleType.REGISTER || Start.Value.Type == HandleType.CONSTANT) && 
			(Offset.Value.Type == HandleType.REGISTER || Offset.Value.Type == HandleType.CONSTANT))
		{
			return new ComplexMemoryHandle(new Result(Start.Value), new Result(Offset.Value), Stride);
		}

		throw new ApplicationException("Parameters of a complex memory handle were in invalid format for freeze operation");
	}

	public override bool Equals(object? obj)
	{
		return obj is ComplexMemoryHandle handle &&
			   EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
			   EqualityComparer<Result>.Default.Equals(Offset, handle.Offset) &&
			   Stride == handle.Stride;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Offset, Stride);
	}
}

public class RegisterHandle : Handle
{
	public Register Register { get; private set; }

	public RegisterHandle(Register register) : base(register.IsMediaRegister ? HandleType.MEDIA_REGISTER : HandleType.REGISTER)
	{
		Register = register;
		Format = Register.IsMediaRegister ? Format.DECIMAL : Assembler.Size.ToFormat();
	}

	public override string ToString()
	{
		if (Size == Size.NONE)
		{
			return Register[Assembler.Size];
		}

		return Register[Size];
	}

	public override Handle Freeze() 
	{
		return (Handle)this.MemberwiseClone();
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