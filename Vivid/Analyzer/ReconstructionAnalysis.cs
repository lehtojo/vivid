using System.Collections.Generic;
using System.Linq;
using System;

public static class ReconstructionAnalysis
{
	public static Node? TryRewriteAsActionOperation(Node edit, bool force = false)
	{
		if (!edit.Is(NodeType.INCREMENT, NodeType.DECREMENT) || (!force && IsValueUsed(edit)))
		{
			return null;
		}

		if (edit is IncrementNode increment)
		{
			var destination = increment.Object.Clone();
			var type = destination.TryGetType() ?? throw new ApplicationException("Could not retrieve type from increment node");

			return new OperatorNode(Operators.ASSIGN_ADD, increment.Position).SetOperands(
				destination,
				new NumberNode(type.Format, 1L, increment.Position)
			);
		}

		if (edit is DecrementNode decrement)
		{
			var destination = decrement.Object.Clone();
			var type = destination.TryGetType() ?? throw new ApplicationException("Could not retrieve type from decrement node");

			return new OperatorNode(Operators.ASSIGN_SUBTRACT, decrement.Position).SetOperands(
				destination,
				new NumberNode(type.Format, 1L, decrement.Position)
			);
		}

		return null;
	}

	public static Node? TryRewriteAsAssignOperation(Node edit, bool force = false)
	{
		if (!force && IsValueUsed(edit))
		{
			return null;
		}

		switch (edit)
		{
			case IncrementNode increment:
			{
				var destination = increment.Object.Clone();

				return new OperatorNode(Operators.ASSIGN, increment.Position).SetOperands(
					destination,
					new OperatorNode(Operators.ADD, increment.Position).SetOperands(
						destination.Clone(),
						new NumberNode(Parser.Format, 1L, increment.Position)
					)
				);
			}

			case DecrementNode decrement:
			{
				var destination = decrement.Object.Clone();

				return new OperatorNode(Operators.ASSIGN, decrement.Position).SetOperands(
					destination,
					new OperatorNode(Operators.SUBTRACT, decrement.Position).SetOperands(
						destination.Clone(),
						new NumberNode(Parser.Format, 1L, decrement.Position)
					)
				);
			}

			case OperatorNode operation:
			{
				if (operation.Operator.Type != OperatorType.ACTION)
				{
					return null;
				}

				if (operation.Operator == Operators.ASSIGN)
				{
					return edit;
				}

				var destination = operation.Left.Clone();
				var type = ((ActionOperator)operation.Operator).Operator;

				if (type == null)
				{
					return null;
				}

				return new OperatorNode(Operators.ASSIGN, operation.Position).SetOperands(
					destination,
					new OperatorNode(type, operation.Position).SetOperands(
						destination.Clone(),
						edit.Last!.Clone()
					)
				);
			}
		}

		return null;
	}

	/// <summary>
	/// Removes redundant parenthesis in the specified node tree
	/// Example: x = x * (((x + 1)))
	/// </summary>
	private static void RemoveRedundantParenthesis(Node root)
	{
		if (root.Is(NodeType.CONTENT) || root.Is(NodeType.LIST))
		{
			foreach (var iterator in root)
			{
				if (iterator is ContentNode parenthesis && parenthesis.Count() == 1)
				{
					iterator.Replace(iterator.First!);
				}
			}

			// Remove all parenthesis which block logical operators
			if (root.First is OperatorNode x && x.Operator.Type == OperatorType.LOGIC)
			{
				root.Replace(root.First!);
			}
		}

		root.ForEach(RemoveRedundantParenthesis);
	}

	/// <summary>
	/// Removes negation nodes which cancel each other out.
	/// Example: x = -(-a)
	/// </summary>
	private static void RemoveCancellingNegations(Node root)
	{
		if (root is NegateNode x && x.Object is NegateNode y)
		{
			root.Replace(y.Object);
			RemoveCancellingNegations(y.Object);
			return;
		}

		foreach (var iterator in root)
		{
			RemoveCancellingNegations(iterator);
		}
	}

