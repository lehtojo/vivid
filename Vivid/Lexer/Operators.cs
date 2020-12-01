using System.Collections.Generic;

public static class Operators
{
	public static readonly IndependentOperator COLON = new IndependentOperator(":");

	public static readonly ClassicOperator POWER = new ClassicOperator("^", 15);

	public static readonly ClassicOperator MULTIPLY = new ClassicOperator("*", 12);
	public static readonly ClassicOperator DIVIDE = new ClassicOperator("/", 12);
	public static readonly ClassicOperator MODULUS = new ClassicOperator("%", 12);

	public static readonly ClassicOperator ADD = new ClassicOperator("+", 11);
	public static readonly ClassicOperator SUBTRACT = new ClassicOperator("-", 11);

	public static readonly ClassicOperator SHIFT_LEFT = new ClassicOperator("<|", 10);
	public static readonly ClassicOperator SHIFT_RIGHT = new ClassicOperator("|>", 10);

	public static readonly ComparisonOperator GREATER_THAN = new ComparisonOperator(">", 9);
	public static readonly ComparisonOperator GREATER_OR_EQUAL = new ComparisonOperator(">=", 9);
	public static readonly ComparisonOperator LESS_THAN = new ComparisonOperator("<", 9);
	public static readonly ComparisonOperator LESS_OR_EQUAL = new ComparisonOperator("<=", 9);

	public static readonly ComparisonOperator EQUALS = new ComparisonOperator("==", 8);
	public static readonly ComparisonOperator NOT_EQUALS = new ComparisonOperator("!=", 8);

	public static readonly ClassicOperator BITWISE_AND = new ClassicOperator("&", 7);
	public static readonly ClassicOperator BITWISE_XOR = new ClassicOperator("¤", 6);
	public static readonly ClassicOperator BITWISE_OR = new ClassicOperator("|", 5);

	public static readonly LogicOperator AND = new LogicOperator("and", 4);
	public static readonly LogicOperator OR = new LogicOperator("or", 3);

	public static readonly ActionOperator ASSIGN = new ActionOperator("=", null, 1);
	public static readonly ActionOperator ASSIGN_POWER = new ActionOperator("^=", POWER, 1);
	public static readonly ActionOperator ASSIGN_ADD = new ActionOperator("+=", ADD, 1);
	public static readonly ActionOperator ASSIGN_SUBTRACT = new ActionOperator("-=", SUBTRACT, 1);
	public static readonly ActionOperator ASSIGN_MULTIPLY = new ActionOperator("*=", MULTIPLY, 1);
	public static readonly ActionOperator ASSIGN_DIVIDE = new ActionOperator("/=", DIVIDE, 1);
	public static readonly ActionOperator ASSIGN_MODULUS = new ActionOperator("%=", MODULUS, 1);

	public static readonly ActionOperator ASSIGN_AND = new ActionOperator("&=", BITWISE_AND, 1);
	public static readonly ActionOperator ASSIGN_XOR = new ActionOperator("¤=", BITWISE_XOR, 1);
	public static readonly ActionOperator ASSIGN_OR = new ActionOperator("|=", BITWISE_OR, 1);

	public static readonly IndependentOperator EXCLAMATION = new IndependentOperator("!");

	public static readonly IndependentOperator COMMA = new IndependentOperator(",");
	public static readonly IndependentOperator DOT = new IndependentOperator(".");

	public static readonly IndependentOperator INCREMENT = new IndependentOperator("++");
	public static readonly IndependentOperator DECREMENT = new IndependentOperator("--");

	public static readonly IndependentOperator CAST = new IndependentOperator("->");
	public static readonly IndependentOperator IMPLICATION = new IndependentOperator("=>");

	public static readonly IndependentOperator END = new IndependentOperator("\n");

	private static readonly Dictionary<string, Operator> Map = new Dictionary<string, Operator>();
	private static readonly Dictionary<string, ActionOperator> Actions = new Dictionary<string, ActionOperator>();

	private static void Add(Operator operation)
	{
		Map.Add(operation.Identifier, operation);

		if (operation is ActionOperator action && !string.IsNullOrEmpty(action.Operator?.Identifier))
		{
			Actions.Add(action.Operator.Identifier, action);
		}
	}

	static Operators()
	{
		Add(POWER);

		Add(MULTIPLY);
		Add(DIVIDE);
		Add(MODULUS);
		Add(ADD);
		Add(SUBTRACT);

		Add(SHIFT_LEFT);
		Add(SHIFT_RIGHT);

		Add(GREATER_THAN.SetCounterpart(LESS_OR_EQUAL));
		Add(GREATER_OR_EQUAL.SetCounterpart(LESS_THAN));
		Add(LESS_THAN.SetCounterpart(GREATER_OR_EQUAL));
		Add(LESS_OR_EQUAL.SetCounterpart(GREATER_THAN));
		Add(EQUALS.SetCounterpart(NOT_EQUALS));
		Add(NOT_EQUALS.SetCounterpart(EQUALS));

		Add(BITWISE_AND);
		Add(BITWISE_XOR);
		Add(BITWISE_OR);

		Add(AND);
		Add(OR);

		Add(ASSIGN);
		Add(ASSIGN_POWER);
		Add(ASSIGN_ADD);
		Add(ASSIGN_SUBTRACT);
		Add(ASSIGN_MULTIPLY);
		Add(ASSIGN_DIVIDE);
		Add(ASSIGN_MODULUS);

		Add(ASSIGN_AND);
		Add(ASSIGN_XOR);
		Add(ASSIGN_OR);

		Add(EXCLAMATION);
		Add(COMMA);
		Add(DOT);

		Add(INCREMENT);
		Add(DECREMENT);

		Add(CAST);
		Add(IMPLICATION);

		Add(COLON);
		Add(END);
	}

	public static Operator Get(string text)
	{
		if (Map.TryGetValue(text, out Operator? operation))
		{
			return operation!;
		}

		throw new System.Exception($"Unknown operator '{text}'");
	}

	public static ActionOperator? GetActionOperator(Operator operation)
	{
		if (Actions.TryGetValue(operation.Identifier, out ActionOperator? action))
		{
			return action;
		}

		return null;
	}

	public static bool Exists(string identifier)
	{
		return Map.ContainsKey(identifier);
	}
}