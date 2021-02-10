using System;
using System.Collections.Generic;
using System.Linq;

public static class Analysis
{
	public static bool IsInstructionAnalysisEnabled { get; set; } = false;
	public static bool IsUnwrapAnalysisEnabled { get; set; } = false;
	public static bool IsMathematicalAnalysisEnabled { get; set; } = false;
	public static bool IsRepetitionAnalysisEnabled { get; set; } = false;
	public static bool IsFunctionInliningEnabled { get; set; } = false;

	/// <summary>
	/// Creates a node tree representing the specified components
	/// </summary>
	/// <param name="components">Components representing an expression</param>
	/// <returns>Node tree representing the specified components</returns>
	private static Node Recreate(List<Component> components)
	{
		var result = Recreate(components.First());

		for (var i = 1; i < components.Count; i++)
		{
			var component = components[i];

			if (component is NumberComponent number_component)
			{
				if (Numbers.IsZero(number_component.Value))
				{
					continue;
				}

				var is_negative = number_component.Value is long a && a < 0L || number_component.Value is double b && b < 0.0;
				var number = new NumberNode(number_component.Value is long ? Assembler.Format : Format.DECIMAL, number_component.Value);

				result = is_negative
					? new OperatorNode(Operators.SUBTRACT).SetOperands(result, number.Negate())
					: new OperatorNode(Operators.ADD).SetOperands(result, number);
			}

			if (component is VariableComponent variable_component)
			{
				// When the coefficient is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
				if (Numbers.IsZero(variable_component.Coefficient))
				{
					continue;
				}

				var node = CreateVariableWithOrder(variable_component.Variable, variable_component.Order);
				bool is_coefficient_negative;

				// When the coefficient is exactly one (double), the coefficient can be ignored, meaning the inaccuracy of the comparison is expected
				if (variable_component.Coefficient is double b)
				{
					is_coefficient_negative = b < 0.0;

					if (Math.Abs(b) != 1.0)
					{
						node = new OperatorNode(Operators.MULTIPLY).SetOperands(
							node,
							new NumberNode(Format.DECIMAL, Math.Abs(b))
						);
					}
				}
				else
				{
					is_coefficient_negative = (long)variable_component.Coefficient < 0;

					if (Math.Abs((long)variable_component.Coefficient) != 1L)
					{
						node = new OperatorNode(Operators.MULTIPLY).SetOperands(
							node,
							new NumberNode(Assembler.Format, Math.Abs((long)variable_component.Coefficient))
						);
					}
				}

				result = new OperatorNode(is_coefficient_negative ? Operators.SUBTRACT : Operators.ADD).SetOperands(result, node);
			}

			if (component is ComplexComponent complex_component)
			{
				result = new OperatorNode(complex_component.IsNegative ? Operators.SUBTRACT : Operators.ADD).SetOperands(
					result,
					complex_component.Node.Clone()
				);
			}

			if (component is VariableProductComponent product)
			{
				var is_negative = product.Coefficient is long a && a < 0L || product.Coefficient is double b && b < 0.0;

				if (is_negative)
				{
					product.Negate();
				}

				var other = Recreate(product);

				result = is_negative
					? new OperatorNode(Operators.SUBTRACT).SetOperands(result, other)
					: new OperatorNode(Operators.ADD).SetOperands(result, other);
			}
		}

		return result;
	}

	/// <summary>
	/// Builds a node tree representing a variable with an order
	/// </summary>
	/// <param name="variable">Target variable</param>
	/// <param name="order">Order of the variable</param>
	private static Node CreateVariableWithOrder(Variable variable, int order)
	{
		if (order == 0)
		{
			return new NumberNode(Assembler.Format, 1L);
		}

		var result = (Node)new VariableNode(variable);

		for (var i = 1; i < Math.Abs(order); i++)
		{
			result = new OperatorNode(Operators.MULTIPLY).SetOperands(result, new VariableNode(variable));
		}

		if (order < 0)
		{
			result = new OperatorNode(Operators.DIVIDE).SetOperands(new NumberNode(Assembler.Format, 1L), result);
		}

		return result;
	}

