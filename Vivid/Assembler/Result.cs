using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public enum DirectiveType
{
	NON_VOLATILITY,
	SPECIFIC_REGISTER,
	AVOID_REGISTERS
}

public class Directive
{
	public DirectiveType Type { get; }

	public Directive(DirectiveType type)
	{
		Type = type;
	}

	public T To<T>() where T : Directive
	{
		return (T)this;
	}
}

public class NonVolatilityDirective : Directive
{
	public NonVolatilityDirective() : base(DirectiveType.NON_VOLATILITY) { }
}

public class SpecificRegisterDirective : Directive
{
	public Register Register { get; }

	public SpecificRegisterDirective(Register register) : base(DirectiveType.SPECIFIC_REGISTER)
	{
		Register = register;
	}
}

public class AvoidRegistersDirective : Directive
{
	public Register[] Registers { get; }

	public AvoidRegistersDirective(Register[] registers) : base(DirectiveType.AVOID_REGISTERS)
	{
		Registers = registers;
	}
}

public class Result
{
	public Instruction? Instruction { get; set; }

	private Handle _Value;
	public Handle Value
	{
		get => _Value;
		set
		{
			_Value = value;
			Connections.ForEach(c => c._Value = value);
		}
	}

	private Format _Format { get; set; } = Assembler.Size.ToFormat();
	public Format Format
	{
		get => _Format;
		set
		{
			_Format = value;
			Connections.ForEach(c => c._Format = value);
		}
	}

	public Size Size => Size.FromFormat(Format);

	public Lifetime Lifetime { get; private set; } = new Lifetime();

	private List<Result> Connections { get; } = new List<Result>();
	private IEnumerable<Result> System => Connections.Concat(new List<Result> { this });
	private IEnumerable<Result> Others => Connections;

	public bool IsExpression => _Value.Type == HandleType.EXPRESSION;
	public bool IsConstant => _Value.Type == HandleType.CONSTANT;
	public bool IsStandardRegister => _Value.Type == HandleType.REGISTER;
	public bool IsMediaRegister => _Value.Type == HandleType.MEDIA_REGISTER;
	public bool IsAnyRegister => _Value.Type == HandleType.REGISTER || _Value.Type == HandleType.MEDIA_REGISTER;
	public bool IsMemoryAddress => _Value.Type == HandleType.MEMORY;
	public bool IsStackVariable => _Value.Instance == HandleInstanceType.STACK_VARIABLE;
	public bool IsDataSectionHandle => _Value.Instance == HandleInstanceType.DATA_SECTION || _Value.Instance == HandleInstanceType.CONSTANT_DATA_SECTION;
	public bool IsModifier => _Value.Type == HandleType.MODIFIER;
	public bool IsInline => _Value.Instance == HandleInstanceType.INLINE;
	public bool IsUnsigned => Format.IsUnsigned();
	public bool IsEmpty => _Value.Type == HandleType.NONE;

	public bool IsReleasable(Unit unit)
	{
		return unit.Scope!.Variables.Values.Any(i => i.Equals(this));
	}

	public Result(Instruction instruction)
	{
		_Value = new Handle();
		Instruction = instruction;
	}

	public Result(Handle value, Format format)
	{
		_Value = value;
		Format = format;
	}

	public Result()
	{
		_Value = new Handle();
		Format = Assembler.Format;
	}

	/// <summary>
	/// Connects this result to the other system (doesn't make duplicates)
	/// </summary>
	private void Connect(IEnumerable<Result> system)
	{
		Connections.AddRange(system.Where(result => System.All(m => m != result)));
	}

	[SuppressMessage("Microsoft.Maintainability", "CA2245", Justification = "Assigning to the variable itself causes an update")]
	private void Update()
	{
		// Update the value to the same because it sends an update wave which corrects all values across the system
		Value = Value;
		Format = Format;

		foreach (var member in Others)
		{
			member.Instruction = Instruction;
			member.Lifetime = Lifetime;
		}
	}

	public void Disconnect()
	{
		foreach (var member in Others)
		{
			var connection = member.Connections.FindIndex(c => c == this);

			if (connection != -1)
			{
				member.Connections.RemoveAt(connection);
			}
		}

		Connections.Clear();
	}

	public void Join(Result parent)
	{
		if (Connections.Any(i => i == parent))
		{
			return;
		}

		Disconnect();

		foreach (var member in System)
		{
			member.Connect(parent.System);
		}

		foreach (var member in parent.System)
		{
			member.Connect(System);
		}

		parent.Update();
	}

	public bool IsExpiring(int position)
	{
		return position == -1 || !Lifetime.IsActive(position + 1);
	}

	public bool IsOnlyValid(int position)
	{
		return Lifetime.IsOnlyActive(position);
	}

	public bool IsValid(int position)
	{
		return Lifetime.IsActive(position);
	}

	public void Use(int position)
	{
		if (position > Lifetime.End)
		{
			Lifetime.End = position;
		}

		if (Lifetime.Start == -1 || position < Lifetime.Start)
		{
			Lifetime.Start = position;
		}

		Value.Use(position);

		foreach (var connection in Connections)
		{
			connection.Lifetime.Start = Lifetime.Start;
			connection.Lifetime.End = Lifetime.End;
		}
	}

	public override bool Equals(object? other)
	{
		return base.Equals(other) || other is Result result && result.Connections.Exists(c => c == this);
	}

	public override int GetHashCode()
	{
		return 0;
	}

	public override string ToString()
	{
		return Value.ToString() ?? string.Empty;
	}
}