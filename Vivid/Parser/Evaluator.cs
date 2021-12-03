using System;
using System.Globalization;
using System.Linq;

public static class Evaluator
{
	/// <summary>
	/// Returns the value of the specified node
	/// </summary>
	private static object? GetValue(Node node)
	{
		return node.Instance switch
		{
			NodeType.NUMBER => node.To<NumberNode>().Value,
			NodeType.STRING => node.To<StringNode>().Text,
			NodeType.OPERATOR => EvaluateOperator(node.To<OperatorNode>()),
			_ => null
		};
	}

	/// <summary>
	/// Tries to return the value of the specified node
	/// </summary>
	public static object? TryGetValue(Node node)
	{
		return node.Instance switch
		{
			NodeType.NUMBER => node.To<NumberNode>().Value,
			NodeType.STRING => node.To<StringNode>().Text,
			NodeType.OPERATOR => TryEvaluateOperator(node.To<OperatorNode>()),
			_ => null
		};
	}

	/// <summary>
	/// Tries to compute the specified expression.
	/// Returns true or false if the expression could be computed, otherwise null.
	/// </summary>
	public static bool? TryEvaluateOperator(OperatorNode comparison)
	{
		var left = TryGetValue(comparison.Left);
		var right = TryGetValue(comparison.Right);

		var is_equals_operator = comparison.Operator == Operators.EQUALS;
		var is_not_equals_operator = comparison.Operator == Operators.NOT_EQUALS;

		if ((is_equals_operator || is_not_equals_operator) && 
			 (Common.IsZero(comparison.Left) && Analyzer.GetSource(comparison.Right).Is(NodeType.STACK_ADDRESS) || 
			  Common.IsZero(comparison.Right) && Analyzer.GetSource(comparison.Left).Is(NodeType.STACK_ADDRESS)))
		{
			return is_not_equals_operator;
		}

		if (left == null || right == null) return null;

		if (is_equals_operator) return Equals(left, right);
		if (is_not_equals_operator) return !Equals(left, right);

		if (comparison.Operator == Operators.AND)
		{
			return Convert.ToInt64(left) != 0 && Convert.ToInt64(right) != 0;
		}
		if (comparison.Operator == Operators.OR)
		{
			return Convert.ToInt64(left) != 0 || Convert.ToInt64(right) != 0;
		}

		// The following comparisons need the left and right side values to be comparable
		if (left is not IComparable || right is not IComparable) return null;

		try
		{
			if (left is double || right is double)
			{
				var a = Convert.ToDouble(left);
				var b = Convert.ToDouble(right);

				if (comparison.Operator == Operators.GREATER_THAN) return a > b;
				if (comparison.Operator == Operators.LESS_THAN) return a < b;
				if (comparison.Operator == Operators.GREATER_OR_EQUAL) return a >= b;
				if (comparison.Operator == Operators.LESS_OR_EQUAL) return a <= b;
			}
			else
			{
				var a = Convert.ToInt64(left);
				var b = Convert.ToInt64(right);

				if (comparison.Operator == Operators.GREATER_THAN) return a > b;
				if (comparison.Operator == Operators.LESS_THAN) return a < b;
				if (comparison.Operator == Operators.GREATER_OR_EQUAL) return a >= b;
				if (comparison.Operator == Operators.LESS_OR_EQUAL) return a <= b;
			}

			return null;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Returns the result of comparison as a boolean
	/// </summary>
	public static bool? EvaluateOperator(OperatorNode comparison)
	{
		var left = GetValue(comparison.Left);
		var right = GetValue(comparison.Right);

		if (left == null || right == null) throw Errors.Get(comparison.Position, "Could not resolve a comparison operand");

		if (comparison.Operator == Operators.EQUALS) return Equals(left, right);
		if (comparison.Operator == Operators.NOT_EQUALS) return !Equals(left, right);

		if (comparison.Operator == Operators.AND)
		{
			return Convert.ToInt64(left) != 0 && Convert.ToInt64(right) != 0;
		}
		if (comparison.Operator == Operators.OR)
		{
			return Convert.ToInt64(left) != 0 || Convert.ToInt64(right) != 0;
		}

		// The following comparisons need the left and right side values to be comparable
		if (left is not IComparable x || right is not IComparable y) throw new ArgumentException("One of the comparison operands was not comparable");

		if (comparison.Operator == Operators.GREATER_THAN) return x.CompareTo(y) > 0;
		if (comparison.Operator == Operators.LESS_THAN) return x.CompareTo(y) < 0;
		if (comparison.Operator == Operators.GREATER_OR_EQUAL) return x.CompareTo(y) >= 0;
		if (comparison.Operator == Operators.LESS_OR_EQUAL) return x.CompareTo(y) <= 0;

		throw new ArgumentException("Unsupported comparison");
	}

	/// <summary>
	/// Evaluates the condition of the specified if-statement and returns the context of the branch which will be executed.
	/// </summary>
	private static Context? Evaluate(IfNode statement)
	{
		var value = Convert.ToInt64(GetValue(statement.Condition));

		if (value != 0)
		{
			// Evaluate the body of the if-statement
			EvaluateNode(statement.Body.Context, statement.Body);

			return statement.Body.Context;
		}

		var successor = statement.Successor;

		if (successor == null) return null;

		if (successor.Is(NodeType.ELSE))
		{
			var node = successor.To<ElseNode>();

			// Evaluate the body of the else-statement
			EvaluateNode(node.Body.Context, node.Body);

			return node.Body.Context;
		}

		return Evaluate((IfNode)successor);
	}

	/// <summary>
	/// Finds if-statements and tries to evaluate their conditions
	/// </summary>
	private static void EvaluateNode(Context context, Node root)
	{
		foreach (var iterator in root)
		{
			if (!iterator.Is(NodeType.IF)) continue;

			var statement = iterator.To<IfNode>();
			var result = Evaluate(statement);

			if (result != null)
			{
				context.Merge(result);
			}
		}
	}

	/// <summary>
	/// Tries to evaluate the result of the specified expression
	/// </summary>
	private static void EvaluateLogicalOperator(OperatorNode expression)
	{
		if (expression.Left.Is(Operators.AND) || expression.Left.Is(Operators.OR))
		{
			EvaluateLogicalOperator(expression.Left.To<OperatorNode>());
		}

		if (expression.Right.Is(Operators.AND) || expression.Right.Is(Operators.OR))
		{
			EvaluateLogicalOperator(expression.Right.To<OperatorNode>());
		}

		if (!expression.Left.Is(NodeType.NUMBER) && !expression.Right.Is(NodeType.NUMBER)) return;

		var a = expression.Left is NumberNode x && x.Value.Equals(0L);
		var b = expression.Right is NumberNode y && y.Value.Equals(0L);

		if (a && b)
		{
			expression.Replace(new NumberNode(Parser.Format, 0L, expression.Position));
			return;
		}

		expression.Replace(a ? expression.Right : expression.Left);
	}

	/// <summary>
	/// Evaluates expressions under the specified node
	/// </summary>
	private static void EvaluateLogicalOperators(Node root)
	{
		foreach (var iterator in root)
		{
			if (iterator.Is(Operators.AND) || iterator.Is(Operators.OR))
			{
				EvaluateLogicalOperator(iterator.To<OperatorNode>());
			}
			else
			{
				EvaluateLogicalOperators(iterator);
			}
		}
	}

	/// <summary>
	/// Tries to evaluate the specified conditional statement
	/// </summary>
	private static bool EvaluateConditionalStatement(IfNode root)
	{
		if (root.Condition is not NumberNode condition || root.GetConditionInitialization().Any())
		{
			return false;
		}

		if (!condition.Value.Equals(0L))
		{
			// None of the successors will execute
			root.GetSuccessors().ForEach(i => i.Remove());

			if (root.Predecessor == null)
			{
				// Since the root node is the first branch, the body can be inlined
				root.ReplaceWithChildren(root.Body.Clone());
			}
			else
			{
				// Since there is a branch before the root node, the root can be replaced with an else statement
				root.Replace(new ElseNode(root.Body.Context, root.Body.Clone(), root.Position, root.Body.End));
			}
		}
		else if (root.Successor == null || root.Predecessor != null)
		{
			root.Remove();
		}
		else
		{
			if (root.Successor is ElseIfNode x)
			{
				root.Replace(new IfNode(x.Body.Context, x.Condition, x.Body, x.Position, x.Body.End));
				x.Remove();
				return true;
			}

			root.ReplaceWithChildren(root.Successor);
			root.Successor.Remove();
		}

		return true;
	}

	/// <summary>
	/// Evaluates conditional statements under the specified node
	/// </summary>
	private static void EvaluateConditionalStatements(Node root)
	{
		var iterator = root.First;

		while (iterator != null)
		{
			if (iterator is IfNode x)
			{
				if (EvaluateConditionalStatement(x))
				{
					iterator = root.First;
				}
				else
				{
					iterator = iterator.Next;
				}

				continue;
			}
			
			if (iterator.Is(NodeType.ELSE))
			{
				iterator = iterator.Next;
				continue;
			}
			
			EvaluateConditionalStatements(iterator);
			iterator = iterator.Next;
		}
	}

	/// <summary>
	/// Evaluates expressions under the specified node
	/// </summary>
	private static void EvaluateCompilesNodes(Node root)
	{
		var expressions = root.FindAll(NodeType.COMPILES);

		foreach (var expression in expressions)
		{
			var result = 1L;

			if (expression.Find(i => i is IResolvable x && x.GetStatus().IsProblematic) != null)
			{
				result = 0L;
			}

			expression.Replace(new NumberNode(Parser.Format, result, expression.Position));
		}
	}

	/// <summary>
	/// Evaluates expressions in all the functions inside the specified context
	/// </summary>
	public static void Evaluate(Context context)
	{
		foreach (var type in context.Types.Values)
		{
			Evaluate(type);
		}

		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			if (implementation.Node == null) continue;

			// Should evaluate as long as the node tree changes
			EvaluateCompilesNodes(implementation.Node!);
			EvaluateLogicalOperators(implementation.Node!);
			EvaluateConditionalStatements(implementation.Node!);
			Evaluate(implementation);
		}
	}

	public static Status Evaluate(Context context, Node root)
	{
		try
		{
			// Apply constants and such
			Analyzer.Analyze(root, context);

			EvaluateNode(context, root);
			return Status.OK;
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}
	}
}