using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public enum HandleType
{
	MEMORY,
	CONSTANT,
	REGISTER,
	MEDIA_REGISTER,
	EXPRESSION,
	MODIFIER,
	NONE
}

public class Handle
{
	public HandleType Type { get; protected set; }
	public bool IsPrecise { get; set; } = false;

	public Format Format { get; set; } = Assembler.Format;
	public Size Size => Size.FromFormat(Format);
	public bool IsUnsigned => Format.IsUnsigned();

	public Handle()
	{
		Type = HandleType.NONE;
	}

	public Handle(HandleType type)
	{
		Type = type;
	}

	public bool Is(HandleType type)
	{
		return Type == type;
	}

	/// <summary>
	/// Returns all results which the handle requires to be in registers
	/// </summary>
	public virtual Result[] GetRegisterDependentResults()
	{
		return Array.Empty<Result>();
	}

	/// <summary>
	/// Returns all results used in the handle
	/// </summary>
	public virtual Result[] GetInnerResults()
	{
		return Array.Empty<Result>();
	}

	public T To<T>() where T : Handle
	{
		return (T)this;
	}

	public virtual void Use(int position) { }

	public virtual Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override string ToString()
	{
		return string.Empty;
	}
}

public class ConstantDataSectionHandle : DataSectionHandle
{
	public object Value { get; private set; }

	public ConstantDataSectionHandle(ConstantHandle handle) : base(handle.ToString())
	{
		Value = handle.Value;
	}

	public ConstantDataSectionHandle(byte[] bytes) : base("{ " + string.Join(", ", bytes.Select(i => i.ToString(CultureInfo.InvariantCulture))) + " }")
	{
		Value = bytes;
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is ConstantDataSectionHandle handle &&
			   base.Equals(other) &&
			   EqualityComparer<object>.Default.Equals(Value, handle.Value);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value);
	}
}

public class DataSectionHandle : Handle
{
	public string Identifier { get; set; }
	public long Offset { get; set; } = 0;

	// Address means whether to use the value of the address or not
	public bool Address { get; set; } = false;

	public DataSectionHandle(string identifier, bool address = false) : base(HandleType.MEMORY)
	{
		Identifier = identifier;
		Address = address;
	}

	public DataSectionHandle(string identifier, long offset, bool address = false) : base(HandleType.MEMORY)
	{
		Identifier = identifier;
		Offset = offset;
		Address = address;
	}

	public override string ToString()
	{
		if (Address)
		{
			return Identifier;
		}

		if (Offset != 0)
		{
			var offset = Offset.ToString(CultureInfo.InvariantCulture);

			if (Offset > 0)
			{
				offset = '+' + offset;
			}

			if (Assembler.Is64bit)
			{
				return IsPrecise ? $"{Size} ptr [rip+{Identifier}{offset}]" : $"[rip+{Identifier}{offset}]";
			}

			return IsPrecise ? $"{Size} ptr [{Identifier}{offset}]" : $"[{Identifier}{offset}]";
		}

		if (Assembler.Is64bit)
		{
			return IsPrecise ? $"{Size} ptr [rip+{Identifier}]" : $"[rip+{Identifier}]";
		}

		return IsPrecise ? $"{Size} ptr [{Identifier}]" : $"[{Identifier}]";
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is DataSectionHandle handle &&
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
	public object Value { get; set; }
	public int Bits => GetBits();

	private int GetBits()
	{
		if (Value is double)
		{
			return Assembler.Size.Bits;
		}

		var x = (long)Value;

		if (x < 0)
		{
			if (x < int.MinValue)
			{
				return 64;
			}
			else if (x < short.MinValue)
			{
				return 32;
			}
			else if (x < byte.MinValue)
			{
				return 16;
			}
		}
		else
		{
			if (x > int.MaxValue)
			{
				return 64;
			}
			else if (x > short.MaxValue)
			{
				return 32;
			}
			else if (x > byte.MaxValue)
			{
				return 16;
			}
		}

		return 8;
	}

	public ConstantHandle(object value) : base(HandleType.CONSTANT)
	{
		Value = value;
	}

	public void Convert(Format format)
	{
		if (format == Format.DECIMAL)
		{
			Value = System.Convert.ToDouble(Value, CultureInfo.InvariantCulture);
		}
		else
		{
			Value = System.Convert.ToInt64(Value, CultureInfo.InvariantCulture);
		}
	}

	public string ToStringShared()
	{
		var result = Value?.ToString()?.Replace(',', '.');

		if (result == null)
		{
			return string.Empty;
		}

		if (Format.IsDecimal() && !result.Contains('.'))
		{
			return result + ".0";
		}

		return result;
	}

	public override string ToString()
	{
		if (Assembler.IsArm64)
		{
			return '#' + ToStringShared();
		}

		return ToStringShared();
	}

	public override bool Equals(object? other)
	{
		return other is ConstantHandle handle &&
			  EqualityComparer<object>.Default.Equals(Value, handle.Value);
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Value);
	}
}