	/// <summary>
	/// Removes not nodes which cancel each other out.
	/// Example: x = !!a
	/// </summary>
	private static void RemoveCancellingNots(Node root)
	{
		if (root is NotNode x && x.Object is NotNode y)
		{
			root.Replace(y.Object);
			RemoveCancellingNegations(y.Object);
			return;
		}

		foreach (var iterator in root)
		{
			RemoveCancellingNots(iterator);
		}
	}

	/// <summary>
	/// Creates a condition which passes if the source has the same type as the specified type in runtime
	/// </summary>
	private static Node CreateTypeCondition(Node source, Type expected)
	{
		var type = source.GetType();
	
		if (type.Configuration == null || expected.Configuration == null)
		{
			// If the configuration of the type is not present, it means that the type can not be inherited
			// Since the type can not be inherited, this means the result of the condition can be determined

			return new NumberNode(Parser.Format, type == expected ? 1L : 0L);
		}

		var configuration = type.GetConfigurationVariable();
		var start = new LinkNode(source, new VariableNode(configuration));

		var a = new OperatorNode(Operators.EQUALS).SetOperands(
			OffsetNode.CreateConstantOffset(start, 0, 1, Parser.Format),
			new DataPointer(expected.Configuration.Descriptor)
		);

		var b = new FunctionNode(Parser.InheritanceFunction!).SetParameters(new Node {
			OffsetNode.CreateConstantOffset(start.Clone(), 0, 1, Parser.Format),
			new DataPointer(expected.Configuration.Descriptor)
		});

		return new OperatorNode(Operators.OR).SetOperands(a, b);
	}

	/// <summary>
	/// Rewrites is expressions so that they use logic that can be compiled
	/// </summary>
	private static void RewriteIsExpressions(Node root)
	{
		var expressions = root.FindAll(i => i.Is(NodeType.IS)).Cast<IsNode>().ToList();

		for (var i = expressions.Count - 1; i >= 0; i--)
		{
			var expression = expressions[i];

			if (expression.HasResultVariable)
			{
				continue;
			}

			expression.Replace(CreateTypeCondition(expression.Object, expression.Type));
			expressions.RemoveAt(i);
		}

		foreach (var expression in expressions)
		{
			// Initialize the result variable
			var initialization = (Node)new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(expression.Result),
				new NumberNode(Parser.Format, 0L)
			);

			// The result variable must be initialized outside the condition
			var scope = (Node)expression.FindContext();
			scope.Insert(scope.First, initialization);

			// Get the context of the expression
			var expression_context = expression.GetParentContext();

			// Declare a variable which is used to store the inspected object
			var object_type = expression.Object.GetType();
			var object_variable = expression_context.DeclareHidden(object_type);

			// Object variable should be declared
			initialization = new DeclareNode(object_variable);

			scope.Insert(scope.First, initialization);

			// Load the inspected object
			var load = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(object_variable),
				expression.Object
			);

			var assignment_context = new Context();
			assignment_context.Link(expression_context);

			// Create a condition which passes if the inspected object is the expected type
			var condition = CreateTypeCondition(new VariableNode(object_variable), expression.Type);