	/// <summary>
	/// Creates a node tree representing the specified component
	/// </summary>
	/// <returns>Node tree representing the specified component</returns>
	private static Node Recreate(Component component)
	{
		if (component is NumberComponent number_component)
		{
			return new NumberNode(number_component.Value is long ? Assembler.Format : Format.DECIMAL, number_component.Value);
		}

		if (component is VariableComponent variable_component)
		{
			// When the coefficient is exactly zero (double), the variable can be ignored, meaning the inaccuracy of the comparison is expected
			if (variable_component.Coefficient is double a && a == 0.0)
			{
				return new NumberNode(Format.DECIMAL, 0.0);
			}

			if (variable_component.Coefficient  is long b && b == 0.0)
			{
				return new NumberNode(Assembler.Format, 0L);
			}

			var result = CreateVariableWithOrder(variable_component.Variable, variable_component.Order);

			// When the coefficient is exactly one (double), the coefficient can be ignored, meaning the inaccuracy of the comparison is expected
			if (variable_component.Coefficient is double c)
			{
				if (c == 1.0)
				{
					return result;
				}

				return new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(Format.DECIMAL, c));
			}

			return !Numbers.IsOne(variable_component.Coefficient)
				? new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(Assembler.Format, variable_component.Coefficient))
				: result;
		}

		if (component is ComplexComponent complex_component)
		{
			return complex_component.IsNegative
				? new NegateNode(complex_component.Node)
				: complex_component.Node;
		}

		if (component is VariableProductComponent product)
		{
			var result = CreateVariableWithOrder(product.Variables.First().Variable, product.Variables.First().Order);

			for (var i = 1; i < product.Variables.Count; i++)
			{
				var variable = product.Variables[i];

				result = new OperatorNode(Operators.MULTIPLY).SetOperands(result, CreateVariableWithOrder(variable.Variable, variable.Order));
			}

			return !Numbers.Equals(product.Coefficient, 1L)
				? new OperatorNode(Operators.MULTIPLY).SetOperands(result, new NumberNode(Assembler.Format, product.Coefficient))
				: result;
		}

		throw new NotImplementedException("Unsupported component encountered while recreating");
	}

	/// <summary>
	/// Negates the all the specified components using their internal negation method
	/// </summary>
	/// <param name="components">Components to negate</param>
	/// <returns>The specified components</returns>
	private static List<Component> Negate(List<Component> components)
	{
		components.ForEach(c => c.Negate());
		return components;
	}

	private static List<Component> CollectComponents(Node node)
	{
		var result = new List<Component>();

		if (node.Is(NodeType.NUMBER))
		{
			result.Add(new NumberComponent(node.To<NumberNode>().Value));
		}
		else if (node.Is(NodeType.VARIABLE))
		{
			result.Add(new VariableComponent(node.To<VariableNode>().Variable));
		}
		else if (node.Is(NodeType.OPERATOR))
		{
			result.AddRange(CollectComponents(node.To<OperatorNode>()));
		}
		else if (node.Is(NodeType.CONTENT))
		{
			if (!Equals(node.First, null))
			{
				result.AddRange(CollectComponents(node.First));
			}
		}
		else if (node.Is(NodeType.NEGATE))
		{
			result.AddRange(Negate(CollectComponents(node.First!)));
		}
		else
		{
			result.Add(new ComplexComponent(node));
		}

		return result;
	}

	private static List<Component> CollectComponents(OperatorNode node)
	{
		var left_components = CollectComponents(node.Left);
		var right_components = CollectComponents(node.Right);

		if (Equals(node.Operator, Operators.ADD))
		{
			return SimplifyAddition(left_components, right_components);
		}

		if (Equals(node.Operator, Operators.SUBTRACT))
		{
			return SimplifySubtraction(left_components, right_components);
		}

		if (Equals(node.Operator, Operators.MULTIPLY))
		{
			return SimplifyMultiplication(left_components, right_components);
		}

		if (Equals(node.Operator, Operators.DIVIDE))
		{
			return SimplifyDivision(left_components, right_components);
		}

		return new List<Component>
		{
			new ComplexComponent(new OperatorNode(node.Operator).SetOperands(Recreate(left_components), Recreate(right_components)))
		};
	}

	/// <summary>
	/// Tries to simplify the specified components
	/// </summary>
	/// <param name="components">Components to simplify</param>
	/// <returns>A simplified version of the components</returns>
	private static List<Component> Simplify(List<Component> components)
	{
		if (components.Count <= 1)
		{
			return components;
		}

		for (var i = 0; i < components.Count; i++)
		{
			var current = components[i];

			// Start iterating from the next component
			for (var j = i + 1; j < components.Count;)
			{
				var result = current + components[j];

				// Move to the next component if the two components could not be added together
				if (result == null)
				{
					j++;
					continue;
				}

				// Remove the components added together
				components.RemoveAt(j);
				components.RemoveAt(i);

				// Apply the changes
				components.Insert(i, result);
				current = result;
			}
		}

		return components;
	}

	/// <summary>
	/// Simplifies the addition between the specified operands
	/// </summary>
	/// <param name="left_components">Components of the left hand side</param>
	/// <param name="right_components">Components of the right hand side</param>
	/// <returns>Simplified version of the expression</returns>
	private static List<Component> SimplifyAddition(List<Component> left_components, List<Component> right_components)
	{
		return Simplify(left_components.Concat(right_components).ToList());
	}

	/// <summary>
	/// Simplifies the subtraction between the specified operands
	/// </summary>
	/// <param name="left_components">Components of the left hand side</param>
	/// <param name="right_components">Components of the right hand side</param>
	/// <returns>Simplified version of the expression</returns>
	private static List<Component> SimplifySubtraction(List<Component> left_components, List<Component> right_components)
	{
		Negate(right_components);

		return SimplifyAddition(left_components, right_components);
	}

	/// <summary>
	/// Simplifies the multiplication between the specified operands
	/// </summary>
	/// <param name="left_components">Components of the left hand side</param>
	/// <param name="right_components">Components of the right hand side</param>
	/// <returns>Simplified version of the expression</returns>
	private static List<Component> SimplifyMultiplication(List<Component> left_components, List<Component> right_components)
	{
		var components = new List<Component>();

		foreach (var left_component in left_components)
		{
			foreach (var right_component in right_components)
			{
				var result = left_component * right_component ?? new ComplexComponent(
					new OperatorNode(Operators.MULTIPLY).SetOperands(Recreate(left_component), Recreate(right_component))
				);

				components.Add(result);
			}
		}

		return Simplify(components);
	}

	/// <summary>
	/// Simplifies the division between the specified operands
	/// </summary>
	/// <param name="left_components">Components of the left hand side</param>
	/// <param name="right_components">Components of the right hand side</param>
	/// <returns>Simplified version of the expression</returns>
	private static List<Component> SimplifyDivision(List<Component> left_components, List<Component> right_components)
	{
		if (left_components.Count == 1 && right_components.Count == 1)
		{
			var result = left_components.First() / right_components.First();

			if (result != null)
			{
				return new List<Component> { result };
			}
		}

		return new List<Component>
		{
			new ComplexComponent(new OperatorNode(Operators.DIVIDE).SetOperands(Recreate(left_components), Recreate(right_components)))
		};
	}

	/// <summary>
	/// Tries to simplify the specified node
	/// </summary>
	private static Node GetSimplifiedValue(Node value)
	{
		var components = CollectComponents(value);
		var simplified = Recreate(components);

		return simplified;
	}

	/// <summary>
	/// Returns whether the specified node is primitive that is whether it contains only operators, numbers, parameter- or local variables
	/// </summary>
	/// <returns>True if the definition is primitive, otherwise false</returns>
	public static bool IsPrimitive(Node node)
	{
		return node.Find(n => !(n.Is(NodeType.NUMBER) || n.Is(NodeType.OPERATOR) || n.Is(NodeType.VARIABLE) && n.To<VariableNode>().Variable.IsPredictable)) == null;
	}

	private static Node? GetBranch(Node node)
	{
		return node.FindParent(p => p.Is(NodeType.LOOP, NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE));
	}

	private static List<Node> GetDenylist(Node node)
	{
		var denylist = new List<Node>();
		var branch = node;

		while ((branch = GetBranch(branch!)) != null)
		{
			if (branch is IfNode x)
			{
				denylist.AddRange(x.GetBranches().Where(b => b != x));
			}
			else if (branch is ElseIfNode y)
			{
				denylist.AddRange(y.GetRoot().GetBranches().Where(b => b != y));
			}
			else if (branch is ElseNode z)
			{
				denylist.AddRange(z.GetRoot().GetBranches().Where(b => b != z));
			}
		}

		return denylist;
	}

	/// <summary>
	/// Returns whether the specified variable will be used in the future starting from the specified node perspective
	/// </summary>
	public static bool IsUsedLater(Variable variable, Node perspective)
	{
		// Get a denylist which describes which sections of the node tree have not been executed in the past or won't be executed in the future
		var denylist = GetDenylist(perspective);

		// If any of the references is placed after the specified perspective, the variable is needed
		if (variable.References.Any(i => !denylist.Any(j => i.IsUnder(j)) && i.IsAfter(perspective)))
		{
			return true;
		}

		return perspective.FindParent(i => i.Is(NodeType.LOOP)) != null;
	}

	public static bool OptimizeComparisons(Node root)
	{
		var comparisons = root.FindAll(n => n.Is(NodeType.OPERATOR) && n.To<OperatorNode>().Operator.Type == OperatorType.COMPARISON);
		var precomputed = false;

		foreach (var comparison in comparisons)
		{
			var left = CollectComponents(comparison.First!);
			var right = CollectComponents(comparison.Last!);

			var i = 0;
			var j = 0;

			while (left.Any() && right.Any() && i < left.Count)
			{
				if (j >= right.Count)
				{
					i++;
					j = 0;
					continue;
				}

				var x = left[i];
				var y = right[j];

				if (x is ComplexComponent)
				{
					i++;
					j = 0;
					continue;
				}
				else if (y is ComplexComponent)
				{
					j++;
					continue;
				}

				var s = x - y;

				if (s != null)
				{
					left.RemoveAt(i);
					right.RemoveAt(j);

					left.Insert(i, s);

					j = 0;
				}
				else
				{
					j++;
				}
			}

			if (!left.Any())
			{
				left.Add(new NumberComponent(0L));
			}

			if (!right.Any())
			{
				right.Add(new NumberComponent(0L));
			}

			left = Simplify(left);
			right = Simplify(right);

			comparison.First!.Replace(Recreate(left));
			comparison.Last!.Replace(Recreate(right));

			var evaluation = Preprocessor.TryEvaluateOperator(comparison.To<OperatorNode>());

			if (evaluation != null)
			{
				comparison.Replace(new NumberNode(Parser.Format, (bool)evaluation ? 1L : 0L, comparison.Position));
				precomputed = true;
			}
		}

		return precomputed;
	}

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

		if (!expression.Left.Is(NodeType.NUMBER) && !expression.Right.Is(NodeType.NUMBER))
		{
			return;
		}

		var a = expression.Left is NumberNode x && x.Value.Equals(0L);
		var b = expression.Right is NumberNode y && y.Value.Equals(0L);

		if (a && b)
		{
			expression.Replace(new NumberNode(Parser.Format, 0L, expression.Position));
			return;
		}

		expression.Replace(a ? expression.Right : expression.Left);
	}

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
				root.Replace(new ElseNode(root.Body.Context, root.Body.Clone(), root.Position));
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
				root.Replace(new IfNode(x.Body.Context, x.Condition, x.Body, x.Position));
				x.Remove();
				return true;
			}

			root.ReplaceWithChildren(root.Successor);
			root.Successor.Remove();
		}

		return true;
	}

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
			else if (iterator.Is(NodeType.ELSE))
			{
				iterator = iterator.Next;
			}
			else
			{
				EvaluateConditionalStatements(iterator);
				iterator = iterator.Next;
			}
		}
	}

	public static bool UnwrapStatements(Node root)
	{
		var iterator = root.First;
		var unwrapped = false;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.IF))
			{
				var statement = iterator.To<IfNode>();

				if (statement.Condition.Is(NodeType.NUMBER))
				{
					var successors = statement.GetSuccessors();

					if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
					{
						// Disconnect all the successors
						successors.ForEach(s => s.Remove());

						iterator = statement.Next;

						// Replace the conditional statement with the body
						statement.ReplaceWithChildren(statement.Body);

						unwrapped = true;
					}
					else
					{
						if (statement.Successor == null)
						{
							iterator = statement.Next;
							statement.Remove();
							continue;
						}

						if (statement.Successor.Is(NodeType.ELSE))
						{
							iterator = statement.Successor.Next;

							// Replace the conditional statement with the body of the successor
							statement.ReplaceWithChildren(statement.Successor.To<ElseNode>().Body);

							unwrapped = true;
							continue;
						}

						var successor = statement.Successor.To<ElseIfNode>();

						// Create a conditional statement identical to the successor but as an if-statement
						var replacement = new IfNode();
						successor.ForEach(i => replacement.Add(i));

						iterator = replacement;

						successor.Remove();

						statement.Replace(replacement);

						unwrapped = true;
					}

					continue;
				}
			}
			else if (iterator.Is(NodeType.LOOP))
			{
				var statement = iterator.To<LoopNode>();

				if (!statement.IsForeverLoop)
				{
					if (!statement.Condition.Is(NodeType.NUMBER))
					{
						iterator = statement.Next;

						if (TryUnwrapLoop(statement))
						{
							unwrapped = true;
						}

						continue;
					}

					// NOTE: Here the condition of the loop must be a number node
					// Basically if the number node represents a non-zero value it means the loop should be reconstructed as a forever loop
					if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
					{
						var parent = statement.Parent!;

						parent.Insert(statement, statement.Initialization);

						var replacement = new LoopNode(statement.Context, null, statement.Body, statement.Position);
						parent.Insert(statement, replacement);

						statement.Remove();

						iterator = replacement.Next;
					}
					else
					{
						statement.Replace(statement.Initialization);
						iterator = statement.Initialization;
					}

					unwrapped = true;
					continue;
				}
			}

			iterator = iterator.Next;
		}

		return unwrapped;
	}

	private class LoopUnwrapDescriptor
	{
		public Variable Iterator { get; set; }
		public long Steps { get; set; }
		public List<Component> Start { get; set; }
		public List<Component> Step { get; set; }

		public LoopUnwrapDescriptor(Variable iterator, long steps, List<Component> start, List<Component> step)
		{
			Iterator = iterator;
			Steps = steps;
			Start = start;
			Step = step;
		}
	}

	private static LoopUnwrapDescriptor? TryGetLoopUnwrapDescriptor(LoopNode loop)
	{
		// First, ensure that the condition contains a comparison operator and that it is primitive.
		// Examples:
		// i < 10
		// i == 0
		// 0 < 10 * a + 10 - x

		// Ensure there is only one condition present
		var condition = loop.Condition;

		if (loop.GetConditionInitialization().Any())
		{
			return null;
		}

		if (!condition.Is(NodeType.OPERATOR) ||
			condition.To<OperatorNode>().Operator.Type != OperatorType.COMPARISON ||
			!IsPrimitive(condition))
		{
			return null;
		}

		// Ensure that the initialization is empty or it contains a definition of an integer variable
		var initialization = loop.Initialization;

		if (initialization.IsEmpty || initialization.First != initialization.Last)
		{
			return null;
		}

		initialization = initialization.First!;

		if (!initialization.Is(Operators.ASSIGN) || !initialization.First!.Is(NodeType.VARIABLE))
		{
			return null;
		}

		// Make sure the variable is predictable and it is an integer
		var variable = initialization.First!.To<VariableNode>().Variable;

		if (!variable.IsPredictable ||
			!(initialization.First.To<VariableNode>().Variable.Type is Number) ||
			!initialization.Last!.Is(NodeType.NUMBER))
		{
			return null;
		}

		var start_value = initialization.Last.To<NumberNode>().Value;

		// Ensure there is only one action present
		var action = loop.Action;

		if (action.IsEmpty || action.First != action.Last)
		{
			return null;
		}

		action = action.First!;

		var step_value = new List<Component>();

		if (action.Is(NodeType.INCREMENT))
		{
			var statement = action.To<IncrementNode>();

			if (!statement.Object.Is(variable))
			{
				return null;
			}

			step_value.Add(new NumberComponent(1L));
		}
		else if (action.Is(NodeType.DECREMENT))
		{
			var statement = action.To<IncrementNode>();

			if (!statement.Object.Is(variable))
			{
				return null;
			}

			step_value.Add(new NumberComponent(-1L));
		}
		else if (action.Is(NodeType.OPERATOR))
		{
			var statement = action.To<OperatorNode>();

			if (!statement.Left.Is(variable))
			{
				return null;
			}

			if (statement.Operator == Operators.ASSIGN)
			{
				statement = ReconstructionAnalysis.TryRewriteAsActionOperation(statement);

				if (statement == null)
				{
					return null;
				}
			}

			if (statement.Operator == Operators.ASSIGN_ADD)
			{
				step_value = CollectComponents(statement.Right);
			}
			else if (statement.Operator == Operators.ASSIGN_SUBTRACT)
			{
				step_value = Negate(CollectComponents(statement.Right));
			}
			else
			{
				return null;
			}
		}
		else
		{
			return null;
		}

		// Try to rewrite the condition so that the initialized variable is on the left side of the comparison
		// Example:
		// 0 < 10 * a + 10 - x => x < 10 * a + 10
		var left = CollectComponents(condition.First!);

		// Abort the optimization if the comparison contains complex variable components
		// Examples (x is the iterator variable):
		// x^2 < 10
		// x < ax + 10
		if (left.Exists(c => c is VariableComponent x && x.Variable == variable && x.Order != 1 ||
			c is VariableProductComponent y && y.Variables.Exists(i => i.Variable == variable)))
		{
			return null;
		}

		var right = CollectComponents(condition.Last!);

		if (right.Exists(c => c is VariableComponent x && x.Variable == variable && x.Order != 1 ||
			c is VariableProductComponent y && y.Variables.Exists(i => i.Variable == variable)))
		{
			return null;
		}

		// Ensure that the condition contains atleast one initialization variable
		if (!left.Concat(right).Any(c => c is VariableComponent x && x.Variable == variable))
		{
			return null;
		}

		// Move all other than initialization variables to the right hand side
		for (var i = left.Count - 1; i >= 0; i--)
		{
			var x = left[i];

			if (x is VariableComponent a && a.Variable == variable)
			{
				continue;
			}

			x.Negate();

			right.Add(x);
			left.RemoveAt(i);
		}

		// Move all initialization variables to the left hand side
		for (var i = right.Count - 1; i >= 0; i--)
		{
			var x = right[i];

			if (x is not VariableComponent a || a.Variable != variable)
			{
				continue;
			}

			x.Negate();

			left.Add(x);
			right.RemoveAt(i);
		}

		// Substract the starting value from the right hand side of the condition
		var range = SimplifySubtraction(right, new List<Component> { new NumberComponent(start_value) });
		var result = SimplifyDivision(range, step_value);

		if (result != null)
		{
			if (result.Count != 1)
			{
				return null;
			}

			if (result.First() is NumberComponent steps)
			{
				if (steps.Value is double)
				{
					Console.WriteLine("Loop can not be unwrapped since the amount of steps is expressed in decimals?");
					return null;
				}

				return new LoopUnwrapDescriptor(variable, (long)steps.Value, new List<Component> { new NumberComponent(start_value) }, step_value);
			}

			// If the amount of steps is not a constant, it means the length of the loop varies, therefore the loop can not be unwrapped
			return null;
		}

		Console.WriteLine("Encountered possible complex loop increment value division, please implement");
		return null;
	}

	public static bool TryUnwrapLoop(LoopNode loop)
	{
		var descriptor = TryGetLoopUnwrapDescriptor(loop);

		if (descriptor == null || descriptor.Steps > 100)
		{
			return false;
		}

		loop.InsertChildren(loop.Initialization.Clone());

		var action = ReconstructionAnalysis.TryRewriteAsAssignOperation(loop.Action.First!) ?? loop.Action.First!.Clone();

		for (var i = 0; i < descriptor.Steps; i++)
		{
			loop.InsertChildren(loop.Body.Clone());
			loop.Insert(action.Clone());
		}

		loop.Remove();

		return true;
	}

	public static bool RemoveUnreachableStatements(Node root)
	{
		var return_statements = root.FindAll(n => n.Is(NodeType.RETURN));
		var removed = false;

		for (var i = return_statements.Count - 1; i >= 0; i--)
		{
			var return_statement = return_statements[i];

			// Remove all statements which are after the return statement in its scope
			var iterator = return_statement.Parent!.Last;

			while (iterator != return_statement)
			{
				var previous = iterator!.Previous;
				iterator.Remove();
				iterator = previous;
				removed = true;
			}
		}

		return removed;
	}

	public static long GetCost(Node node)
	{
		var result = 0L;
		var iterator = node.First;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.OPERATOR))
			{
				var operation = iterator.To<OperatorNode>().Operator;

				if (operation == Operators.ADD || operation == Operators.SUBTRACT)
				{
					result += 2;
				}
				else if (operation == Operators.MULTIPLY)
				{
					result += 10;
				}
				else if (operation == Operators.DIVIDE)
				{
					result += 70;
				}
				else if (operation.Type == OperatorType.COMPARISON)
				{
					result++;
				}
				else if (operation.Type == OperatorType.ACTION)
				{
					result++;
				}
			}
			else if (iterator.Is(NodeType.LINK, NodeType.OFFSET))
			{
				result += 10;
			}
			else if (iterator.Is(NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE))
			{
				result += 50;
			}
			else if (iterator.Is(NodeType.LOOP))
			{
				result += 100;
			}

			result += GetCost(iterator);

			iterator = iterator.Next;
		}

		return result;
	}

	/// <summary>
	/// Tries to optimize all expressions in the specified node tree
	/// </summary>
	public static void OptimizeAllExpressions(Node root)
	{
		// Find all top level operators
		var expressions = new List<Node>();

		foreach (var operation in FindTopLevelOperators(root))
		{
			if (operation.Operator.Type == OperatorType.ACTION)
			{
				expressions.Add(operation.Last!);
			}
			else
			{
				expressions.Add(operation);
			}
		}

		foreach (var expression in expressions)
		{
			// Replace the expression with a simplified version
			expression.Replace(GetSimplifiedValue(expression));
		}
	}

	/// <summary>
	/// Returns all operator nodes which are first encounter when decending from the specified node
	/// </summary>
	private static List<OperatorNode> FindTopLevelOperators(Node node)
	{
		if (node.Is(NodeType.OPERATOR) && node.To<OperatorNode>().Operator.Type == OperatorType.CLASSIC)
		{
			return new List<OperatorNode> { (OperatorNode)node };
		}

		var operators = new List<OperatorNode>();
		var child = node.First;

		while (child != null)
		{
			if (child.Is(NodeType.OPERATOR))
			{
				var operation = child.To<OperatorNode>();

				if (operation.Operator.Type != OperatorType.CLASSIC)
				{
					operators.AddRange(FindTopLevelOperators(operation.Left));
					operators.AddRange(FindTopLevelOperators(operation.Right));
				}
				else
				{
					operators.Add(operation);
				}
			}
			else
			{
				operators.AddRange(FindTopLevelOperators(child));
			}

			child = child.Next;
		}

		return operators;
	}

	/// <summary>
	/// Adds logic for allocating the instance, registering virtual functions and initializing member variables to the constructors of the specified type
	/// </summary>
	private static void CompleteConstructors(Type type)
	{
		if (type.Configuration == null)
		{
			return;
		}

		var expressions = new List<OperatorNode>(type.Initialization);

		foreach (var constructor in type.Constructors.Overloads.SelectMany(i => i.Implementations).Where(i => i.Node != null))
		{
			var self = Common.GetSelfPointer(constructor, null);

			foreach (var iterator in expressions)
			{
				var expression = iterator.Clone().To<OperatorNode>();
				var members = expression.FindAll(i => i.Is(NodeType.VARIABLE) && i.To<VariableNode>().Variable.IsMember && !i.Parent!.Is(NodeType.LINK));

				foreach (var member in members)
				{
					member.Replace(new LinkNode(self.Clone(), member.Clone()));
				}

				var position = constructor.Node!.First;

				if (position == null)
				{
					constructor.Node!.Add(expression);
				}
				else
				{
					constructor.Node!.Insert(position, expression);
				}
			}

			foreach (var return_statement in constructor.Node!.FindAll(i => i.Is(NodeType.RETURN)))
			{
				return_statement.Replace(new ReturnNode(self.Clone(), return_statement.Position));
			}

			constructor.Node!.Add(new ReturnNode(self));
		}
	}

	/// <summary>
	/// Evaluates the values of size nodes
	/// </summary>
	private static void CompleteSizes(Node root)
	{
		var sizes = root.FindAll(i => i.Is(NodeType.SIZE)).Cast<SizeNode>();

		foreach (var size in sizes)
		{
			size.Replace(new NumberNode(Parser.Format, (long)size.Type.ReferenceSize));
		}
	}

	/// <summary>
	/// Adds logic for allocating the instance, registering virtual functions and initializing member variables to the constructors of the specified type
	/// </summary>
	public static void Complete(Context context)
	{
		foreach (var type in context.Types.Values)
		{
			Complete(type);
			CompleteConstructors(type);
		}

		foreach (var implementation in context.GetImplementedFunctions())
		{
			Complete(implementation);
			CompleteSizes(implementation.Node!);
		}
	}

	public static void CaptureContextLeaks(Context context, Node root)
	{
		var variables = root.FindAll(i => i.Is(NodeType.VARIABLE)).Cast<VariableNode>().Where(i => !i.Variable.IsConstant && i.Variable.IsPredictable && !i.Variable.Context.IsInside(context));

		// Try to find variables whose parent context is not defined inside the implementation, if even one is found it means something has leaked
		if (variables.Any())
		{
			throw new ApplicationException("Found a context leak");
		}

		var declarations =root.FindAll(i => i.Is(NodeType.DECLARE)).Cast<DeclareNode>().Where(i => !i.Variable.IsConstant && i.Variable.IsPredictable && !i.Variable.Context.IsInside(context));

		// Try to find declaration nodes whose variable is not defined inside the implementation, if even one is found it means something has leaked
		if (declarations.Any())
		{
			throw new ApplicationException("Found a context leak");
		}

		var subcontexts = root.FindAll(i => i is IContext && !i.Is(NodeType.TYPE)).Cast<IContext>().Where(i => !i.GetContext().IsInside(context));

		// Try to find declaration nodes whose variable is not defined inside the implementation, if even one is found it means something has leaked
		if (subcontexts.Any())
		{
			throw new ApplicationException("Found a context leak");
		}
	}
	
	/// <summary>
	/// Collects all types and subtypes from the specified context
	/// </summary>
	public static List<Type> GetAllTypes(Context context)
	{
		var result = context.Types.Values.ToList();
		result.AddRange(result.SelectMany(i => GetAllTypes(i)));

		return result;
	}

	/// <summary>
	/// Collects all function implementations from the specified context
	/// </summary>
	public static FunctionImplementation[] GetAllFunctionImplementations(Context context)
	{
		var types = GetAllTypes(context);
		
		// Collect all functions, constructors, destructors and virtual functions
		var type_functions = types.SelectMany(i => i.Functions.Values.SelectMany(j => j.Overloads));
		var type_constructors = types.SelectMany(i => i.Constructors.Overloads);
		var type_destructors = types.SelectMany(i => i.Destructors.Overloads);
		var type_virtual_functions = types.SelectMany(i => i.Virtuals.Values.SelectMany(j => j.Overloads));
		var context_functions = context.Functions.Values.SelectMany(i => i.Overloads);

		var implementations = type_functions.Concat(type_constructors).Concat(type_destructors).Concat(type_virtual_functions).Concat(context_functions).SelectMany(i => i.Implementations).ToArray();

		// Concat all functions with lambdas, which can be found inside the collected functions
		return implementations.Concat(implementations.SelectMany(i => GetAllFunctionImplementations(i)))
			.Distinct(new HashlessReferenceEqualityComparer<FunctionImplementation>()).ToArray();
	}

	public static void Analyze(Context context)
	{
		var implementations = GetAllFunctionImplementations(context).OrderByDescending(i => i.References.Count).ToList();

		for (var i = 0; i < implementations.Count; i++)
		{
			var implementation = implementations[i];

			ReconstructionAnalysis.Reconstruct(implementation.Node!);

			CaptureContextLeaks(implementation, implementation.Node!);

			implementation.Node = GeneralAnalysis.Optimize(implementation, implementation.Node!);

			ReconstructionAnalysis.Finish(implementation.Node!);

			if (implementation is LambdaImplementation lambda)
			{
				lambda.Seal();
			}

			//Console.WriteLine($"Analysis {i}/{implementations.Count}");
		}
	}

	public static void Evaluate(Node root)
	{
		var expressions = root.FindAll(i => i.Is(NodeType.COMPILES));

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

	public static void Evaluate(Context context)
	{
		foreach (var type in context.Types.Values)
		{
			Evaluate(type);
		}

		foreach (var implementation in context.GetImplementedFunctions())
		{
			// Should evaluate as long as the node tree changes
			Evaluate(implementation.Node!);
			EvaluateLogicalOperators(implementation.Node!);
			EvaluateConditionalStatements(implementation.Node!);
			Evaluate(implementation);
		}
	}

	private static Node ReplaceRepetition(Node repetition, Variable variable, bool store = false)
	{
		if (Analyzer.IsEdited(repetition))
		{
			var edit = Analyzer.GetEditor(repetition);

			if (edit.Is(Operators.ASSIGN))
			{
				// Store the value of the assignment to the specified variable
				var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(variable),
					edit.Right
				);

				var inline = new InlineNode(edit.Position) { initialization };
				
				edit.Replace(inline);

				// Store the value into the repetition
				inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
					repetition.Clone(),
					new VariableNode(variable)
				));

				// Add a result to the inline node if the return value of the edit is used
				if (ReconstructionAnalysis.IsValueUsed(edit))
				{
					inline.Add(new VariableNode(variable));
				}

				return inline;
			}

			// Increments, decrements and special assignment operators should be unwrapped before unrepetition
			throw new ApplicationException("Repetition was edited by increment, decrement or special assignment operator which should no happen");
		}
		
		if (store)
		{
			// Store the value of the repetition to the specified variable
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(variable),
				repetition.Clone()
			);
			
			// Replace the repetition with the initialization
			var inline = new InlineNode(repetition.Position) { initialization, new VariableNode(variable) };
			repetition.Replace(inline);

			return inline;
		}

		var result = new VariableNode(variable);
		repetition.Replace(result);

		return result;
	}

	private static List<Node> FindTop(Node root, Predicate<Node> filter)
	{
		var nodes = new List<Node>();
		var iterator = (IEnumerable<Node>)root;

		if (root is OperatorNode x && x.Operator.Type == OperatorType.ACTION)
		{
			iterator = iterator.Reverse();
		}

		foreach (var i in iterator)
		{
			if (filter(i))
			{
				nodes.Add(i);
			}
			else
			{
				nodes.AddRange(FindTop(i, filter));
			}
		}

		return nodes;
	}

	private static List<Node> GetEditables(Node node)
	{
		var result = new List<Node>();

		foreach (var iterator in node)
		{
			if (iterator is LinkNode link)
			{
				result.Add(link);
				result.AddRange(GetEditables(link));
			}
			else if (iterator != null)
			{
				var editables = GetEditables(iterator);

				if (editables.Any())
				{
					result.AddRange(editables);
					result.AddRange(editables.SelectMany(i => GetEditables(i)));
				}
				else
				{
					result.Add(node.GetBottomLeft()!);
				}
			}
		}

		return result;
	}

	public static void Unrepeat(Node root)
	{
		var filtered = new Queue<Node>();
		var links = FindTop(root, i => i.Is(NodeType.LINK, NodeType.OFFSET));

		Start:
		var flow = new Flow(root);

		if (!links.Any())
		{
			return;
		}

		var repetitions = new List<Node>();
		var start = links.First();

		// Collect all parts of the start node which can be edited
		var dependencies = GetEditables(start);

		for (var j = links.Count - 1; j >= 1; j--)
		{
			var other = links[j];

			if (other.Any(i => !i.Is(NodeType.VARIABLE, NodeType.TYPE, NodeType.LINK, NodeType.OFFSET, NodeType.CONTENT)))
			{
				links.RemoveAt(j);
				continue;
			}

			if (!start.Equals(other))
			{
				var sublinks = other.FindAll(i => i.Is(NodeType.LINK)).Cast<LinkNode>();

				foreach (var sublink in sublinks)
				{
					if (sublink.Equals(start))
					{
						repetitions.Insert(0, sublink);
					}
				}

				continue;
			}

			repetitions.Insert(0, other);
		}

		// The current is processed, so remove it now
		links.RemoveAt(0);

		if (!repetitions.Any())
		{
			// Find inner links inside the current one and process them now
			var inner = FindTop(start, i => i.Is(NodeType.LINK, NodeType.OFFSET));
			links.InsertRange(0, inner);

			goto Start;
		}

		repetitions.ForEach(i => links.Remove(i));

		var context = start.FindParent(i => i.Is(NodeType.IMPLEMENTATION))!.To<ImplementationNode>().Context;
		var variable = context.DeclareHidden(start.GetType());

		// Initialize the variable
		var scope = ReconstructionAnalysis.GetSharedScope(repetitions.Concat(new[] { start }).ToArray());

		if (scope == null)
		{
			throw new ApplicationException("Repetitions did not have a shared scope");
		}

		// Since the repetitions are ordered find the insert position using the first repetition and the shared scope
		ReconstructionAnalysis.GetInsertPosition(start, scope).Insert(new DeclareNode(variable));
		
		ReplaceRepetition(start, variable, true);

		foreach (var repetition in repetitions)
		{
			var store = false;

			// Find all edits between the start and the repetition
			var edits = flow.FindBetween(start, repetition, i => i.Is(OperatorType.ACTION) || i.Is(NodeType.INCREMENT, NodeType.DECREMENT));

			// If any of the edits contain a destination which matches any of the dependencies, a store is required
			foreach (var edit in edits)
			{
				var edited = Analyzer.GetEdited(edit);

				if (!dependencies.Contains(edited))
				{
					continue;
				}

				start = repetition;
				store = true;
				break;
			}

			// 1. If there are function calls between the start and the repetition, the function calls could edit the repetition, so a store is required
			// 2. If the start is not always executed before the repetition, a store is needed
			if (flow.Between(start, repetition, i => i.Is(NodeType.FUNCTION, NodeType.CALL)))
			{
				start = repetition;
				store = true;
			}
			else if (!flow.IsExecutedBefore(start, repetition))
			{
				start = repetition;
				store = true;
			}

			ReplaceRepetition(repetition, variable, store);
		}

		goto Start;
	}

	// TODO: Remove redundant casts and add necessary casts
	// TODO: Remove exploits such as: !(!(!(x)))
}