public class StackVariableHandle : StackMemoryHandle
{
	public Variable Variable { get; private set; }

	public StackVariableHandle(Unit unit, Variable variable) : base(unit, variable.LocalAlignment ?? 0)
	{
		Variable = variable;

		if (!Variable.IsPredictable)
		{
			throw new ArgumentException("Tried to create stack variable handle for a variable which is not stored in the stack");
		}
	}

	public override string ToString()
	{
		if (Variable.LocalAlignment == null)
		{
			return $"[{Variable.Name}]";
		}

		Offset = (int)Variable.LocalAlignment;

		return base.ToString();
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is StackVariableHandle handle &&
			  base.Equals(other) &&
			  EqualityComparer<Variable>.Default.Equals(Variable, handle.Variable);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Variable);
	}
}

public class MemoryHandle : Handle
{
	public Unit Unit { get; private set; }
	public Result Start { get; private set; }
	public int Offset { get; set; }

	private int AbsoluteOffset => GetAbsoluteOffset();

	public MemoryHandle(Unit unit, Result start, int offset) : base(HandleType.MEMORY)
	{
		Unit = unit;
		Start = start;
		Offset = offset;
	}

	public virtual int GetAbsoluteOffset()
	{
		return Offset;
	}

	public override void Use(int position)
	{
		Start.Use(position);
	}

	public override string ToString()
	{
		var start = Start.Value;
		var offset = AbsoluteOffset;

		if (Start.Value is InlineHandle inline)
		{
			start = new RegisterHandle(Unit.GetStackPointer());
			offset += inline.AbsoluteOffset;
		}

		var constant = string.Empty;

		if (Assembler.IsArm64)
		{
			if (AbsoluteOffset != 0)
			{
				constant = $", #{offset}";
			}
		}
		else
		{
			if (AbsoluteOffset > 0)
			{
				constant = $"+{offset}";
			}
			else if (AbsoluteOffset < 0)
			{
				constant = offset.ToString(CultureInfo.InvariantCulture);
			}
		}

		if (start.Is(HandleType.REGISTER) || start.Is(HandleType.CONSTANT))
		{
			var address = $"[{start}{constant}]";

			if (IsPrecise && Assembler.IsX64)
			{
				return $"{Size} ptr {address}";
			}
			else
			{
				return $"{address}";
			}
		}

		return string.Empty;
	}

	public override Result[] GetRegisterDependentResults()
	{
		if (Start.Value is InlineHandle)
		{
			return Array.Empty<Result>();
		}

		return new[] { Start };
	}

	public override Result[] GetInnerResults()
	{
		return new[] { Start };
	}

