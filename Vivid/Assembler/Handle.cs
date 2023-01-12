using System.Collections.Generic;
using System.Linq;
using System;

public enum HandleType
{
	MEMORY = 1,
	CONSTANT = 2,
	REGISTER = 4,
	MEDIA_REGISTER = 8,
	EXPRESSION = 16,
	MODIFIER = 32,
	NONE = 64
}

public enum HandleInstanceType
{
	NONE,
	CONSTANT_DATA_SECTION,
	DATA_SECTION,
	CONSTANT,
	STACK_VARIABLE,
	MEMORY,
	STACK_MEMORY,
	TEMPORARY_MEMORY,
	COMPLEX_MEMORY,
	EXPRESSION,
	STACK_ALLOCATION,
	REGISTER,
	MODIFIER,
	LOWER_12_BITS,
	DISPOSABLE_PACK,
}

public class Handle
{
	public HandleType Type { get; protected set; }
	public HandleInstanceType Instance { get; protected set; }
	public Format Format { get; set; } = Settings.Format;
	public Size Size => Size.FromFormat(Format);
	public bool IsUnsigned => Format.IsUnsigned();

	public Handle()
	{
		Type = HandleType.NONE;
		Instance = HandleInstanceType.NONE;
	}

	public Handle(HandleType type, HandleInstanceType instance)
	{
		Type = type;
		Instance = instance;
	}

	public bool Is(HandleType type)
	{
		return Type == type;
	}

	public bool Is(HandleInstanceType instance)
	{
		return Instance == instance;
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

	public virtual void Use(Instruction instruction) { }

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
		Instance = HandleInstanceType.CONSTANT_DATA_SECTION;
	}

