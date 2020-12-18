using System.Collections.Generic;
using System.Linq;
using System;

public interface IHint
{
	bool IsRelevant(int perspective);
}

public class DirectToNonVolatileRegister : IHint
{
	public Instruction[] Calls { get; }

	public DirectToNonVolatileRegister(Instruction[] calls)
	{
		Calls = calls;
	}

	public bool IsRelevant(int perspective)
	{
		return Calls.Any(c => c.Position > perspective);
	}
}

public class DirectToReturnRegister : IHint
{
	public List<ReturnInstruction> Objectives { get; } = new List<ReturnInstruction>();

	public DirectToReturnRegister(ReturnInstruction objective)
	{
		Objectives.Add(objective);
	}

	public ReturnInstruction GetClosestReturnInstruction(int perspective)
	{
		return Objectives.Where(r => r.Position > perspective).OrderBy(r => r.Position - perspective).First();
	}

	public bool IsRelevant(int perspective)
	{
		return Objectives.Any(c => c.Position > perspective);
	}
}

public class DirectToRegister : IHint
{
	public Instruction Objective { get; }
	public Register Register { get; }

	public DirectToRegister(Instruction objective, Register register)
	{
		Objective = objective;
		Register = register;
	}

	public bool IsRelevant(int perspective)
	{
		return Objective.Position > perspective;
	}
}

public class AvoidRegisters : IHint
{
	public Instruction End { get; }
	public Register[] Registers { get; }

	public AvoidRegisters(Instruction end, Register[] registers)
	{
		End = end;
		Registers = registers;
	}

	public bool IsRelevant(int perspective)
	{
		return perspective <= End.Position;
	}
}

public class Result
{
	public Instruction? Instruction { get; set; }

	private Metadata _Metadata = new Metadata();
	public Metadata Metadata
	{
		get => _Metadata;
		set
		{
			_Metadata = value;
			Connections.ForEach(c => c._Metadata = value);
		}
	}

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

	public List<IHint> Hints { get; private set; } = new List<IHint>();

	public Size Size => Size.FromFormat(Format);
	public bool IsUnsigned => Format.IsUnsigned();

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
	public bool IsStackVariable => _Value is StackVariableHandle;
	public bool IsDataSectionHandle => _Value is DataSectionHandle;
	public bool IsEmpty => _Value.Type == HandleType.NONE;

	public bool IsReleasable()
	{
		// Prevent releasing this pointer
		if (Metadata.IsPrimarilyVariable && Metadata.Primary!.To<VariableAttribute>().Variable.IsSelfPointer)
		{
			return false;
		}

		return Metadata.Variables.Any() && Metadata.Variables.All(v => v.Variable.IsPredictable);
	}

	public Result(Instruction instruction)
	{
		_Value = new Handle();
		Instruction = instruction;
	}

	public Result(Instruction instruction, Handle value)
	{
		_Value = value;
		Instruction = instruction;
	}

	public Result(Handle value, Format format)
	{
		_Value = value;
		Format = format;
	}

	public Result(Format format)
	{
		_Value = new Handle();
		Format = format;
	}

	public Result()
	{
		_Value = new Handle();
		Format = Assembler.Format;
	}

	/// <summary>
	/// Determines the currently most imporant hint, if any exists
	/// </summary>
	public IHint? GetRecommendation(Unit unit)
	{
		var relevant_hints = Hints.FindAll(h => h.IsRelevant(unit.Position));

		if (Hints.Count == 0)
		{
			return null;
		}

		if (Hints.Exists(h => h is DirectToNonVolatileRegister))
		{
			return Hints.Find(h => h is DirectToNonVolatileRegister);
		}
		else if (Hints.Exists(h => h is DirectToReturnRegister))
		{
			return Hints.Find(h => h is DirectToReturnRegister);
		}
		else if (Hints.Exists(h => h is AvoidRegisters))
		{
			return Hints.Find(h => h is AvoidRegisters);
		}
		else if (Hints.Exists(h => h is DirectToRegister))
		{
			return Hints.Find(h => h is DirectToRegister);
		}

		return null;
	}

	public void AddHint(IHint hint)
	{
		if (!Hints.Exists(h => h.GetType() == hint.GetType()))
		{
			Hints.Add(hint);
		}
	}

	/// <summary>
	/// Connects this result to the other system (doesn't make duplicates)
	/// </summary>
	private void Connect(IEnumerable<Result> system)
	{
		Connections.AddRange(system.Where(result => System.All(m => m != result)));
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA2245", Justification = "Assigning to the variable itself causes an update")]
	private void Update()
	{
		// Update the value to the same because it sends an update wave which corrects all values across the system
		Metadata = Metadata;
		Value = Value;
		Format = Format;

		/// TODO: Turn into automatic update?
		foreach (var member in Others)
		{
			member.Instruction = Instruction;
			member.Lifetime = Lifetime;
			member.Hints = Hints;
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
		Connections.ForEach(c => c.Lifetime = Lifetime.Clone());
	}

	public void Set(Handle value, bool force = false)
	{
		if (force)
		{
			// Do not care about the value permissions
			System.ToList().ForEach(r => r._Value = value);
		}
		else
		{
			Value = value;
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
		return Value?.ToString() ?? string.Empty;
	}
}