	public override Handle Finalize()
	{
		if (Start.IsStandardRegister || Start.IsConstant || Start.Value is InlineHandle)
		{
			return new MemoryHandle(Unit, new Result(Start.Value, Start.Format), Offset);
		}

		throw new ApplicationException("Start of the memory handle was in invalid format for freeze operation");
	}

	public override bool Equals(object? other)
	{
		return other is MemoryHandle handle &&
			  EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
			  Offset == handle.Offset;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Offset);
	}
}

public class StackMemoryHandle : MemoryHandle
{
	public bool IsAbsolute { get; private set; }

	public StackMemoryHandle(Unit unit, int offset, bool absolute = true) : base(unit, new Result(new RegisterHandle(unit.GetStackPointer()), Assembler.Format), offset)
	{ 
		IsAbsolute = absolute;
	}

	public override int GetAbsoluteOffset()
	{
		return (IsAbsolute ? Unit.StackOffset : 0) + Offset;
	}

	public override Handle Finalize()
	{
		if (Start.Value.To<RegisterHandle>().Register == Unit.GetStackPointer())
		{
			return new StackMemoryHandle(Unit, Offset, IsAbsolute);
		}

		throw new ApplicationException("Stack memory handle's register was invalid");
	}

	public override bool Equals(object? other)
	{
		return other is StackMemoryHandle handle &&
				 Offset == handle.Offset &&
				  IsAbsolute == handle.IsAbsolute;
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(IsAbsolute);
		return hash.ToHashCode();
	}
}

public class TemporaryMemoryHandle : StackMemoryHandle
{
	public Guid Identifier { get; private set; }

	public TemporaryMemoryHandle(Unit unit) : base(unit, 0)
	{
		Identifier = Guid.NewGuid();
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is TemporaryMemoryHandle handle &&
			  base.Equals(other) &&
			  EqualityComparer<Guid>.Default.Equals(Identifier, handle.Identifier);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Identifier);
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
	}

	public override void Use(int position)
	{
		Start.Use(position);
		Offset.Use(position);
	}

	public override string ToString()
	{
		var offset = string.Empty;

		if (Offset.IsStandardRegister || Offset.IsModifier)
		{
			if (Assembler.IsArm64)
			{
				offset = $", {Offset.ToString()}" + (Stride == 1 ? string.Empty : $", {BitwiseInstruction.ARM64_SHIFT_LEFT_INSTRUCTION} #{(long)Math.Log2(Stride)}");
			}
			else
			{
				offset = "+" + Offset.ToString() + (Stride == 1 ? string.Empty : $"*{Stride}");
			}
		}
		else if (Offset.Value is ConstantHandle constant)
		{
			var index = (long)constant.Value;
			var value = index * Stride;

			if (Assembler.IsArm64)
			{
				if (value != 0)
				{
					offset = $", #{value}";
				}
			}
			else
			{
				if (value > 0)
				{
					offset = $"+{value}";
				}
				else if (value < 0)
				{
					offset = value.ToString(CultureInfo.InvariantCulture);
				}
			}
		}
		else
		{
			return string.Empty;
		}

		if (Start.Value.Type == HandleType.REGISTER || Start.Value.Type == HandleType.CONSTANT)
		{
			var address = $"[{Start.Value}{offset}]";

			if (IsPrecise && Assembler.IsX64)
			{
				return $"{Size} ptr {address}";
			}
			else
			{
				return $"{address}";
			}
		}

		return string.Empty;
	}

	public override Result[] GetRegisterDependentResults()
	{
		if (!Offset.IsConstant && !Offset.IsModifier)
		{
			return new Result[] { Start, Offset };
		}

		return new Result[] { Start };
	}

	public override Result[] GetInnerResults()
	{
		return new[] { Start, Offset };
	}

	public override Handle Finalize()
	{
		return new ComplexMemoryHandle
		(
			new Result(Start.Value.Finalize(), Start.Format),
			new Result(Offset.Value.Finalize(), Offset.Format),
			Stride
		);
	}

	public override bool Equals(object? other)
	{
		return other is ComplexMemoryHandle handle &&
			  EqualityComparer<Result>.Default.Equals(Start, handle.Start) &&
			  EqualityComparer<Result>.Default.Equals(Offset, handle.Offset) &&
			  Stride == handle.Stride;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Offset, Stride);
	}
}