			// Create an assignment which assigns the inspected object to the result variable while casting it to the expected type
			var assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(expression.Result),
				new CastNode(new VariableNode(object_variable), new TypeNode(expression.Type))
			);

			var conditional_assignment = new IfNode(assignment_context, condition, new Node { assignment });

			// Create a condition which represents the result of the is expression
			var result_condition = new VariableNode(expression.Result);

			// Replace the expression with the logic above
			expression.Replace(new InlineNode(expression.Position) { load, conditional_assignment, result_condition });
		}
	}

	/// <summary>
	/// Returns a node representing a position where new nodes can be inserted
	/// </summary>
	private static Node GetInsertPosition(Node reference)
	{
		var iterator = reference.Parent!;
		var position = reference;

		while (!(iterator is IContext || iterator.Is(NodeType.NORMAL)))
		{
			position = iterator;
			iterator = iterator.Parent!;
		}

		return position;
	}

	private static List<OperatorNode> FindBooleanValues(Node root)
	{
		var candidates = root.FindAll(i => i.Is(NodeType.OPERATOR) && (i.To<OperatorNode>().Operator.Type == OperatorType.COMPARISON || i.To<OperatorNode>().Operator.Type == OperatorType.LOGIC)).Cast<OperatorNode>();

		return candidates.Where(candidate =>
		{
			var parent = candidate.FindParent(i => !i.Is(NodeType.CONTENT))!;

			if (!parent.Is(
				NodeType.CAST,
				NodeType.DECREMENT,
				NodeType.ELSE_IF,
				NodeType.ELSE,
				NodeType.FUNCTION,
				NodeType.INCREMENT,
				NodeType.LINK,
				NodeType.LOOP,
				NodeType.NEGATE,
				NodeType.NOT,
				NodeType.OFFSET,
				NodeType.RETURN
			))
			{
				return false;
			}

			return !(parent is OperatorNode operation && (operation.Operator.Type == OperatorType.COMPARISON || operation.Operator.Type == OperatorType.LOGIC));

		}).ToList();
	}

	private static void OutlineBooleanValues(Node root)
	{
		var instances = FindBooleanValues(root);

		foreach (var instance in instances)
		{
			//var position = GetInsertPosition(instance);

			// Declare a hidden variable which represents the result
			var environment = instance.FindContext()?.GetContext() ?? throw new ApplicationException("Could not find the current context");
			var destination = environment.DeclareHidden(Types.BOOL);

			// Initialize the result with value 'false'
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			   new VariableNode(destination),
			   new NumberNode(Assembler.Format, 0L)
			);

			// Replace the operation with the result
			var inline = new InlineNode(instance.Position);
			instance.Replace(inline);
			//var replacement = new VariableNode(destination);

			var context = new Context();
			context.Link(environment);

			// The destination is edited inside the following statement
			var assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
			   new VariableNode(destination),
			   new NumberNode(Assembler.Format, 1L)
			);

			destination.Edits.Add(assignment);

			// Create a conditional statement which sets the value of the destination variable to true if the condition is true
			var statement = new IfNode(context, instance, new Node { assignment });

			// Add the statements which implement the boolean value
			inline.Add(initialization);
			inline.Add(statement);
			inline.Add(new VariableNode(destination));
			//position.Insert(statement);
			//statement.Insert(initialization);
		}
	}

	private static bool IsValueUsed(Node value)
	{
		return value.Parent!.Is(
			NodeType.CAST,
			NodeType.CONTENT,
			NodeType.DECREMENT,
			NodeType.FUNCTION,
			NodeType.INCREMENT,
			NodeType.LINK,
			NodeType.NEGATE,
			NodeType.NOT,
			NodeType.OFFSET,
			NodeType.OPERATOR,
			NodeType.RETURN
		);
	}

	/// <summary>
	/// Rewrites increment and decrement operators as action operations if their values are discard.
	/// Example (value is not discarded):
	/// x = ++i
	/// Before (value is discarded):
	/// loop (i = 0, i < n, i++)
	/// After:
	/// loop (i = 0, i < n, i += 1)
	/// </summary>
	private static void RewriteDiscardedIncrements(Node root)
	{
		var increments = root.FindAll(i => i.Is(NodeType.INCREMENT, NodeType.DECREMENT));

		foreach (var increment in increments)
		{
			if (IsValueUsed(increment))
			{
				continue;
			}

			var replacement = TryRewriteAsActionOperation(increment) ?? throw new ApplicationException("Could not rewrite increment operation as assign operation");

			increment.Replace(replacement);
		}
	}

	private static void RewriteAllEditsAsAssignOperations(Node root)
	{
		var edits = root.FindAll(i => i.Is(NodeType.INCREMENT, NodeType.DECREMENT) || (i is OperatorNode x && x.Operator.Type == OperatorType.ACTION));

		foreach (var edit in edits)
		{
			var replacement = TryRewriteAsAssignOperation(edit);

			if (replacement == null)
			{
				throw new ApplicationException("Could not unwrap an edit node");
			}

			edit.Replace(replacement);
		}
	}

	/// <summary>
	/// Rewrites increments and decrements as inline nodes
	/// Examples:
	/// a = i++ => a = { x = i, i = i + 1, x }
	/// a = ++i => a = { i = i + 1, i }
	/// </summary>
	private static void RewriteIncrements(Node root)
	{
		var expressions = root.FindAll(i => i.Is(NodeType.INCREMENT, NodeType.DECREMENT));

		foreach (var expression in expressions)
		{
			var inline = new ContextInlineNode(expression.GetParentContext(), expression.Position);

			var assignment = TryRewriteAsAssignOperation(expression, true);

			if (assignment == null)
			{
				throw new ApplicationException("Could not rewrite increment or decrement as an assign operation");
			}

			var post = expression is IncrementNode x ? x.Post : expression.To<DecrementNode>().Post;
			var value = expression is IncrementNode y ? y.Object : expression.To<DecrementNode>().Object;

			if (post)
			{
				var temporary = inline.Context.DeclareHidden(expression.GetType());

				inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(temporary),
					value
				));

				inline.Add(assignment);
				inline.Add(new VariableNode(temporary));
			}
			else
			{
				inline.Add(assignment);
				inline.Add(value);
			}

			expression.Replace(inline);
		}
	}

	/// <summary>
	/// Tries to find assign operations which can be written as action operations
	/// Examples: 
	/// i = i + 1 => i += 1
	/// x = 2 * x => x *= 2
	/// this.a = this.a % 2 => this.a %= 2
	/// </summary>
	private static void ConstructActionOperations(Node root)
	{
		var assignments = root.FindTop(i => i.Is(Operators.ASSIGN)).Cast<OperatorNode>();

		foreach (var assignment in assignments)
		{
			if (assignment.Right is OperatorNode operation)
			{
				var value = (Node?)null;

				// Ensure either the left or the right operand is the same as the destination of the assignment
				if (operation.Left.Equals(assignment.Left))
				{
					value = operation.Right;
				}
				else if (operation.Right.Equals(assignment.Left) && 
					operation.Operator != Operators.DIVIDE && 
					operation.Operator != Operators.MODULUS &&
					operation.Operator != Operators.SUBTRACT)
				{
					value = operation.Left;
				}

				if (value == null)
				{
					continue;
				}

				var action_operator = Operators.GetActionOperator(operation.Operator);

				if (action_operator == null)
				{
					continue;
				}

				assignment.Replace(new OperatorNode(action_operator, assignment.Position).SetOperands(assignment.Left, value));
			}
		}
	}

	/// <summary>
	/// Finds all inline nodes which have only one child node and replaces them with their child nodes
	/// </summary>
	public static void SubstituteInlineNodes(Node root)
	{
		var inlines = root.FindAll(i => i.Is(NodeType.INLINE));

		foreach (var inline in inlines)
		{
			// If the inline node contains only one child node, the inline node can be replaced with it
			if (inline.Count() == 1)
			{
				inline.Replace(inline.First!);
			}
		}
	}

	public static void Reconstruct(Node root)
	{
		RemoveRedundantParenthesis(root);
		RemoveCancellingNegations(root);
		RemoveCancellingNots(root);
		RewriteIsExpressions(root);
		OutlineBooleanValues(root);
		RewriteDiscardedIncrements(root);
		RewriteIncrements(root);
		RewriteAllEditsAsAssignOperations(root);
		SubstituteInlineNodes(root);
	}

	public static void Finish(Node root)
	{
		ConstructActionOperations(root);
		SubstituteInlineNodes(root);
	}
}