using System.Collections.Generic;

public static class Operators
{
	public static readonly IndependentOperator COLON = new(":");

	public static readonly ClassicOperator POWER = new("^", 15);

	public static readonly ClassicOperator MULTIPLY = new("*", 12);
	public static readonly ClassicOperator DIVIDE = new("/", 12);
	public static readonly ClassicOperator MODULUS = new("%", 12);

	public static readonly ClassicOperator ADD = new("+", 11);
	public static readonly ClassicOperator SUBTRACT = new("-", 11);

	public static readonly ClassicOperator SHIFT_LEFT = new("<|", 10);
	public static readonly ClassicOperator SHIFT_RIGHT = new("|>", 10);

	public static readonly ComparisonOperator GREATER_THAN = new(">", 9);
	public static readonly ComparisonOperator GREATER_OR_EQUAL = new(">=", 9);
	public static readonly ComparisonOperator LESS_THAN = new("<", 9);
	public static readonly ComparisonOperator LESS_OR_EQUAL = new("<=", 9);

	public static readonly ComparisonOperator EQUALS = new("==", 8);
	public static readonly ComparisonOperator NOT_EQUALS = new("!=", 8);
	public static readonly ComparisonOperator ABSOLUTE_EQUALS = new("===", 8);
	public static readonly ComparisonOperator ABSOLUTE_NOT_EQUALS = new("!==", 8);

	public static readonly ClassicOperator BITWISE_AND = new("&", 7);
	public static readonly ClassicOperator BITWISE_XOR = new("¤", 6);
	public static readonly ClassicOperator BITWISE_OR = new("|", 5);

	public static readonly IndependentOperator RANGE = new("..");

	public static readonly LogicOperator AND = new("and", 4);
	public static readonly LogicOperator OR = new("or", 3);

	public static readonly ActionOperator ASSIGN = new("=", null, 1);
	public static readonly ActionOperator ASSIGN_POWER = new("^=", POWER, 1);
	public static readonly ActionOperator ASSIGN_ADD = new("+=", ADD, 1);
	public static readonly ActionOperator ASSIGN_SUBTRACT = new("-=", SUBTRACT, 1);
	public static readonly ActionOperator ASSIGN_MULTIPLY = new("*=", MULTIPLY, 1);
	public static readonly ActionOperator ASSIGN_DIVIDE = new("/=", DIVIDE, 1);
	public static readonly ActionOperator ASSIGN_MODULUS = new("%=", MODULUS, 1);

	public static readonly ActionOperator ASSIGN_AND = new("&=", BITWISE_AND, 1);
	public static readonly ActionOperator ASSIGN_XOR = new("¤=", BITWISE_XOR, 1);
	public static readonly ActionOperator ASSIGN_OR = new("|=", BITWISE_OR, 1);

	public static readonly IndependentOperator EXCLAMATION = new("!");

	public static readonly IndependentOperator COMMA = new(",");
	public static readonly IndependentOperator DOT = new(".");

	public static readonly IndependentOperator INCREMENT = new("++");
	public static readonly IndependentOperator DECREMENT = new("--");

	public static readonly IndependentOperator ARROW = new("->");
	public static readonly IndependentOperator HEAVY_ARROW = new("=>");

	public static readonly IndependentOperator END = new("\n");

	/// NOTE: The user should not be able to use this operator since it is meant for internal usage
	public static readonly ClassicOperator ASSIGN_EXCHANGE_ADD = new("<+>", 11);

	public static readonly Dictionary<string, Operator> Definitions = new();
	public static readonly Dictionary<string, ActionOperator> Actions = new();

	private static void Add(Operator operation)
	{
		Definitions.Add(operation.Identifier, operation);

		if (operation is ActionOperator action && !string.IsNullOrEmpty(action.Operator?.Identifier))
		{
			Actions.Add(action.Operator.Identifier, action);
		}
	}

	public static void Initialize()
	{
		Definitions.Clear();
		Actions.Clear();

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
		Add(ABSOLUTE_EQUALS.SetCounterpart(ABSOLUTE_NOT_EQUALS));
		Add(ABSOLUTE_NOT_EQUALS.SetCounterpart(ABSOLUTE_EQUALS));

		Add(BITWISE_AND);
		Add(BITWISE_XOR);
		Add(BITWISE_OR);

		Add(RANGE);

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

		Add(ARROW);
		Add(HEAVY_ARROW);

		Add(COLON);
		Add(END);
		Add(ASSIGN_EXCHANGE_ADD);
	}

	public static Operator Get(string text)
	{
		if (Definitions.TryGetValue(text, out Operator? operation))
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
		return Definitions.ContainsKey(identifier);
	}
}