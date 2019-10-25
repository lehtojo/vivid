using System;

public class Classic
{
	private static Type GetOperationType(OperatorNode node)
	{
		try
		{
			Context context = node.GetContext();

			if (context == null)
			{
				throw new Exception("Couldn't resolve operation result type");
			}
			return (Type)context;
		}
		catch (Exception e)
		{
			Console.Error.WriteLine("Error: " + e.Message);
			Environment.Exit(-1);
			return null;
		}
	}

	private static Instructions Build(Unit unit, string instruction, OperatorNode node)
	{
		Instructions instructions = new Instructions();

		Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.REGISTER, ReferenceType.READ);

		Type type = GetOperationType(node);
		Size size = Size.Get(type.Size);

		instructions.Append(new Instruction(instruction, operands[0], operands[1], size));
		instructions.SetReference(Value.GetOperation(operands[0].GetRegister(), size));

		return instructions;
	}

	private static Instructions Divide(Unit unit, string instruction, OperatorNode node, bool remainder)
	{
		Instructions instructions = new Instructions();

		Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.REGISTER, ReferenceType.REGISTER);

		Reference left = operands[0];
		Reference right = operands[1];

		if (left.GetRegister() != unit.EAX && right.GetRegister() == unit.EAX)
		{
			instructions.Append(Memory.Exchange(unit, left.GetRegister(), right.GetRegister()));
		}
		else if (left.GetRegister() != unit.EAX)
		{
			instructions.Append(Memory.Move(unit, left, new RegisterReference(unit.EAX)));
		}

		instructions.Append(Memory.Clear(unit, unit.EDX, true));

		Type type = GetOperationType(node);
		Size size = Size.Get(type.Size);

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
		Instructions instructions = new Instructions();

		Reference[] operands = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.READ, ReferenceType.READ);

		Reference left = operands[0];
		Reference right = operands[1];

		Instructions call = Call.Build(unit, null, "function_integer_power", Size.DWORD, left, right);
		instructions.Append(call);

		return instructions.SetReference(call.Reference);
	}

	public static Instructions Build(Unit unit, OperatorNode node)
	{
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