public class ExpressionHandle : Handle
{
	public Result Multiplicand { get; private set; }
	public int Multiplier { get; private set; }
	public Result? Addition { get; private set; }
	public int Constant { get; private set; }

	public static ExpressionHandle CreateAddition(Result left, Result right)
	{
		return new ExpressionHandle(left, 1, right, 0);
	}

	public static ExpressionHandle CreateAddition(Handle left, Handle right)
	{
		return new ExpressionHandle(new Result(left, Assembler.Format), 1, new Result(right, Assembler.Format), 0);
	}

	public static ExpressionHandle CreateMemoryAddress(Result start, int offset)
	{
		return new ExpressionHandle(start, 1, null, offset);
	}

	public static ExpressionHandle CreateMemoryAddress(Result start, Result offset, int stride)
	{
		return new ExpressionHandle(offset, stride, start, 0);
	}

	public ExpressionHandle(Result multiplicand, int multiplier, Result? addition, int constant) : base(HandleType.EXPRESSION)
	{
		Multiplicand = multiplicand;
		Multiplier = multiplier;
		Addition = addition;
		Constant = constant;
	}

	public override void Use(int position)
	{
		Multiplicand.Use(position);
		Addition?.Use(position);
	}

	private void Validate()
	{
		if ((Multiplicand.Value.Type != HandleType.REGISTER && Multiplicand.Value.Type != HandleType.CONSTANT) ||
			(Addition != null && (Addition.Value.Type != HandleType.REGISTER && Addition.Value.Type != HandleType.CONSTANT)) ||
			  Multiplier <= 0)
		{
			throw new ApplicationException("Detected an invalid calculation handle");
		}
	}

	public string ToStringX64()
	{
		var result = Multiplicand.ToString();

		if (Multiplier > 1)
		{
			result += "*" + Multiplier.ToString(CultureInfo.InvariantCulture);
		}

		if (Addition != null)
		{
			if (Addition.IsConstant && Numbers.IsNegative(Addition.Value.To<ConstantHandle>().Value))
			{
				result += Addition.ToString();
			}
			else
			{
				result += "+" + Addition.ToString();
			}
		}

		if (Constant != 0)
		{
			result += (Constant > 0 ? "+" : "") + Constant;
		}

		return '[' + result + ']';
	}

	public string ToStringArm64()
	{
		if (Addition != null && Constant != 0)
		{
			throw new ApplicationException("Complex expression handles are not supported on architecture Arm64");
		}

		// Examples:
		// x0, x1
		// x0, #1
		// x0, x1, lsl #2
		
		var result = Multiplicand.ToString();
		var constant = (long)Constant;

		if (Addition != null)
		{
			if (Addition.IsConstant)
			{
				constant += (Addition.Value.To<ConstantHandle>().Value as long?) ?? throw new ApplicationException("Constant was not an integer");
			}
			else
			{
				result += ", " + Addition.ToString();
			}
		}

		if (Multiplier > 1)
		{
			result += $", {BitwiseInstruction.ARM64_SHIFT_LEFT_INSTRUCTION} #" + Multiplier;
		}

		if (Constant != 0)
		{
			result += ", #" + Constant;
		}

		return result;
	}

	public override string ToString()
	{
		Validate();

		if (Assembler.IsArm64)
		{
			return ToStringArm64();
		}
		
		return ToStringX64();
	}

	public override Result[] GetRegisterDependentResults()
	{
		var result = new List<Result>();

		if (Multiplicand.Value.Type != HandleType.CONSTANT)
		{
			result.Add(Multiplicand);
		}

		/// TODO: Bit limit
		if (Addition != null && Addition.Value.Type != HandleType.CONSTANT)
		{
			result.Add(Addition);
		}

		return result.ToArray();
	}