	public ConstantDataSectionHandle(byte[] bytes) : base("{ " + string.Join(", ", bytes.Select(i => i.ToString())) + " }")
	{
		Value = bytes;
		Instance = HandleInstanceType.CONSTANT_DATA_SECTION;
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

public enum DataSectionModifier
{
	NONE = 0,
	GLOBAL_OFFSET_TABLE = 1,
	PROCEDURE_LINKAGE_TABLE = 2
}

public class DataSectionHandle : Handle
{
	public const string X64_GLOBAL_OFFSET_TABLE = "@GOTPCREL";
	public const string X64_PROCEDURE_LINKAGE_TABLE = "@PLT";

	public const string ARM64_GLOBAL_OFFSET_TABLE = ":got:";

	public string Identifier { get; set; }
	public long Offset { get; set; } = 0;

	// Address means whether to use the value of the address or not
	public bool Address { get; set; } = false;
	public DataSectionModifier Modifier { get; set; } = DataSectionModifier.NONE;

	public DataSectionHandle(string identifier, bool address = false) : base(HandleType.MEMORY, HandleInstanceType.DATA_SECTION)
	{
		Identifier = identifier;
		Address = address;
	}

	public DataSectionHandle(string identifier, long offset, bool address = false) : base(HandleType.MEMORY, HandleInstanceType.DATA_SECTION)
	{
		Identifier = identifier;
		Offset = offset;
		Address = address;
	}

	public override string ToString()
	{
		// If the value of the address is only required, return it
		if (Address)
		{
			if (Settings.IsX64)
			{
				if (Modifier == DataSectionModifier.GLOBAL_OFFSET_TABLE) return string.Empty;
				if (Modifier == DataSectionModifier.PROCEDURE_LINKAGE_TABLE) return Identifier + X64_PROCEDURE_LINKAGE_TABLE;
			}
			else if (Settings.IsArm64)
			{
				if (Modifier == DataSectionModifier.GLOBAL_OFFSET_TABLE) return ARM64_GLOBAL_OFFSET_TABLE + Identifier;
			}

			return Identifier;
		}

		// When building for arm64, the code below should not execute
		if (Settings.IsArm64) return string.Empty;

		// If a modifier is attached, the offset is taken into account elsewhere
		if (Modifier != DataSectionModifier.NONE)
		{
			if (Modifier == DataSectionModifier.GLOBAL_OFFSET_TABLE) return $"{Size}{Assembler.MemoryAddressExtension}[{Assembler.RelativeSymbolSpecifier}{Identifier + X64_GLOBAL_OFFSET_TABLE}]";
			if (Modifier == DataSectionModifier.PROCEDURE_LINKAGE_TABLE) return $"{Size}{Assembler.MemoryAddressExtension}[{Assembler.RelativeSymbolSpecifier}{Identifier + X64_PROCEDURE_LINKAGE_TABLE}]";
			return string.Empty;
		}

		// Apply the offset if it is not zero
		if (Offset != 0)
		{
			var postfix = Offset.ToString();
			if (Offset > 0) { postfix = '+' + postfix; }

			return $"{Size}{Assembler.MemoryAddressExtension}[{Assembler.RelativeSymbolSpecifier}{Identifier}{postfix}]";
		}

		return $"{Size}{Assembler.MemoryAddressExtension}[{Assembler.RelativeSymbolSpecifier}{Identifier}]";
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
	public int Bits => Common.GetBits(Value);

	public ConstantHandle(object value) : base(HandleType.CONSTANT, HandleInstanceType.CONSTANT)
	{
		Value = value;
	}

	public ConstantHandle(object value, Format format) : base(HandleType.CONSTANT, HandleInstanceType.CONSTANT)
	{
		Value = value;
		Format = format;
	}

	public void Convert(Format format)
	{
		if (format == Format.DECIMAL)
		{
			Value = System.Convert.ToDouble(Value);
		}
		else
		{
			if (Format == Format.DECIMAL && double.IsInfinity((double)Value))
			{
				Value = double.IsPositiveInfinity((double)Value) ? long.MaxValue : long.MinValue;
			}
			else
			{
				Value = System.Convert.ToInt64(Value);
			}
		}

		Format = format;
	}

	public string ToStringShared()
	{
		return Format.IsDecimal() ? ((double)Value).ToString(false) : ((long)Value).ToString(false);
	}

	public override string ToString()
	{
		if (Settings.IsArm64)
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

		Instance = HandleInstanceType.STACK_VARIABLE;
	}

	public override int GetAbsoluteOffset()
	{
		// Update the offset, if the variable has alignment now
		if (Variable.LocalAlignment != null) { Offset = (int)Variable.LocalAlignment; }

		return (IsAbsolute ? Unit.StackOffset : 0) + Offset;
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

	public MemoryHandle(Unit unit, Result start, int offset) : base(HandleType.MEMORY, HandleInstanceType.MEMORY)
	{
		Unit = unit;
		Start = start;
		Offset = offset;
	}

	public virtual int GetAbsoluteOffset()
	{
		return Offset;
	}

	public override void Use(Instruction instruction)
	{
		Start.Use(instruction);
	}

	public Register? GetStart()
	{
		return Start.Value.Instance switch
		{
			HandleInstanceType.REGISTER => Start.Value.To<RegisterHandle>().Register,
			HandleInstanceType.STACK_ALLOCATION => Unit.GetStackPointer(),
			_ => null
		};
	}

	public int GetOffset()
	{
		return Start.Value.Instance switch
		{
			HandleInstanceType.CONSTANT => (int)(long)Start.Value.To<ConstantHandle>().Value + AbsoluteOffset,
			HandleInstanceType.STACK_ALLOCATION => Start.Value.To<StackAllocationHandle>().AbsoluteOffset + AbsoluteOffset,
			_ => AbsoluteOffset
		};
	}

	public override string ToString()
	{
		var start = GetStart();
		var offset = GetOffset();

		if (start == null)
		{
			if (Settings.IsArm64) return $"[xzr, #{offset.ToString(false)}]";
			else return $"{Size}{Assembler.MemoryAddressExtension}[{offset.ToString(false)}]";
		}
		else
		{
			if (Settings.IsArm64) return $"[{start}, #{offset.ToString(false)}]";
			else {
				var offset_text = string.Empty;

				if (offset > 0) { offset_text = $"+{offset}"; }
				else if (offset < 0) { offset_text = $"-{-offset}"; }

				return $"{Size}{Assembler.MemoryAddressExtension}[{start}{offset_text}]";
			}
		}
	}

	public override Result[] GetRegisterDependentResults()
	{
		if (Start.IsStackAllocation)
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
		if (Start.IsStandardRegister || Start.IsConstant || Start.IsStackAllocation)
		{
			return new MemoryHandle(Unit, new Result(Start.Value, Start.Format), Offset);
		}

		throw new ApplicationException("Start of the memory handle was in invalid format during finalization");
	}

	public override bool Equals(object? other)
	{
		return other is MemoryHandle handle && Equals(Start.Value, handle.Start.Value) && Offset == handle.Offset;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Offset);
	}
}

public class StackMemoryHandle : MemoryHandle
{
	public bool IsAbsolute { get; private set; }

	public StackMemoryHandle(Unit unit, int offset, bool absolute = true) : base(unit, new Result(new RegisterHandle(unit.GetStackPointer()), Settings.Format), offset)
	{
		IsAbsolute = absolute;
		Instance = HandleInstanceType.STACK_MEMORY;
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

		throw new ApplicationException("Stack memory handle did not use the stack pointer register");
	}

	public override bool Equals(object? other)
	{
		return other is StackMemoryHandle handle &&
				 Offset == handle.Offset &&
				 IsAbsolute == handle.IsAbsolute;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(IsAbsolute);
		return hash.ToHashCode();
	}
}

public class TemporaryMemoryHandle : StackMemoryHandle
{
	public string Identity { get; private set; }
	public int Bytes => Size.Bytes;

	public TemporaryMemoryHandle(Unit unit) : base(unit, 0)
	{
		Identity = unit.GetNextIdentity();
		Instance = HandleInstanceType.TEMPORARY_MEMORY;
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is TemporaryMemoryHandle handle &&
			  base.Equals(other) &&
			  Equals(Identity, handle.Identity);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Identity);
	}
}

public class ComplexMemoryHandle : Handle
{
	public Result Start { get; private set; }
	public Result Index { get; private set; }
	public int Stride { get; private set; }
	public int Offset { get; private set; }

	public ComplexMemoryHandle(Result start, Result index, int stride, int offset = 0) : base(HandleType.MEMORY, HandleInstanceType.COMPLEX_MEMORY)
	{
		Start = start;
		Index = index;
		Stride = stride;
		Offset = offset;

		if (Settings.IsArm64 && offset != 0)
		{
			throw new InvalidOperationException("Architecture arm64 does not support memory handles with multiple offsets");
		}
	}

	public override void Use(Instruction instruction)
	{
		Start.Use(instruction);
		Index.Use(instruction);
	}

	public Register? GetStart()
	{
		return Start.Value.Instance switch
		{
			HandleInstanceType.REGISTER => Start.Value.To<RegisterHandle>().Register,
			_ => null
		};
	}

	public Register? GetIndex()
	{
		return Index.Value.Instance switch
		{
			HandleInstanceType.REGISTER => Index.Value.To<RegisterHandle>().Register,
			_ => null
		};
	}

	public int GetOffset()
	{
		var offset = Offset;

		offset += Start.Value.Instance switch
		{
			HandleInstanceType.CONSTANT => (int)(long)Start.Value.To<ConstantHandle>().Value,
			_ => 0
		};

		offset += Index.Value.Instance switch
		{
			HandleInstanceType.CONSTANT => (int)(long)Index.Value.To<ConstantHandle>().Value * Stride,
			_ => 0
		};

		return offset;
	}

	public string ToStringX64()
	{
		// Examples: [1], [rax], [rax-1], [rax+rbx], [rax+rbx+1], [rax+rbx*2], [rax+rbx*2-1]
		var start = GetStart();
		var index = GetIndex();
		var offset = GetOffset();

		var result = "[";

		if (start != null) { result += start.ToString(); }

		if (index != null)
		{
			// Add a plus-operator to separate the base from the index if needed
			if (result.Length > 1) { result += '+'; }
			result += index.ToString();

			// Multiply the index register, if the stride is not one
			if (Stride != 1) { result += '*' + Stride.ToString(false); }
		}

		// Finally, add the offset. Add the sign always, if something has been added to the result.
		if (result.Length > 1)
		{
			if (offset != 0) { result += offset.ToString(true); }
		}
		else
		{
			result += offset.ToString(false);
		}

		return $"{Size}{Assembler.MemoryAddressExtension}{result}]";
	}

	public string ToStringArm64()
	{
		// Examples: [xzr, #1], [x0], [x0, #-1], [x0, x1], [x0, x1, lsl #1], [x0, :lo12:symbol]
		var start = GetStart();
		var index = GetIndex();
		var offset = GetOffset();

		var result = "[";

		if (start != null) { result += start.ToString(); }

		if (index != null) {
			// Separate the base from the index if needed
			if (result.Length > 1) { result += ", "; }
			result += index.ToString();

			// Multiply the index register, if the stride is not one
			if (Stride != 1) { result += $", {Instructions.Arm64.SHIFT_LEFT} #{(long)Math.Log2(Stride)}"; }

			// The offset must be zero here
			if (offset != 0) return string.Empty;
		}
		else if (Index.IsModifier) {
			// Separate the base from the index if needed
			if (result.Length > 1) { result += ", "; }

			result += Index.ToString();

			// Stride must be one and offset must be zero here
			if (Stride != 1 || offset != 0) return string.Empty;
		}

		// Add the offset
		if (result.Length == 1) {
			result += $"xzr, #{offset.ToString(false)}";
		}
		else if (offset != 0) {
			result += $", #{offset.ToString(false)}";
		}

		return result + ']';
	}

	public override string ToString()
	{
		if (Settings.IsX64) return ToStringX64();
		else return ToStringArm64();
	}

	public override Result[] GetRegisterDependentResults()
	{
		var result = new List<Result>();
		if (!Start.IsConstant) { result.Add(Start); }
		if (!Index.IsConstant && !Index.IsModifier) { result.Add(Index); }
		return result.ToArray();
	}

	public override Result[] GetInnerResults()
	{
		return new[] { Start, Index };
	}

	public override Handle Finalize()
	{
		return new ComplexMemoryHandle
		(
			new Result(Start.Value.Finalize(), Start.Format),
			new Result(Index.Value.Finalize(), Index.Format),
			Stride,
			Offset
		);
	}

	public override bool Equals(object? other)
	{
		return other is ComplexMemoryHandle handle &&
			  Equals(Start.Value, handle.Start.Value) &&
			  Equals(Index.Value, handle.Index.Value) &&
			  Stride == handle.Stride &&
			  Offset == handle.Offset;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Start, Index, Stride, Offset);
	}
}

public class ExpressionHandle : Handle
{
	public const string ARM64_ZERO_REGISTER = "xzr";

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
		return new ExpressionHandle(new Result(left, Settings.Format), 1, new Result(right, Settings.Format), 0);
	}

