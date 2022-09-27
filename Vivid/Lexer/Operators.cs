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
	public static readonly Operator AND = new("and", OperatorType.LOGICAL, 4);
	public static readonly Operator OR = new("or", OperatorType.LOGICAL, 3);
	public static readonly AssignmentOperator ASSIGN = new("=", null, 1);
	public static readonly AssignmentOperator ASSIGN_POWER = new("^=", POWER, 1);
	public static readonly AssignmentOperator ASSIGN_ADD = new("+=", ADD, 1);
	public static readonly AssignmentOperator ASSIGN_SUBTRACT = new("-=", SUBTRACT, 1);
	public static readonly AssignmentOperator ASSIGN_MULTIPLY = new("*=", MULTIPLY, 1);
	public static readonly AssignmentOperator ASSIGN_DIVIDE = new("/=", DIVIDE, 1);
	public static readonly AssignmentOperator ASSIGN_MODULUS = new("%=", MODULUS, 1);
	public static readonly AssignmentOperator ASSIGN_AND = new("&=", BITWISE_AND, 1);
	public static readonly AssignmentOperator ASSIGN_XOR = new("¤=", BITWISE_XOR, 1);
	public static readonly AssignmentOperator ASSIGN_OR = new("|=", BITWISE_OR, 1);
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

	public static readonly Dictionary<string, Operator> All = new();
	public static readonly Dictionary<string, AssignmentOperator> AssignmentOperators = new();

	private static void Add(Operator operation)
	{
		All.Add(operation.Identifier, operation);

		if (operation.Type == OperatorType.ASSIGNMENT && operation.To<AssignmentOperator>().Operator != null && operation.To<AssignmentOperator>().Operator!.Identifier.Length > 0)
		{
			AssignmentOperators.Add(((AssignmentOperator)operation).Operator!.Identifier, operation.To<AssignmentOperator>());
		}
	}

	public static void Initialize()
	{
		All.Clear();
		AssignmentOperators.Clear();

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
		if (All.TryGetValue(text, out Operator? operation))
		{
			return operation!;
		}

		throw new System.Exception($"Unknown operator '{text}'");
	}

	public static AssignmentOperator? GetAssignmentOperator(Operator operation)
	{
		if (AssignmentOperators.TryGetValue(operation.Identifier, out AssignmentOperator? action)) return action;
		return null;
	}

	public static bool Exists(string identifier)
	{
		return All.ContainsKey(identifier);
	}
}