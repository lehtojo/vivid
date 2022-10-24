using System.Collections.Generic;

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
	public List<Register> Registers { get; }

	public AvoidRegistersDirective(List<Register> registers) : base(DirectiveType.AVOID_REGISTERS)
	{
		Registers = registers;
	}
}

public static class Trace
{
	public static List<Directive> For(Unit unit, Result result)
	{
		var directives = new List<Directive>();
		var reorders = new List<ReorderInstruction>();

		var usages = result.Lifetime.Usages;
		var instructions = unit.Instructions;

		var start = usages.Count;
		var end = -1;

		// Find the last usage
		foreach (var usage in usages)
		{
			var position = unit.Instructions.IndexOf(usage);
			if (position > end) { end = position; }
			if (position < start) { start = position; }
		}

		if (unit.Position > start) { start = unit.Position; }

		// Do not process results, which have already expired
		if (start > end) return new List<Directive>();

		for (var i = start; i <= end; i++)
		{
			var instruction = instructions[i];

			if (instruction.Type == InstructionType.CALL)
			{
				directives.Add(new NonVolatilityDirective());
				continue;
			}

			if (instruction.Type == InstructionType.REORDER)
			{
				reorders.Add((ReorderInstruction)instruction);
			}
		}

		var avoid = new List<Register>();

		// Look for return instructions, which have return values, if the current function has a return type
		if (!Primitives.IsPrimitive(unit.Function.ReturnType, Primitives.UNIT))
		{
			for (var i = start; i <= end; i++)
			{
				var instruction = instructions[i];

				// Look for return instructions, which have return values
				if (instruction.Type != InstructionType.RETURN) continue;
				if (instruction.To<ReturnInstruction>().Object == null) continue;

				// If the returned object is the specified result, it should try to use the return register, otherwise it should avoid it
				var register = instruction.To<ReturnInstruction>().ReturnRegister;

				if (instruction.To<ReturnInstruction>().Object != result)
				{
					avoid.Add(register);
				}
				else {
					directives.Add(new SpecificRegisterDirective(register));
				}

				break;
			}
		}

		if (Settings.IsX64)
		{
			for (var i = start; i <= end; i++)
			{
				var instruction = instructions[i];

				// Look for division instructions
				if (instruction.Type != InstructionType.DIVISION) continue;

				// If the first operand of the division is the specified result, it should try to use the numerator register, otherwise it should avoid it
				var register = unit.GetNumeratorRegister();

				if (instruction.To<DivisionInstruction>().First != result)
				{
					avoid.Add(register);
				}
				else
				{
					directives.Add(new SpecificRegisterDirective(register));
				}

				// All results should avoid the remainder register
				avoid.Add(unit.GetRemainderRegister());
				break;
			}
		}

		foreach (var reorder in reorders)
		{
			// Check if the specified result is relocated to any register
			for (var i = 0; i < reorder.Destinations.Count; i++)
			{
				var destination = reorder.Destinations[i];
				if (destination.Instance != HandleInstanceType.REGISTER) continue;

				var register = destination.To<RegisterHandle>().Register;

				if (reorder.Sources[i] != result)
				{
					avoid.Add(register);
					continue;
				}

				directives.Add(new SpecificRegisterDirective(register));
				break;
			}
		}

		directives.Add(new AvoidRegistersDirective(avoid));
		return directives;
	}

	/// <summary>
	/// Returns whether the specified result lives through at least one call
	/// </summary>
	public static bool IsUsedAfterCall(Unit unit, Result result)
	{
		var usages = result.Lifetime.Usages;
		var instructions = unit.Instructions;

		var start = usages.Count;
		var end = -1;

		// Find the last usage
		foreach (var usage in usages)
		{
			var position = instructions.IndexOf(usage);
			if (position > end) { end = position; }
			if (position < start) { start = position; }
		}

		if (unit.Position > start) { start = unit.Position; }

		// Do not process results, which have already expired
		if (start > end) return false;

		for (var i = start; i < end; i++)
		{
			if (instructions[i].Type == InstructionType.CALL) return true;
		}

		return false;
	}

	/// <summary>
	/// Returns whether the specified result stays constant during the lifetime of the specified parent
	/// </summary>
	public static bool IsLoadingRequired(Unit unit, Result result)
	{
		var usages = result.Lifetime.Usages;
		var instructions = unit.Instructions;

		var start = usages.Count;
		var end = -1;

		// Find the last usage
		foreach (var usage in usages)
		{
			var position = instructions.IndexOf(usage);
			if (position > end) { end = position; }
			if (position < start) { start = position; }
		}

		if (unit.Position > start) { start = unit.Position + 1; }

		// Do not process results, which have already expired
		if (start > end) return false;

		for (var i = start; i < end; i++)
		{
			var instruction = instructions[i];
			var type = instruction.Type;

			if (type == InstructionType.CALL) return true;
			if (type == InstructionType.GET_OBJECT_POINTER && instruction.To<GetObjectPointerInstruction>().Mode == AccessMode.WRITE) return true;
			if (type == InstructionType.GET_MEMORY_ADDRESS && instruction.To<GetMemoryAddressInstruction>().Mode == AccessMode.WRITE) return true;
		}

		return false;
	}
}