	public static ExpressionHandle CreateMemoryAddress(Result start, int offset)
	{
		if (Settings.IsArm64)
		{
			return new ExpressionHandle(start, 1, new Result(new ConstantHandle((long)offset), Settings.Format), 0);
		}

		return new ExpressionHandle(start, 1, null, offset);
	}

	public static ExpressionHandle CreateMemoryAddress(Result start, Result offset, int stride)
	{
		return new ExpressionHandle(offset, stride, start, 0);
	}

	public ExpressionHandle(Result multiplicand, int multiplier, Result? addition, int constant) : base(HandleType.EXPRESSION, HandleInstanceType.EXPRESSION)
	{
		Multiplicand = multiplicand;
		Multiplier = multiplier;
		Addition = addition;
		Constant = constant;
	}

	public override void Use(Instruction instruction)
	{
		Multiplicand.Use(instruction);
		Addition?.Use(instruction);
	}

	private void Validate()
	{
		if ((!Multiplicand.IsStandardRegister && !Multiplicand.IsConstant) || (Addition != null && (!Addition.IsStandardRegister && !Addition.IsConstant)) || Multiplier <= 0)
		{
			throw new ApplicationException("Detected an invalid expression handle");
		}
	}

	public Register? GetStart()
	{
		if (Addition == null) return null;

		return Addition.Value.Instance switch
		{
			HandleInstanceType.REGISTER => Addition.Value.To<RegisterHandle>().Register,
			_ => null
		};
	}

