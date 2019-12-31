using System;
using System.Collections.Generic;
using System.Linq;

public static class DecimalUtility
{
	public static bool IsInteger(this double number)
	{
		return Math.Abs(number % 1) <= double.Epsilon * 100;
	}

	public static bool IsInteger(this float number)
	{
		return Math.Abs(number % 1) <= float.Epsilon * 100;
	}

	public static T First<T>(this T[] items)
	{
		return items[0];
	}

	public static T Last<T>(this T[] items)
	{
		return items[items.Length - 1];
	}
}

public class Classic
{
	private const string SIGNED_MULTIPLICATION = "imul";

	private const string SIGNED_MULTIPLICATION_SHIFT = "sal";
	private const string SIGNED_DIVISON_SHIFT = "sar";

	private const string UNSIGNED_MULTIPLICATION_SHIFT = "shl";
	private const string UNSIGNED_DIVISON_SHIFT = "shr";

	private static Type GetOperationType(OperatorNode node)
	{
		return node.GetType();
	}

	private static List<long> GetMultiplicationConstants(long multiplier)
	{
		var result = new List<long>();

		var a = (long)Math.Round(Math.Log2((long)Math.Abs(multiplier)));
		var b = (long)Math.Pow(2, a) * Math.Sign(multiplier);
		var c = multiplier - b;

		result.Add(b);

		if (c == -1 || c == 1)
		{
			result.Add(c);
		}
		else if (c != 0)
		{
			var constants = GetMultiplicationConstants(c);
			result.AddRange(constants);
		}

		return result;
	}

	private static Instructions BuildConstantMultiplication(Unit unit, Node operand, NumberNode constant)
	{
		var instructions = new Instructions();
		var reference = References.Register(unit, operand);
		var source = reference.Reference.GetRegister();

		instructions.Append(reference);

		var constants = GetMultiplicationConstants((long)constant.Value);

		if (constants.Count == 1)
		{
			instructions.Append(new Instruction
			(
				SIGNED_MULTIPLICATION_SHIFT, 
				reference.Reference, 
				new NumberReference(Math.Log2(constants[0]), Size.DWORD), 
				Size.DWORD
			));
		}
		else
		{
			// Calculate how many LEA operations there will be
			var operations = constants.Where(c => c != -1 && c != 1).Count();

			if (operations <= 3)
			{
				// Calculate how many registers are needed for the multiplication (Two is the max)
				var registers = Math.Min(operations, 2);

				// Make sure there are calculated amount of registers available
				if (unit.UncriticalRegisterCount < registers)
				{
					goto Fallback;
				}

				// Reserve the primary register
				var a = unit.GetNextRegister();
				var result = Value.GetOperation(a, Size.DWORD);

				// Calculate the first constant to the primary register
				instructions.Append($"lea {a}, [{source}*{constants[0]}]");

				// Check if secondary register is needed
				if (registers > 1)
				{
					// Reserve the secondary register
					var b = unit.GetNextRegister();

					for (int i = 1; i < registers; i++)
					{
						var number = constants[i];

						// Calculate the constant to the secondary register
						instructions.Append($"lea {b}, [{source}*{Math.Abs(number)}]");

						if (number < 0)
						{
							instructions.Append($"sub {a}, {b}");
						}
						else
						{
							instructions.Append($"add {a}, {b}");
						}
					}
				}

				if (constants.Last() == -1)
				{
					instructions.Append($"sub {a}, {source}");
				}
				else if (constants.Last() == 1)
				{
					instructions.Append($"add {a}, {source}");
				}

				// Source register has served its purpose by now
				source.Relax();

				return instructions.SetReference(result);
			}

			Fallback:

			instructions.Append(new Instruction
			(
				SIGNED_MULTIPLICATION,
				reference.Reference,
				new NumberReference(constant.Value, Size.DWORD),
				Size.DWORD
			));
		}

		instructions.SetReference(Value.GetOperation(source, Size.DWORD));

		return instructions;
	}

	private static Instructions Build(Unit unit, string instruction, OperatorNode node)
	{
		// if (node.Left is NumberNode a)
		// {
		// 	return BuildConstantMultiplication(unit, node.Right, a);
		// }
		// else if (node.Right is NumberNode b)
		// {
		// 	return BuildConstantMultiplication(unit, node.Left, b);
		// }

		var instructions = new Instructions();

		//Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.REGISTER, ReferenceType.READ);
		References.Get(unit, instructions, node.Left, node.Right, ReferenceType.REGISTER, ReferenceType.READ, out Reference left, out Reference right);

		var type = GetOperationType(node);
		var size = Size.Get(type.Size);

		instructions.Append(new Instruction(instruction, left, right, size));
		instructions.SetReference(Value.GetOperation(left.GetRegister(), size));

		return instructions;
	}

	private static Instructions Divide(Unit unit, string instruction, OperatorNode node, bool remainder)
	{
		var instructions = new Instructions();

		//Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.REGISTER, ReferenceType.REGISTER);

		//Reference left = operands[0];
		//Reference right = operands[1];

		References.Get(unit, instructions, node.Left, node.Right, ReferenceType.READ, ReferenceType.REGISTER, out Reference left, out Reference right);
		
		if (left.IsRegister() && (left.GetRegister() != unit.EAX && right.GetRegister() == unit.EAX))
		{
			instructions.Append(Memory.Exchange(unit, left.GetRegister(), right.GetRegister()));
		}
		else if (left.GetRegister() != unit.EAX)
		{
			Memory.Move(unit, instructions, left, new RegisterReference(unit.EAX));
		}

		instructions.Append(Memory.Clear(unit, unit.EDX, true));

		var type = GetOperationType(node);
		var size = Size.Get(type.Size);

		instructions.Append(new Instruction(instruction, right));
		instructions.SetReference(Value.GetOperation(remainder ? unit.EDX : unit.EAX, size));

		if (remainder)
		{
			unit.EAX.Reset();
		}

		return instructions;
	}

	public static Instructions Power(Unit unit, OperatorNode node)
	{
		var instructions = new Instructions();

		References.Get(unit, instructions, node.Left, node.Right, ReferenceType.READ, ReferenceType.READ, out Reference left, out Reference right);

		var call = Call.Build(unit, null, "function_integer_power", Size.DWORD, left, right);
		instructions.Append(call);

		return instructions.SetReference(call.Reference);
	}

	public static Instructions Build(Unit unit, OperatorNode node, ReferenceType type)
	{
		if (GetOperationType(node) == Types.DECIMAL)
		{
			return Decimals.Build(unit, node, type);
		}

		if (node.Operator == Operators.ADD)
		{
			return Classic.Build(unit, "add", node);
		}
		else if (node.Operator == Operators.SUBTRACT)
		{
			return Classic.Build(unit, "sub", node);
		}
		else if (node.Operator == Operators.MULTIPLY)
		{
			return Classic.Build(unit, "imul", node);
		}
		else if (node.Operator == Operators.DIVIDE)
		{
			return Classic.Divide(unit, "idiv", node, false);
		}
		else if (node.Operator == Operators.POWER)
		{
			return Classic.Power(unit, node);
		}
		else if (node.Operator == Operators.AND)
		{
			return Classic.Build(unit, "and", node);
		}
		else if (node.Operator == Operators.EXTENDER)
		{
			return Arrays.Build(unit, node, ReferenceType.READ);
		}
		else if (node.Operator == Operators.MODULUS)
		{
			return Classic.Divide(unit, "idiv", node, true);
		}

		return null;
	}
}