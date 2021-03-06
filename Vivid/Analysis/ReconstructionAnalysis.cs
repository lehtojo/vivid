using System;
using System.Collections.Generic;
using System.Linq;

public static class ReconstructionAnalysis
{
	/// <summary>
	/// Returns the first position where a statement can be placed outside the scope of the specified node
	/// </summary>
	public static Node GetInsertPosition(Node reference)
	{
		var iterator = reference.Parent!;
		var position = reference;

		while (!iterator.Is(NodeType.SCOPE))
		{
			position = iterator;
			iterator = iterator.Parent!;
		}

		// If the position happens to become a conditional statement, the insert position should become before it
		if (position.Is(NodeType.ELSE_IF))
		{
			position = position.To<ElseIfNode>().GetRoot();
		}
		else if (position.Is(NodeType.ELSE))
		{
			position = position.To<ElseNode>().GetRoot();
		}

		return position;
	}

	/// <summary>
	/// Returns the first position where a statement can be placed outside the scope of the specified node
	/// </summary>
	public static Node GetInsertPosition(Node reference, Node scope)
	{
		var iterator = reference.Parent!;
		var position = reference;

		while (iterator != scope)
		{
			position = iterator;
			iterator = iterator.Parent!;
		}

		// If the position happens to become a conditional statement, the insert position should become before it
		if (position.Is(NodeType.ELSE_IF))
		{
			position = position.To<ElseIfNode>().GetRoot();
		}
		else if (position.Is(NodeType.ELSE))
		{
			position = position.To<ElseNode>().GetRoot();
		}

		return position;
	}

	/// <summary>
	/// Finds the most inner scope that covers all of the specified nodes
	/// </summary>
	public static Node? GetSharedScope(Node[] nodes)
	{
		if (!nodes.Any())
		{
			return null;
		}

		if (nodes.Length == 1)
		{
			return nodes.First().FindParent(i => i.Is(NodeType.SCOPE));
		}

		var shared = nodes[0];

		for (var i = 1; i < nodes.Length; i++)
		{
			shared = Node.GetSharedNode(shared, nodes[i], false);

			if (shared == null)
			{
				return null;
			}
		}

		return shared.Is(NodeType.SCOPE) ? shared : shared.FindParent(i => i.Is(NodeType.SCOPE));
	}

	/// <summary>
	/// Tries to build an action operator out of the specified edit
	/// Examples:
	/// x++ => x += 1
	/// x-- => x -= 1
	/// a = 2 * a => a *= 2
	/// x = x / y => x /= y
	/// </summary>
	public static OperatorNode? TryRewriteAsActionOperation(Node edit, bool force = false)
	{
		if (!edit.Is(NodeType.INCREMENT, NodeType.DECREMENT, NodeType.OPERATOR) || (!force && IsValueUsed(edit)))
		{
			return null;
		}

		if (edit is IncrementNode increment)
		{
			var destination = increment.Object.Clone();
			var type = destination.GetType();

			return new OperatorNode(Operators.ASSIGN_ADD, increment.Position).SetOperands(
				destination,
				new NumberNode(type.Format, 1L, increment.Position)
			);
		}

		if (edit is DecrementNode decrement)
		{
			var destination = decrement.Object.Clone();
			var type = destination.GetType();

			return new OperatorNode(Operators.ASSIGN_SUBTRACT, decrement.Position).SetOperands(
				destination,
				new NumberNode(type.Format, 1L, decrement.Position)
			);
		}

		// Require the edit to be a standard assignment
		if (edit is not OperatorNode assignment || !assignment.Is(Operators.ASSIGN))
		{
			return null;
		}

		// Look for patterns such as: a = a + 1, x = x / 2
		if (assignment.Right is not OperatorNode operation)
		{
			return null;
		}

		var value = (Node?)null;

		// 1. Ensure either the left or the right operand is the same as the destination of the assignment
		// 2. If the operation is division, modulus or subtraction, the order of the operands matter, therefore require the destination variable to be on the left side of the operation in these instances
		if (operation.Left.Equals(assignment.Left))
		{
			value = operation.Right.Clone();
		}
		else if (operation.Right.Equals(assignment.Left) && operation.Operator != Operators.DIVIDE && operation.Operator != Operators.MODULUS && operation.Operator != Operators.SUBTRACT)
		{
			value = operation.Left.Clone();
		}

		if (value == null)
		{
			return null;
		}

		var action_operator = Operators.GetActionOperator(operation.Operator);

		if (action_operator == null)
		{
			return null;
		}

		return new OperatorNode(action_operator, assignment.Position).SetOperands(assignment.Left.Clone(), value);
	}