	public Register? GetIndex()
	{
		return Multiplicand.Value.Instance switch
		{
			HandleInstanceType.REGISTER => Multiplicand.Value.To<RegisterHandle>().Register,
			_ => null
		};
	}

	public int GetOffset()
	{
		var offset = Constant;

		if (Addition != null)
		{
			offset += Addition.Value.Instance switch
			{
				HandleInstanceType.CONSTANT => (int)(long)Addition.Value.To<ConstantHandle>().Value,
				_ => 0
			};
		}

		offset += Multiplicand.Value.Instance switch
		{
			HandleInstanceType.CONSTANT => (int)(long)Multiplicand.Value.To<ConstantHandle>().Value * Multiplier,
			_ => 0
		};

		return offset;
	}

	#warning Upgrade
	public string ToStringX64()
	{
		var result = string.Empty;
		var constant = (long)Constant;

		if (Multiplicand.IsConstant)
		{
			constant += (long)Multiplicand.Value.To<ConstantHandle>().Value * Multiplier;
		}
		else
		{
			result = Multiplicand.ToString();

			if (Multiplier > 1)
			{
				result += "*" + Multiplier.ToString();
			}
		}

		if (Addition != null)
		{
			if (Addition.IsConstant)
			{
				constant += (long)Addition.Value.To<ConstantHandle>().Value;
			}
			else if (!string.IsNullOrEmpty(result))
			{
				result += "+" + Addition.ToString();
			}
			else
			{
				result += Addition.ToString();
			}
		}

		var empty = string.IsNullOrEmpty(result);

		if (constant > 0 || empty)
		{
			result += empty ? constant : $"+{constant}";
		}
		else if (constant < 0)
		{
			result += $"-{-constant}";
		}

		return '[' + result + ']';
	}