	public override Result[] GetInnerResults()
	{
		return new[] { Multiplicand, Addition }.Where(i => i != null).ToArray()!;
	}

	public override Handle Finalize()
	{
		Validate();

		return new ExpressionHandle
		(
			new Result(Multiplicand.Value, Assembler.Format),
			Multiplier,
			Addition == null ? null : new Result(Addition.Value, Assembler.Format),
			Constant
		);
	}

	public override bool Equals(object? other)
	{
		return other is ExpressionHandle handle &&
			EqualityComparer<Result>.Default.Equals(Multiplicand, handle.Multiplicand) &&
			Multiplier == handle.Multiplier &&
			EqualityComparer<Result?>.Default.Equals(Addition, handle.Addition) &&
			Constant == handle.Constant;
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(Type);
		hash.Add(Multiplicand);
		hash.Add(Multiplier);
		hash.Add(Addition);
		hash.Add(Constant);
		return hash.ToHashCode();
	}
}

public class InlineHandle : Handle
{
	public Guid Identifier { get; private set; }

	public Unit Unit { get; private set; }

	public int Offset { get; set; }
	public int Bytes { get; private set; }

	public int AbsoluteOffset => Unit.StackOffset + Offset;

	public InlineHandle(Unit unit, int bytes)
	{ 
		Unit = unit;
		Identifier = Guid.NewGuid();
		Bytes = bytes;
		Type = HandleType.EXPRESSION;
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}
	
	public override string ToString()
	{
		var stack_pointer = Unit.GetStackPointer();
		var offset = AbsoluteOffset;

		if (Assembler.IsArm64)
		{
			return stack_pointer.ToString() + ", #" + offset;
		}

		if (offset > 0)
		{
			return '[' + stack_pointer.ToString() + '+' + offset + ']';
		}

		if (offset < 0)
		{
			return '[' + stack_pointer.ToString() + offset + ']';
		}

		return '[' + stack_pointer.ToString() + ']';
	}

	public override bool Equals(object? other)
	{
		return other is InlineHandle handle &&
				 Type == handle.Type &&
				 Format == handle.Format &&
				 Offset == handle.Offset &&
				 Bytes == handle.Bytes &&
				 EqualityComparer<Guid>.Default.Equals(Identifier, handle.Identifier);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type, Format, Offset, Bytes);
	}
}

public class RegisterHandle : Handle
{
	public Register Register { get; private set; }

	public RegisterHandle(Register register) : base(register.IsMediaRegister ? HandleType.MEDIA_REGISTER : HandleType.REGISTER)
	{
		Register = register;
	}

	public override string ToString()
	{
		if (Size == Size.NONE)
		{
			return Register[Assembler.Size];
		}

		return Register[Size];
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is RegisterHandle handle && Register == handle.Register;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Register);
	}
}

public class ModifierHandle : Handle
{
	public string Modifier { get; private set; }

	public ModifierHandle(string modifier) : base(HandleType.MODIFIER)
	{
		Modifier = modifier;
	}

	public override string ToString()
	{
		return Modifier;
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is ModifierHandle handle && Modifier == handle.Modifier;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Modifier);
	}
}

public class Lower12Bits : Handle
{
	private const string LOWER_12_BITS = ":lo12:";

	private Result Handle { get; set; }

	public Lower12Bits(DataSectionHandle handle) : base(HandleType.MODIFIER)
	{
		var copy = (DataSectionHandle)handle.Finalize();
		copy.Address = true;

		Handle = new Result(copy, Assembler.Format);
	}

	public override Result[] GetInnerResults()
	{
		return new[] { Handle };
	}

	public override string ToString()
	{
		return LOWER_12_BITS + Handle.Value;
	}
}