	/// <summary>
	/// Tries to build an assignment operator out of the specified edit
	/// Examples:
	/// x++ => x = x + 1
	/// x-- => x = x - 1
	/// a *= 2 => a = a * 2
	/// b[i] /= 10 => b[i] = b[i] / 10
	/// </summary>
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
			new OffsetNode(start.Clone(), new NumberNode(Parser.Format, 0L)),
			new DataPointer(expected.Configuration.Descriptor)
		);

		var b = new FunctionNode(Parser.InheritanceFunction!).SetParameters(new Node {
			new OffsetNode(start, new NumberNode(Parser.Format, 0L)),
			new DataPointer(expected.Configuration.Descriptor)
		});

		return new OperatorNode(Operators.OR).SetOperands(a, b);
	}

	/// <summary>
	/// Rewrites is-expressions so that they use nodes which can be compiled
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
			GetInsertPosition(expression).Insert(initialization);

			// Get the context of the expression
			var expression_context = expression.GetParentContext();

			// Declare a variable which is used to store the inspected object
			var object_type = expression.Object.GetType();
			var object_variable = expression_context.DeclareHidden(object_type);

			// Object variable should be declared
			initialization = new DeclareNode(object_variable);

			GetInsertPosition(expression).Insert(initialization);

			// Load the inspected object
			var load = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(object_variable),
				expression.Object
			);

			var assignment_context = new Context(expression_context);

			// Create a condition which passes if the inspected object is the expected type
			var condition = CreateTypeCondition(new VariableNode(object_variable), expression.Type);

			// Create an assignment which assigns the inspected object to the result variable while casting it to the expected type
			var assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(expression.Result),
				new CastNode(new VariableNode(object_variable), new TypeNode(expression.Type))
			);

			var conditional_assignment = new IfNode(assignment_context, condition, new Node { assignment }, expression.Position, null);

			// Create a condition which represents the result of the is expression
			var result_condition = new VariableNode(expression.Result);

			// Replace the expression with the logic above
			expression.Replace(new InlineNode(expression.Position) { load, conditional_assignment, result_condition });
		}
	}

	/// <summary>
	/// Returns if stack construction should be used
	/// </summary>
	private static bool IsStackConstructionPreferred(Node root, Node value)
	{
		var editor = Analyzer.TryGetEditor(value);
		if (editor == null) return false;

		var edited = Analyzer.GetEdited(editor);
		if (!edited.Is(NodeType.VARIABLE)) return false;

		var variable = edited.To<VariableNode>().Variable;
		if (!variable.IsPredictable) return false;

		// Refresh the usages of the variable in order to analyze whether this variable is inlinable
		Analyzer.FindUsages(variable, root);

		return variable.IsInlined;
	}

	/// <summary>
	/// Rewrites construction expressions so that they use nodes which can be compiled
	/// </summary>
	private static void RewriteConstructions(Node root)
	{
		var constructions = root.FindAll(i => i.Is(NodeType.CONSTRUCTION)).Cast<ConstructionNode>();

		foreach (var construction in constructions)
		{
			if (!IsStackConstructionPreferred(root, construction))
			{
				continue;
			}

			var replacement = Common.CreateStackConstruction(construction.GetType(), construction.Constructor);
			construction.Replace(replacement);
		}

		constructions = root.FindAll(i => i.Is(NodeType.CONSTRUCTION)).Cast<ConstructionNode>();

		foreach (var construction in constructions)
		{
			var replacement = Common.CreateHeapConstruction(construction.GetType(), construction.Constructor);
			construction.Replace(replacement);
		}
	}

	/// <summary>
	/// Finds expressions which do not represent statement conditions and can be evaluated to booleans values
	/// Example:
	/// element.is_visible = element.color.alpha > 0
	/// </summary>
	private static List<OperatorNode> FindBooleanValues(Node root)
	{
		var candidates = root.FindAll(i => i.Is(NodeType.OPERATOR) && (i.Is(OperatorType.COMPARISON) || i.Is(OperatorType.LOGIC))).Cast<OperatorNode>();

		return candidates.Where(candidate =>
		{
			var node = candidate.FindParent(i => !i.Is(NodeType.CONTENT))!;

			if (ReconstructionAnalysis.IsStatement(node) || node.Is(NodeType.NORMAL) || IsCondition(candidate))
			{
				return false;
			}

			// Ensure the parent is not a comparison or a logical operator
			return node is not OperatorNode operation || (operation.Operator.Type != OperatorType.COMPARISON && operation.Operator.Type != OperatorType.LOGIC);

		}).ToList();
	}

	/// <summary>
	/// Finds all the boolean values under the specified node and rewrites them using conditional statements
	/// </summary>
	private static void OutlineBooleanValues(Node root)
	{
		var instances = FindBooleanValues(root);

		foreach (var instance in instances)
		{
			// Declare a hidden variable which represents the result
			var environment = instance.GetParentContext();
			var destination = environment.DeclareHidden(Primitives.CreateBool());

			// Initialize the result with value 'false'
			var initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
			   new VariableNode(destination),
			   new NumberNode(Assembler.Format, 0L)
			);

			// Replace the operation with the result
			var inline = new InlineNode(instance.Position);
			instance.Replace(inline);

			var context = new Context(environment);

			// The destination is edited inside the following statement
			var assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
			   new VariableNode(destination),
			   new NumberNode(Assembler.Format, 1L)
			);

			destination.Writes.Add(assignment);

			// Create a conditional statement which sets the value of the destination variable to true if the condition is true
			var statement = new IfNode(context, instance, new Node { assignment }, instance.Position, null);

			// Add the statements which implement the boolean value
			inline.Add(initialization);
			inline.Add(statement);
			inline.Add(new VariableNode(destination));
		}
	}

	/// <summary>
	/// Returns whether a value is expected to return from the specified node
	/// </summary>
	public static bool IsValueUsed(Node value)
	{
		return value.Parent!.Is(
			NodeType.CALL,
			NodeType.CAST,
			NodeType.CONTENT,
			NodeType.CONSTRUCTION,
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
	/// Returns true if the specified node represents a statement
	/// </summary>
	public static bool IsStatement(Node node)
	{
		return node.Is(NodeType.ELSE, NodeType.ELSE_IF, NodeType.IF, NodeType.LOOP, NodeType.SCOPE);
	}

	/// <summary>
	/// Returns true if the specified node represents a scope
	/// </summary>
	public static bool IsScope(Node node)
	{
		return node.Is(NodeType.SCOPE);
	}

	/// <summary>
	/// Returns true if the specified node represents a condition
	/// </summary>
	public static bool IsCondition(Node node)
	{
		var statement = node.FindParent(i => i.Is(NodeType.ELSE_IF, NodeType.IF, NodeType.LOOP));

		if (statement == null)
		{
			return false;
		}

		if (statement.Is(NodeType.IF))
		{
			return ReferenceEquals(statement.To<IfNode>().Condition, node);
		}

		if (statement.Is(NodeType.ELSE_IF))
		{
			return ReferenceEquals(statement.To<ElseIfNode>().Condition, node);
		}

		if (statement.Is(NodeType.LOOP))
		{
			return ReferenceEquals(statement.To<LoopNode>().Condition, node);
		}

		return false;
	}

	/// <summary>
	/// Returns true if the specified node influences a call argument.
	/// This is determined by looking for a parent which represents a call
	/// </summary>
	public static bool IsPartOfCallArgument(Node node)
	{
		return node.FindParent(i => i.Is(NodeType.CALL, NodeType.FUNCTION, NodeType.UNRESOLVED_FUNCTION)) != null;
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

	/// <summary>
	/// Ensures all the edits under the specified node are assignment operations
	/// </summary>
	private static void RewriteAllEditsAsAssignOperations(Node root)
	{
		var edits = root.FindAll(i => i.Is(NodeType.INCREMENT, NodeType.DECREMENT) || (i is OperatorNode x && x.Operator.Type == OperatorType.ACTION));

		foreach (var edit in edits)
		{
			var replacement = TryRewriteAsAssignOperation(edit);
			if (replacement == null) throw new ApplicationException("Could not unwrap an edit node");

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
			var environment = expression.GetParentContext();
			var inline = new ContextInlineNode(new Context(environment), expression.Position);

			var assignment = TryRewriteAsAssignOperation(expression, true);

			if (assignment == null) throw new ApplicationException("Could not rewrite increment or decrement as an assign operation");

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
		var inlines = root.FindAll(i => i.Is(NodeType.INLINE)).Cast<InlineNode>();

		foreach (var inline in inlines)
		{
			// If the inline node contains only one child node, the inline node can be replaced with it
			if (inline.Count() != 1)
			{
				continue;
			}

			inline.Replace(inline.Left);

			if (inline.IsContext)
			{
				var environment = inline.GetParentContext();
				environment.Merge(inline.To<ContextInlineNode>().Context);
			}
		}
	}

	/// <summary>
	/// Inlines the specified destination operand by replacing it with generated nodes.
	/// The rest of the required steps are returned as an array of nodes.
	/// The returned nodes should be executed before the destination operand.
	/// </summary>
	public static Node[] InlineDestination(Context environment, Node destination)
	{
		// Primitive destinations do not need inlining
		if (Analysis.IsPrimitive(destination))
		{
			return Array.Empty<Node>();
		}

		var type = destination.Left.GetType();

		// Example:
		// x.y.z = f(0)
		// =>
		// a = x.y
		// b = f(0)
		// a.z = b
		if (destination.Is(NodeType.LINK))
		{
			// Take the left side of the link and remove it
			var left = destination.Left;
			left.Remove();

			// Load the left side into a variable
			var left_variable = environment.DeclareHidden(type);
			var left_assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(left_variable),
				left
			);

			// Replace the left side with the loaded variable
			destination.Left.Insert(new VariableNode(left_variable));

			return new[] { left_assignment };
		}

		if (!destination.Is(NodeType.OFFSET))
		{
			throw new ApplicationException("Could not inline the destination");
		}

		// Example:
		// x.y.z[g(i)] = f(0)
		// =>
		// a = x.y.z
		// b = g(i)
		// c = f(0)
		// a[b] = c
		var operation = destination.To<OffsetNode>();
		var start = operation.Start;
		var offset = operation.Offset;

		// Load the start into a variable
		var start_variable = environment.DeclareHidden(type);
		var start_assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(start_variable),
			start
		);

		type = offset.GetType();

		// Load the offset into a variable
		var offset_variable = environment.DeclareHidden(type);
		var offset_assignment = new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(offset_variable),
			offset
		);

		// Replace the destination with a new offset operation using the loaded variables
		destination.Detach();
		destination.Add(new VariableNode(start_variable));
		destination.Add(new ContentNode { new VariableNode(offset_variable) });

		return new[] { start_assignment, offset_assignment };
	}

	/// <summary>
	/// Moves and merges the specified inline node so that it is not in the middle of an expression
	/// </summary>
	private static void LiftupInlineNode(InlineNode inline)
	{
		while (true)
		{
			var parent = inline.Parent!;

			// Lifting should be stopped when a statement is encountered, since it can not be inlined
			if (IsStatement(parent))
			{
				// Merge the context of the current inline node with the parent context
				if (inline.IsContext)
				{
					var environment = inline.GetParentContext();
					environment.Merge(inline.To<ContextInlineNode>().Context);
				}

				// Replace the inline node with its child nodes and then stop
				inline.ReplaceWithChildren(inline);
				break;
			}

			// 1. Normal node indicates a section which should not be moved
			// 2. Logical operators create conditional sections, therefore lifting should be stopped
			// 3. If there is no value in the inline node, do nothing, since this situation is spooky
			if (parent.Is(NodeType.NORMAL, NodeType.LIST) || parent.Is(OperatorType.LOGIC) || inline.Last == null)
			{
				break;
			}

			if (parent.Is(NodeType.INLINE))
			{
				if (inline.IsContext)
				{
					if (parent.To<InlineNode>().IsContext)
					{
						// Transfer the contents of the inline context to the context of the parent
						parent.To<ContextInlineNode>().Context.Merge(inline.To<ContextInlineNode>().Context);

						// Replace the inline node with its child nodes
						inline.ReplaceWithChildren(inline);

						// Continue with the destination node
						inline = parent.To<ContextInlineNode>();
					}
					else
					{
						// Transfer all the child nodes of the inline to the parent node for a while
						inline.ReplaceWithChildren(inline);

						// Now take all the child nodes from the parent node
						parent.ForEach(inline.Add);

						// Replace the parent with the inline node and continue
						parent.Replace(inline);
					}
				}
				else
				{
					// Transfer all the child nodes of the inline to the parent node
					inline.ReplaceWithChildren(inline);

					// Continue with the destination node
					inline = parent.To<InlineNode>();
				}

				continue;
			}

			if (parent.Is(NodeType.DECREMENT, NodeType.INCREMENT, NodeType.NEGATE, NodeType.RETURN, NodeType.INSPECTION, NodeType.CONTENT, NodeType.NOT))
			{
				// Take out the value of the inline node
				var value = inline.Last;
				inline.Remove(value);

				var operation = parent.Clone();
				operation.Detach();

				operation.Add(value);

				inline.Add(operation);

				parent.Replace(inline);
				continue;
			}

			if (IsPartOfCallArgument(inline))
			{
				// Example:
				// f(g(), { a = 0, if x > 0 { a = 1 }, a })
				// =>
				// { x = g(), y = { a = 0, if x > 0 { a = 1 }, a }, f(x, y) }

				var context = inline.GetParentContext();
				var environment = new ContextInlineNode(new Context(context), inline.Position);

				// Find the call
				var call = inline.FindParent(i => i.Is(NodeType.CALL, NodeType.FUNCTION, NodeType.UNRESOLVED_FUNCTION))!;

				var parameters = call;
				var root = call;

				// Handle manual function calls by loading the self node and the pointer node into variables
				if (call.Is(NodeType.CALL))
				{
					var node = call.To<CallNode>();
					var self = node.Self;
					var pointer = node.Pointer;

					self.Remove();
					pointer.Remove();

					parameters = node.Parameters;

					// Load the self pointer into a variable
					var self_variable = environment.Context.DeclareHidden(self.GetType());
					environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
						new VariableNode(self_variable),
						self
					));

					// Load the function pointer into a variable
					var pointer_variable = environment.Context.DeclareHidden(pointer.GetType());
					environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
						new VariableNode(pointer_variable),
						pointer
					));

					call.Left.Insert(new VariableNode(pointer_variable));
					call.Left.Insert(new VariableNode(self_variable));
				}

				// Handle member function calls by loading the instance into a variable before anything else
				if (call.Parent!.Is(NodeType.LINK))
				{
					// Load the instance into a variable
					var instance = call.Parent.Left;
					instance.Remove();

					var instance_variable = environment.Context.DeclareHidden(call.Parent.Left.GetType());
					environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
						new VariableNode(instance_variable),
						instance
					));

					// Replace the left side of the link with the loaded variable
					call.Parent.Left.Insert(new VariableNode(instance_variable));

					// Since the call is a member function call the root is the link
					root = call.Parent;
				}

				var arguments = new List<Node>();

				// Extract its arguments to temporary variables
				while (parameters.Any())
				{
					// Take out the first argument
					var argument = parameters.Left;
					argument.Remove();

					var variable = environment.Context.DeclareHidden(argument.GetType());
					environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
						new VariableNode(variable),
						argument
					));

					arguments.Add(new VariableNode(variable));
				}

				// Add all the loaded variables as arguments to the call node
				arguments.ForEach(parameters.Add);

				root.Replace(environment);
				environment.Add(root);

				// Liftup the created inline node and after that continue lifting up the current inline node
				LiftupInlineNode(environment);
				continue;
			}

			if (parent.First == inline)
			{
				// Example:
				// { ..., a[i] } = f()
				// { ..., a[i] = f() }

				var value = inline.Last;

				var operation = parent.Clone();
				operation.Detach();

				inline.Remove(value);

				operation.Add(value);
				operation.Add(parent.Right.Clone());

				inline.Add(operation);

				parent.Replace(inline);
				parent.Detach();
			}
			else if (Analysis.IsPrimitive(parent.Left))
			{
				// Example:
				// a = { ..., 1 + 2 }
				// { ... a = 1 + 2 }

				var value = inline.Last;

				var operation = parent.Clone();
				operation.Detach();

				inline.Remove(value);

				operation.Add(parent.Left.Clone());
				operation.Add(value);

				inline.Add(operation);

				parent.Replace(inline);
				parent.Detach();
			}
			else if (Analyzer.IsEdited(parent.Left))
			{
				// Example:
				// x.y.z[i] = { a = i, i++, a }
				// =>
				// { m = x.y.z, n = i, a = i, i++, m[n] = a }

				var context = inline.GetParentContext();
				var environment = new ContextInlineNode(new Context(context), inline.Position);

				// Handle the inlinement of the destination node
				InlineDestination(environment.Context, parent.Left).ForEach(i => environment.Add(i));

				// Load the right side of the operation into a variable
				var type = parent.Right.GetType();
				var value = environment.Context.DeclareHidden(type);

				environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(value),
					parent.Right
				));

				// Finally, clone the parent and ensure it does not have any child nodes
				var operation = parent.Clone();
				operation.Detach();

				operation.Add(parent.Left);
				operation.Add(new VariableNode(value));

				// Add the operation to the new inline node
				environment.Add(operation);

				parent.Replace(environment);

				/// NOTE: No need to worry about context leak here, since the inline node is under the environment node
				inline = environment;
			}
			else
			{
				// Example:
				// f(x) + { a = f(x + 1), b = f(x + 2), a + b }
				// =>
				// { i = f(x), a = f(x + 1), b = f(x + 2), i + a + b }

				// Take out the value of the inline node
				var value = inline.Last;
				inline.Remove(value);

				var context = inline.GetParentContext();
				var environment = new ContextInlineNode(new Context(context), inline.Position);

				// Get the type of the left side
				var type = parent.Left.GetType();

				// Load the left side into a variable and execute it before the right side
				var left = environment.Context.DeclareHidden(type);
				environment.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
					new VariableNode(left),
					parent.Left
				));

				// Transfer all child nodes to the environment node
				inline.ForEach(environment.Add);

				// If the inline node has a context, transfer its data to the environment context
				if (inline.IsContext)
				{
					environment.Context.Merge(inline.To<ContextInlineNode>().Context);
				}
				
				// Finally, clone the parent and ensure it does not have any child nodes
				var operation = parent.Clone();
				operation.Detach();

				operation.Add(new VariableNode(left));
				operation.Add(value);

				// Add the operation in order to make it the return value of the new inline node 
				environment.Add(operation);

				parent.Replace(environment);

				inline = environment;
			}
		}
	}

	/// <summary>
	/// Moves and merges inlines nodes so that they are not in the middle of an expression
	/// </summary>
	private static void LiftupInlineNodes(Node root)
	{
		while (true)
		{
			// Try to find the next inline node which can be lifted up
			var inline = root.Find(i => i.Is(NodeType.INLINE) && !IsStatement(i.Parent!) && !i.Parent!.Is(NodeType.NORMAL) && !i.Parent.Is(OperatorType.LOGIC) && i.Any());

			if (inline == null)
			{
				break;
			}

			LiftupInlineNode(inline.To<InlineNode>());
		}
	}

	/// <summary>
	/// Rewrites the specified remainder operation where the divisor is an integer constant as:
	/// Formula: a % c = a - (a / c) * c
	/// </summary>
	private static void RewriteRemainderOperation(OperatorNode remainder)
	{
		if (!remainder.Right.Is(NodeType.NUMBER))
		{
			throw new InvalidOperationException("Rewriting a remainder operation requires the divisor to be an integer constant");
		}

		var environment = remainder.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), remainder.Position);

		var a = inline.Context.DeclareHidden(remainder.Left.GetType());

		inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(a),
			remainder.Left.Clone()
		));

		// Formula: a % c = a - (a / c) * c
		inline.Add(new OperatorNode(Operators.SUBTRACT).SetOperands(
			new VariableNode(a),
			new OperatorNode(Operators.MULTIPLY).SetOperands(
				new OperatorNode(Operators.DIVIDE).SetOperands(
					new VariableNode(a),
					remainder.Right.Clone()
				),
				remainder.Right.Clone()
			)
		));

		remainder.Replace(inline);
	}

	/// <summary>
	/// Rewrites remainder operations which have a constant divisors as:
	/// Formula: a % b = a - (a / b) * b
	/// </summary>
	public static void RewriteRemainderOperations(Node root)
	{
		var remainders = root.FindAll(i => i.Is(Operators.MODULUS)).Cast<OperatorNode>();

		foreach (var remainder in remainders)
		{
			if (remainder.Right.Is(NodeType.NUMBER))
			{
				RewriteRemainderOperation(remainder);
			}
		}
	}

	/// <summary>
	/// Returns whether the node uses the local self pointer.
	/// This function assumes the node is a member object.
	/// </summary>
	public static bool IsUsingLocalSelfPointer(Node node)
	{
		var link = node.Parent!;

		// Take into account the following situation:
		// Inheritant Inheritor {
		//   init() { 
		//     Inheritant.init()
		//     Inheritant.member = 0
		//   }
		// }
		if (link.Left.Is(NodeType.TYPE)) return true;

		// Take into account the following situation:
		// Namespace.Inheritant Inheritor {
		//   init() { 
		//     Namespace.Inheritant.init()
		//     Namespace.Inheritant.member = 0
		//   }
		// }
		return link.Left.Is(NodeType.LINK) && link.Left.Right.Is(NodeType.TYPE);
	}

	/// <summary>
	/// Rewrites supertypes accesses so that they can be compiled
	/// Example:
	/// Base Inheritor {
	/// 	a: large
	/// 
	/// 	init() {
	/// 		Base.a = 1
	/// 		# The expression is rewritten as:
	/// 		this.a = 1
	/// 		# The rewritten expression still refers to the same member variable even though Inheritor has its own member variable a
	/// 	}
	/// }
	/// </summary>
	public static void RewriteSupertypeAccessors(Node root)
	{
		var links = root.FindTop(i => i.Is(NodeType.LINK)).Cast<LinkNode>();

		foreach (var link in links)
		{
			if (!IsUsingLocalSelfPointer(link.Right))
			{
				continue;
			}

			if (link.Right.Is(NodeType.FUNCTION))
			{
				var function = link.Right.To<FunctionNode>();

				if (function.Function.IsStatic || !function.Function.IsMember)
				{
					continue;
				}
			}
			else if (link.Right.Is(NodeType.VARIABLE))
			{
				var variable = link.Right.To<VariableNode>();

				if (variable.Variable.IsStatic || !variable.Variable.IsMember)
				{
					continue;
				}
			}
			else
			{
				continue;
			}

			link.Left.Replace(Common.GetSelfPointer(link.GetParentContext(), link.Left.Position));
		}
	}

	/// <summary>
	/// Casts called objects to match the expected self pointer type
	/// </summary>
	public static void CastMemberCalls(Node root)
	{
		var calls = root.FindAll(i => i.Is(NodeType.LINK) && i.Last!.Is(NodeType.FUNCTION)).Cast<LinkNode>();

		foreach (var call in calls)
		{
			var left = call.Left;

			var function = call.Right.To<FunctionNode>();
			var expected = function.Function.Metadata.GetTypeParent() ?? throw new ApplicationException("Missing parent type");
			var actual = left.GetType();

			if (actual == expected || actual.GetSupertypeBaseOffset(expected) == 0)
			{
				continue;
			}

			left.Remove();

			call.Right.Insert(new CastNode(
				left,
				new TypeNode(expected)
			));
		}
	}
	
	/// <summary>
	/// Finds casts which have no effect and removes them
	/// Example: x = 0 as large
	/// </summary>
	public static void RemoveRedundantCasts(Node root)
	{
		var casts = root.FindAll(i => i.Is(NodeType.CAST)).Cast<CastNode>();

		foreach (var cast in casts)
		{
			// Do not remove the cast if it changes the type
			if (!ReferenceEquals(cast.GetType(), cast.Object.GetType())) continue;

			// Remove the cast since it does nothing
			cast.Replace(cast.Object);
		}
	}

	/// <summary>
	/// Finds assignments which have implicit casts and adds them
	/// </summary>
	public static void AddAssignmentCasts(Node root)
	{
		var assignments = root.FindAll(i => i.Is(Operators.ASSIGN));

		foreach (var assignment in assignments)
		{
			var to = assignment.Left.GetType();
			var from = assignment.Right.GetType();

			// Skip assignments which do not cast the value
			if (to == from) continue;

			// If the right operand is a number and it is converted into different kind of number, it can be done without a cast node
			if (assignment.Right.Is(NodeType.NUMBER) && from is Number && to is Number)
			{
				assignment.Right.To<NumberNode>().Convert(to.Format);
				continue;
			}

			// Remove the right operand from the assignment
			var value = assignment.Right;
			value.Remove();

			// Now cast the right operand and add it back
			assignment.Add(new CastNode(value, new TypeNode(to, value.Position), value.Position));
		}
	}

	/// <summary>
	/// Rewrites lambda nodes using simpler nodes
	/// </summary>
	public static void RewriteLambdaConstructions(Node root)
	{
		var lambdas = root.FindAll(i => i.Is(NodeType.LAMBDA)).Cast<LambdaNode>();

		foreach (var lambda in lambdas)
		{
			var environment = lambda.GetParentContext();
			var inline = new ContextInlineNode(new Context(environment));

			var lambda_implementation = (LambdaImplementation)lambda.Implementation!;

			lambda_implementation.Seal();

			var type = lambda_implementation.Type!;
			var allocator = (Node?)null;

			if (IsStackConstructionPreferred(root, lambda))
			{
				allocator = new CastNode(new StackAddressNode(inline.Context, type), new TypeNode(type));
			}
			else
			{
				allocator = new CastNode(
					new FunctionNode(Parser.AllocationFunction!, lambda.Position).SetParameters(new Node { new NumberNode(Parser.Format, (long)type.ContentSize) }),
					new TypeNode(type)
				);
			}

			var container = inline.Context.DeclareHidden(type);
			var allocation = new OperatorNode(Operators.ASSIGN).SetOperands
			(
				new VariableNode(container),
				allocator
			);

			inline.Add(allocation);

			var function_pointer_assignment = new OperatorNode(Operators.ASSIGN).SetOperands
			(
				new LinkNode(new VariableNode(container), new VariableNode(lambda_implementation.Function!)),
				new DataPointer(lambda_implementation)
			);

			inline.Add(function_pointer_assignment);

			foreach (var capture in lambda_implementation.Captures)
			{
				var assignment = new OperatorNode(Operators.ASSIGN).SetOperands
				(
					new LinkNode(new VariableNode(container), new VariableNode(capture)),
					new VariableNode(capture.Captured)
				);

				inline.Add(assignment);
			}

			inline.Add(new VariableNode(container));

			lambda.Replace(inline);
		}
	}

	/// <summary>
	/// Finds statements which can not be reached and removes them
	/// </summary>
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

	/// <summary>
	/// Finds links whose right operand does not need the left operand.
	/// Those links will be replaced with the right operand.
	/// </summary>
	private static void StripLinks(Node root)
	{
		var links = root.FindAll(i => i.Is(NodeType.LINK));

		foreach (var link in links)
		{
			var right = link.Right;

			if (right.Is(NodeType.VARIABLE))
			{
				if (right.To<VariableNode>().Variable.IsMember && !right.To<VariableNode>().Variable.IsStatic) continue;
			}
			else if (right.Is(NodeType.FUNCTION))
			{
				if (right.To<FunctionNode>().Function.IsMember && !right.To<FunctionNode>().Function.IsStatic) continue;
			}
			else if (!right.Is(NodeType.CONSTRUCTION))
			{
				continue;
			}

			link.Replace(right);
		}
	}

	/// <summary>
	/// Rewrites nodes under the specified node to match the requirements to be analyzed and passed to the back end
	/// </summary>
	public static void Reconstruct(Node root)
	{
		StripLinks(root);
		RemoveRedundantParenthesis(root);
		RemoveCancellingNegations(root);
		RemoveCancellingNots(root);
		RemoveRedundantCasts(root);
		AddAssignmentCasts(root);
		RewriteSupertypeAccessors(root);
		RewriteIsExpressions(root);
		RewriteLambdaConstructions(root);
		RewriteConstructions(root);
		OutlineBooleanValues(root);
		RewriteDiscardedIncrements(root);
		RewriteIncrements(root);
		RewriteAllEditsAsAssignOperations(root);
		RewriteRemainderOperations(root);
		CastMemberCalls(root);

		if (Analysis.IsFunctionInliningEnabled)
		{
			Inlines.Build(root);
		}

		SubstituteInlineNodes(root);
		LiftupInlineNodes(root);
	}

	/// <summary>
	/// Do some finishing touches to the nodes under the specified node
	/// </summary>
	public static void Finish(Node root)
	{
		ConstructActionOperations(root);
		SubstituteInlineNodes(root);
		RewriteRemainderOperations(root);

		/// NOTE: Inline nodes can be directly under logical operators now, so outline possible booleans values which represent conditions
		OutlineBooleanValues(root);
	}
}