	public string ToStringArm64()
	{
		if (Constant != 0 && !Multiplicand.IsConstant)
		{
			throw new ApplicationException("Complex expression handles are not supported on architecture arm64");
		}

		// Examples:
		// x0, x1
		// x0, #1
		// x0, x1, lsl #2
		var result = Addition != null ? Addition.ToString() : ARM64_ZERO_REGISTER;
		var constant = (long)Constant;

		if (Multiplicand.IsConstant)
		{
			constant += (long)Multiplicand.Value.To<ConstantHandle>().Value * Multiplier;
		}
		else
		{
			result += ", " + Multiplicand.ToString();

			if (Multiplier > 1)
			{
				result += $", {Instructions.Arm64.SHIFT_LEFT} #" + (long)Math.Log2(Multiplier);
			}

			return result;
		}

		if (constant != 0)
		{
			if (constant >= 0) { result += $", #{constant}"; }
			else { result += $", #-{-constant}"; }
		}
		else
		{
			result += $", {ARM64_ZERO_REGISTER}";
		}

		return result;
	}

	public override string ToString()
	{
		Validate();

		if (Settings.IsArm64)
		{
			return ToStringArm64();
		}

		return ToStringX64();
	}

	public override Result[] GetRegisterDependentResults()
	{
		var result = new List<Result>();

		if (!Multiplicand.IsConstant)
		{
			result.Add(Multiplicand);
		}

		if (Addition != null && (Settings.IsArm64 || !Addition.IsConstant))
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
			new Result(Multiplicand.Value, Multiplicand.Format),
			Multiplier,
			Addition == null ? null : new Result(Addition.Value, Addition.Format),
			Constant
		);
	}

	public override bool Equals(object? other)
	{
		return other is ExpressionHandle handle &&
			Equals(Multiplicand.Value, handle.Multiplicand.Value) &&
			Multiplier == handle.Multiplier &&
			Equals(Addition?.Value, handle.Addition?.Value) &&
			Constant == handle.Constant;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(Type);
		hash.Add(Multiplicand);
		hash.Add(Multiplier);
		hash.Add(Addition);
		hash.Add(Constant);
		return hash.ToHashCode();
	}
}

public class StackAllocationHandle : Handle
{
	public Unit Unit { get; private set; }
	public int Offset { get; set; }
	public int Bytes { get; private set; }
	public string Identity { get; private set; }

	public int AbsoluteOffset => Unit.StackOffset + Offset;

	public StackAllocationHandle(Unit unit, int bytes, string identity)
	{
		Unit = unit;
		Bytes = bytes;
		Identity = identity;
		Type = HandleType.EXPRESSION;
		Instance = HandleInstanceType.STACK_ALLOCATION;
	}

	public override Handle Finalize()
	{
		return (Handle)MemberwiseClone();
	}

	public override string ToString()
	{
		var stack_pointer = Unit.GetStackPointer();
		var offset = AbsoluteOffset;

		if (Settings.IsArm64)
		{
			if (offset >= 0) return stack_pointer.ToString() + ", #" + offset;
			return stack_pointer.ToString() + ", #-" + (-offset);
		}

		if (offset != 0) return '[' + stack_pointer.ToString() + offset.ToString(true) + ']';

		return '[' + stack_pointer.ToString() + ']';
	}

	public override bool Equals(object? other)
	{
		return other is StackAllocationHandle handle &&
				 Type == handle.Type &&
				 Format == handle.Format &&
				 Offset == handle.Offset &&
				 Bytes == handle.Bytes &&
				 Equals(Identity, handle.Identity);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type, Format, Offset, Bytes);
	}
}

public class RegisterHandle : Handle
{
	public Register Register { get; private set; }

	public RegisterHandle(Register register) : base(register.IsMediaRegister ? HandleType.MEDIA_REGISTER : HandleType.REGISTER, HandleInstanceType.REGISTER)
	{
		Register = register;
	}

	public override string ToString()
	{
		if (Size == Size.NONE)
		{
			return Register[Settings.Size];
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

	public ModifierHandle(string modifier) : base(HandleType.MODIFIER, HandleInstanceType.MODIFIER)
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
	private const string GLOBAL_OFFSET_TABLE_LOWER_12_BITS = ":got_lo12:";

	private Result Handle { get; set; }
	private bool GlobalOffsetTable { get; set; }

	public Lower12Bits(DataSectionHandle handle, bool global_offset_table) : base(HandleType.MODIFIER, HandleInstanceType.LOWER_12_BITS)
	{
		var copy = (DataSectionHandle)handle.Finalize();
		copy.Modifier = DataSectionModifier.NONE;
		copy.Address = true;

		Handle = new Result(copy, Settings.Format);
		GlobalOffsetTable = global_offset_table;
	}

	public override Result[] GetInnerResults()
	{
		return new[] { Handle };
	}

	public override string ToString()
	{
		return (GlobalOffsetTable ? GLOBAL_OFFSET_TABLE_LOWER_12_BITS : LOWER_12_BITS) + Handle.Value;
	}

	public override bool Equals(object? other)
	{
		return other is Lower12Bits bits && base.Equals(other) && EqualityComparer<Result>.Default.Equals(Handle, bits.Handle);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Handle);
	}
}

public class DisposablePackHandle : Handle
{
	public Dictionary<Variable, Result> Members { get; } = new Dictionary<Variable, Result>();

	public DisposablePackHandle(Unit unit, Type type) : base(HandleType.EXPRESSION, HandleInstanceType.DISPOSABLE_PACK)
	{
		// Initialize the members
		foreach (var member in type.Variables.Values)
		{
			var value = new Result();

			if (member.Type!.IsPack)
			{
				value.Value = new DisposablePackHandle(unit, member.Type!);
			}

			Members[member] = value;
		}
	}

	public override void Use(Instruction instruction)
	{
		foreach (var member in Members.Values)
		{
			member.Use(instruction);
		}
	}

	public override Result[] GetInnerResults()
	{
		return Members.Values.ToArray();
	}
}