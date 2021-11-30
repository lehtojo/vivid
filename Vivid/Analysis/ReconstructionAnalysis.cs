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
		if (!nodes.Any()) return null;
		if (nodes.Length == 1) return nodes.First().FindParent(NodeType.SCOPE);

		var shared = nodes[0];

		for (var i = 1; i < nodes.Length; i++)
		{
			shared = Node.GetSharedNode(shared, nodes[i], false);
			if (shared == null) return null;
		}

		return shared.Is(NodeType.SCOPE) ? shared : shared.FindParent(NodeType.SCOPE);
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
				new NumberNode(Assembler.Format, 1L, increment.Position)
			);
		}

		if (edit is DecrementNode decrement)
		{
			var destination = decrement.Object.Clone();
			var type = destination.GetType();

			return new OperatorNode(Operators.ASSIGN_SUBTRACT, decrement.Position).SetOperands(
				destination,
				new NumberNode(Assembler.Format, 1L, decrement.Position)
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
	public static Node? TryRewriteAsAssignmentOperation(Node edit, bool force = false)
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
				if (operation.Operator.Type != OperatorType.ACTION) return null;
				if (operation.Operator == Operators.ASSIGN) return edit;

				var destination = operation.Left.Clone();
				var type = ((ActionOperator)operation.Operator).Operator;

				if (type == null) return null;

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
	/// Removes redundant parentheses in the specified node tree
	/// Example: x = x * (((x + 1)))
	/// </summary>
	private static void RemoveRedundantParentheses(Node root)
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

			// Remove all parentheses, which block logical operators
			if (root.First is OperatorNode x && x.Operator.Type == OperatorType.LOGIC)
			{
				root.Replace(root.First!);
			}
		}

		root.ForEach(RemoveRedundantParentheses);
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

		var condition = new FunctionNode(Parser.InheritanceFunction!).SetArguments(new Node {
			new OffsetNode(start, new NumberNode(Parser.Format, 0L)),
			new DataPointer(expected.Configuration.Descriptor)
		});

		return condition;
	}

	/// <summary>
	/// Rewrites is-expressions so that they use nodes which can be compiled
	/// </summary>
	private static void RewriteIsExpressions(Node root)
	{
		var expressions = root.FindAll(NodeType.IS).Cast<IsNode>().ToList();

		for (var i = expressions.Count - 1; i >= 0; i--)
		{
			var expression = expressions[i];

			if (expression.HasResultVariable) continue;

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
			initialization = new OperatorNode(Operators.ASSIGN).SetOperands(
				new VariableNode(object_variable),
				new UndefinedNode(object_variable.Type!, object_variable.GetRegisterFormat())
			);

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
			var result_condition = new OperatorNode(Operators.NOT_EQUALS).SetOperands(
				new VariableNode(expression.Result),
				new NumberNode(Parser.Format, 0L)
			);

			// Replace the expression with the logic above
			expression.Replace(new InlineNode(expression.Position) { load, conditional_assignment, result_condition });
		}
	}

	/// <summary>
	/// Rewrites when-expressions so that they use nodes which can be compiled
	/// </summary>
	private static void RewriteWhenExpressions(Node root)
	{
		var expressions = root.FindAll(NodeType.WHEN).Cast<WhenNode>();

		foreach (var expression in expressions)
		{
			var position = expression.Position;
			var return_type = Resolver.GetSharedType(expression.Sections.Select(i => expression.GetSectionBody(i).Last!.GetType()).ToArray()) ?? throw Errors.Get(position, "Could not resolve the return type of the statement");
			var container = Common.CreateInlineContainer(return_type, expression);

			// The load must be executed before the actual when-statement
			container.Node.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
				expression.Inspected.Clone(),
				expression.Value
			));

			// Define the result variable
			container.Node.Add(new OperatorNode(Operators.ASSIGN, position).SetOperands(
				new VariableNode(container.Result),
				new UndefinedNode(return_type, return_type.GetRegisterFormat())
			));

			foreach (var section in expression.Sections)
			{
				var body = expression.GetSectionBody(section);

				// Load the return value of the section to the return value variable
				var value = body.Last!;
				var destination = new Node();
				value.Replace(destination);

				destination.Replace(new OperatorNode(Operators.ASSIGN, value.Position).SetOperands(
					new VariableNode(container.Result, value.Position),
					value
				));

				container.Node.Add(section);
			}

			container.Destination.Replace(container.Node);
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

		return variable.IsInlined();
	}

	/// <summary>
	/// Rewrites construction expressions so that they use nodes which can be compiled
	/// </summary>
	private static void RewriteConstructions(Node root)
	{
		var constructions = root.FindAll(NodeType.CONSTRUCTION).Cast<ConstructionNode>();

		foreach (var construction in constructions)
		{
			// 1. Use stack allocation, if the user forces it
			// 2. Try to automatically detect whether to use stack allocation here
			if (!construction.IsStackAllocated && !IsStackConstructionPreferred(root, construction)) continue;

			var container = Common.CreateStackConstruction(construction.GetType(), construction, construction.Constructor);
			container.Destination.Replace(container.Node);
		}

		constructions = root.FindAll(NodeType.CONSTRUCTION).Cast<ConstructionNode>();

		foreach (var construction in constructions)
		{
			var container = Common.CreateHeapConstruction(construction.GetType(), construction, construction.Constructor);
			container.Destination.Replace(container.Node);
		}
	}

	/// <summary>
	/// Finds expressions which do not represent statement conditions and can be evaluated to booleans values
	/// Example:
	/// element.is_visible = element.color.alpha > 0
	/// </summary>
	private static List<OperatorNode> FindBoolValues(Node root)
	{
		var candidates = root.FindAll(i => i.Is(NodeType.OPERATOR) && (i.Is(OperatorType.COMPARISON) || i.Is(OperatorType.LOGIC))).Cast<OperatorNode>();

		return candidates.Where(candidate =>
		{
			var node = candidate.FindParent(i => !i.Is(NodeType.CONTENT, NodeType.CONTENT))!;

			if (IsStatement(node) || node.Is(NodeType.NORMAL) || IsCondition(candidate)) return false;

			// Ensure the parent is not a comparison or a logical operator
			return node is not OperatorNode operation || operation.Operator.Type != OperatorType.LOGIC;

		}).ToList();
	}

	/// <summary>
	/// Finds all the boolean values under the specified node and rewrites them using conditional statements
	/// </summary>
	private static void ExtractBoolValues(Node root)
	{
		var expressions = FindBoolValues(root);

		foreach (var expression in expressions)
		{
			var container = Common.CreateInlineContainer(Primitives.CreateBool(), expression);
			var position = expression.Position;

			// Create the container, since it will contain a conditional statement
			container.Destination.Replace(container.Node);

			// Initialize the result with value 'false'
			var initialization = new OperatorNode(Operators.ASSIGN, position).SetOperands(
				new VariableNode(container.Result, position),
				new NumberNode(Parser.Format, 0L, position)
			);

			container.Node.Add(initialization);

			// The destination is edited inside the following statement
			var assignment = new OperatorNode(Operators.ASSIGN, position).SetOperands(
				new VariableNode(container.Result, position),
				new NumberNode(Parser.Format, 1L, position)
			);

			// Create a conditional statement which sets the value of the destination variable to true if the condition is true
			var environment = initialization.GetParentContext();
			var context = new Context(environment);

			var body = new Node();
			body.Add(assignment);

			var statement = new IfNode(context, expression, body, position, null);
			container.Node.Add(statement);

			// If the container node is placed inside an expression, the node must return the result
			container.Node.Add(new VariableNode(container.Result, position));
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
	/// Returns whether the specified node might have a direct effect on the flow
	/// </summary>
	public static bool IsAffector(Node node)
	{
		return node.Is(NodeType.CALL, NodeType.CONSTRUCTION, NodeType.DECLARE, NodeType.DECREMENT, NodeType.DISABLED, NodeType.FUNCTION, NodeType.INCREMENT, NodeType.INSTRUCTION, NodeType.JUMP, NodeType.LABEL, NodeType.LOOP_CONTROL, NodeType.RETURN) || node.Is(OperatorType.ACTION);
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
		var statement = node.FindParent(NodeType.ELSE_IF, NodeType.IF, NodeType.LOOP);

		if (statement == null) return false;
		if (statement.Is(NodeType.IF)) return ReferenceEquals(statement.To<IfNode>().Condition, node);
		if (statement.Is(NodeType.ELSE_IF)) return ReferenceEquals(statement.To<ElseIfNode>().Condition, node);
		if (statement.Is(NodeType.LOOP)) return ReferenceEquals(statement.To<LoopNode>().Condition, node);

		return false;
	}

	/// <summary>
	/// Returns true if the specified node influences a call argument.
	/// This is determined by looking for a parent which represents a call
	/// </summary>
	public static bool IsPartOfCallArgument(Node node)
	{
		return node.FindParent(NodeType.CALL, NodeType.FUNCTION, NodeType.UNRESOLVED_FUNCTION) != null;
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
		var increments = root.FindAll(NodeType.INCREMENT, NodeType.DECREMENT);

		foreach (var increment in increments)
		{
			if (IsValueUsed(increment)) continue;

			var replacement = TryRewriteAsActionOperation(increment) ?? throw new ApplicationException("Could not rewrite increment operation as assign operation");

			increment.Replace(replacement);
		}
	}

	/// <summary>
	/// Ensures all edits under the specified node are assignments
	/// </summary>
	private static void RewriteEditsAsAssignments(Node root)
	{
		var edits = root.FindAll(i => i.Is(NodeType.INCREMENT, NodeType.DECREMENT) || i.Is(OperatorType.ACTION));

		foreach (var edit in edits)
		{
			var replacement = TryRewriteAsAssignmentOperation(edit);
			if (replacement == null) throw new ApplicationException("Could not rewrite edit as an assignment operator");

			edit.Replace(replacement);
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
			if (assignment.Right is not OperatorNode operation) continue;

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

			if (value == null) continue;

			var action_operator = Operators.GetActionOperator(operation.Operator);
			if (action_operator == null) continue;

			assignment.Replace(new OperatorNode(action_operator, assignment.Position).SetOperands(assignment.Left, value));
		}
	}

	/// <summary>
	/// Finds all inlines nodes, which can be replaced with their own child nodes
	/// </summary>
	public static void RemoveRedundantInlineNodes(Node root)
	{
		var inlines = root.FindAll(NodeType.INLINE).Cast<InlineNode>();

		foreach (var inline in inlines)
		{
			// If the inline node contains only one child node, the inline node can be replaced with it
			if (inline.Count() == 1)
			{
				inline.Replace(inline.Left);
			}
			else if (inline.Parent != null && (ReconstructionAnalysis.IsStatement(inline.Parent) || inline.Parent.Is(NodeType.INLINE, NodeType.NORMAL)))
			{
				inline.ReplaceWithChildren(inline);
			}
			else
			{
				continue;
			}

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

	public static Node GetExpressionExtractPosition(Node expression)
	{
		#warning Upgrade second stage
		var iterator = expression.Parent;
		var position = expression;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.INLINE, NodeType.NORMAL, NodeType.SCOPE)) break;

			// Logical operators also act as scopes. You can not for example extract function calls from them, because those function calls are not always executed.
			// Example of what this function should do in the following situation:
			// a(b(i)) and c(d(j))
			// =>
			// { x = b(i), a(x) } and { y = d(j), c(y) }
			if (iterator.Is(OperatorType.LOGIC))
			{
				var scope = new InlineNode();
				position.Replace(scope);
				scope.Add(position);
				break;
			}

			position = iterator;
			iterator = iterator.Parent;
		}

		return position;
	}

	/// <summary>
	/// Returns all the increment and decrement nodes under the specified node
	/// </summary>
	private static List<Node> FindIncrements(Node root)
	{
		var result = new List<Node>();

		foreach (var node in root)
		{
			result.AddRange(FindIncrements(node));

			// Add the increment later than its child nodes, since the child nodes are executed first
			if (node.Is(NodeType.INCREMENT, NodeType.DECREMENT)) { result.Add(node); }
		}

		return result;
	}

	/// <summary>
	/// This function extracts function calls and boolean expressions from complex expressions, so that they are executed first.
	/// Example 1:
	/// a = f(x) + g(y)
	/// =>
	/// i = f(x)
	/// j = g(y)
	/// a = i + j
	/// Example 2:
	/// a.x = a.y + f(a) + a.z
	/// =>
	/// i = f(a)
	/// a.x = a.y + i + a.z
	/// Example 3:
	/// b = a.x + f(g(a) > h(a))
	/// =>
	/// i = f(g(a) > h(a))
	/// b = a.x + i
	/// =>
	/// j = g(a) > h(a)
	/// i = f(j)
	/// b = a.x + i
	/// =>
	/// k = g(a)
	/// l = h(a)
	/// j = k > l
	/// i = f(j)
	/// b = a.x + i
	/// </summary>
	private static void ExtractExpressions(Node root)
	{
		var nodes = root.FindAll(NodeType.CALL, NodeType.CONSTRUCTION, NodeType.FUNCTION, NodeType.LAMBDA, NodeType.WHEN);
		nodes.AddRange(FindBoolValues(root));

		for (var i = 0; i < nodes.Count; i++)
		{
			var node = nodes[i];
			var parent = node.Parent;

			// Calls should always have a parent node
			if (parent == null) continue;
			
			// Skip values which are assigned to hidden local variables
			if (parent.Is(Operators.ASSIGN) && ReferenceEquals(parent.Right, node) && parent.Left.Is(NodeType.VARIABLE) && parent.Left.To<VariableNode>().Variable.IsPredictable) continue;

			// Nothing can be done if the value is directly under a logical operator
			if (parent.Is(OperatorType.LOGIC) || parent.Is(NodeType.CONSTRUCTION)) continue;

			// Select the parent node, if the current node is a member function call
			if (node.Is(NodeType.FUNCTION) && parent.Is(NodeType.LINK) && parent.Right == node) { node = parent; }

			var position = GetExpressionExtractPosition(node);

			// Do nothing if the call should not move
			if (position == node) continue;

			var context = node.GetParentContext();
			var variable = context.DeclareHidden(node.GetType());

			// Replace the result of the call with the created variable
			node.Replace(new VariableNode(variable, node.Position));
			
			position.Insert(new OperatorNode(Operators.ASSIGN, node.Position).SetOperands(
				new VariableNode(variable, node.Position),
				node
			));
		}

		var all = FindIncrements(root);
		var filtered = all.GroupBy(i => GetExpressionExtractPosition(i), new ReferenceEqualityComparer<Node>()).ToArray();

		// Extract increment nodes
		foreach (var extracts in filtered)
		{
			// Create the extract position
			/// NOTE: This uses a temporary node, since sometimes the extract position can be next to an increment node, which is problematic
			var destination = new Node();
			extracts.Key.Insert(destination);

			var locals = extracts.Where(i => i.First().Is(NodeType.VARIABLE)).ToArray();
			var others = extracts.ToList();

			for (var i = others.Count - 1; i >= 0; i--)
			{
				if (locals.All(j => !ReferenceEquals(others[i], j))) continue;
				others.RemoveAt(i);
			}

			foreach (var extract in locals.GroupBy(i => i.First().To<VariableNode>().Variable).ToArray())
			{
				// Determine the edited node
				var edited = extract.Key;
				var difference = 0L;

				foreach (var increment in extract.Reverse())
				{
					var step = increment.Is(NodeType.INCREMENT) ? 1L : -1L;
					var post = (step == 1 && increment.To<IncrementNode>().Post) || (step == -1 && increment.To<DecrementNode>().Post);
					var position = increment.Position;

					if (post) { difference -= step; }

					if (difference > 0)
					{
						increment.Replace(new OperatorNode(Operators.ADD, position).SetOperands(increment.First(), new NumberNode(Parser.Format, difference, position)));
					}
					else if (difference < 0)
					{
						increment.Replace(new OperatorNode(Operators.SUBTRACT, position).SetOperands(increment.First(), new NumberNode(Parser.Format, -difference, position)));
					}
					else
					{
						increment.Replace(increment.First());
					}

					if (!post) { difference -= step; }
				}

				destination.Insert(new OperatorNode(Operators.ASSIGN_ADD, destination.Position).SetOperands(new VariableNode(edited, destination.Position), new NumberNode(Parser.Format, -difference, destination.Position)));
			}

			foreach (var increment in others)
			{
				// Determine the edited node
				var edited = increment.First();
				var environment = destination.GetParentContext();

				var value = environment.DeclareHidden(increment.GetType());
				var position = increment.Position;
				var load = new OperatorNode(Operators.ASSIGN, position).SetOperands(new VariableNode(value, position), edited.Clone());

				var step = increment.Is(NodeType.INCREMENT) ? 1L : -1L;
				var post = (step == 1 && increment.To<IncrementNode>().Post) || (step == -1 && increment.To<DecrementNode>().Post);
				
				if (post) { destination.Insert(load); }

				destination.Insert(new OperatorNode(Operators.ASSIGN_ADD, position).SetOperands(increment.First(), new NumberNode(Parser.Format, step, position)));
				increment.Replace(new VariableNode(value, position));

				if (!post) { destination.Insert(load); }
			}

			destination.Remove();
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

		// Formula: a % c = a - (a / c) * c
		remainder.Replace(new OperatorNode(Operators.SUBTRACT).SetOperands(
			remainder.Left.Clone(),
			new OperatorNode(Operators.MULTIPLY).SetOperands(
				new OperatorNode(Operators.DIVIDE).SetOperands(
					remainder.Left.Clone(),
					remainder.Right.Clone()
				),
				remainder.Right.Clone()
			)
		));
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
			if (!remainder.Right.Is(NodeType.NUMBER)) continue;
			RewriteRemainderOperation(remainder);
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
			var expected = function.Function.Metadata.FindTypeParent() ?? throw new ApplicationException("Missing parent type");
			var actual = left.GetType();

			if (actual == expected || actual.GetSupertypeBaseOffset(expected) == 0)
			{
				continue;
			}

			left.Remove();

			call.Right.Insert(new CastNode(left, new TypeNode(expected)));
		}
	}
	
	/// <summary>
	/// Finds casts which have no effect and removes them
	/// Example: x = 0 as large
	/// </summary>
	public static void RemoveRedundantCasts(Node root)
	{
		var casts = root.FindAll(NodeType.CAST).Cast<CastNode>();

		foreach (var cast in casts)
		{
			// Do not remove the cast if it changes the type
			if (!Equals(cast.GetType(), cast.Object.GetType())) continue;

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
			if (Equals(to, from)) continue;

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
		var lambdas = root.FindAll(NodeType.LAMBDA).Cast<LambdaNode>();

		foreach (var construction in lambdas)
		{
			var position = construction.Position;

			var environment = construction.GetParentContext();
			var implementation = (LambdaImplementation)construction.Implementation!;
			implementation.Seal();

			var type = implementation.Type!;

			var container = Common.CreateInlineContainer(type, construction);
			var allocator = (Node?)null;

			if (IsStackConstructionPreferred(root, construction))
			{
				allocator = new CastNode(new StackAddressNode(environment, type), new TypeNode(type), position);
			}
			else
			{
				var arguments = new Node();
				arguments.Add(new NumberNode(Parser.Format, (long)type.ContentSize));

				var call = new FunctionNode(Parser.AllocationFunction!, construction.Position).SetArguments(arguments);

				allocator = new CastNode(call, new TypeNode(type), position);
			}

			var allocation = new OperatorNode(Operators.ASSIGN, position).SetOperands
			(
				new VariableNode(container.Result),
				allocator
			);

			container.Node.Add(allocation);

			var function_pointer_assignment = new OperatorNode(Operators.ASSIGN, position).SetOperands
			(
				new LinkNode(new CastNode(new VariableNode(container.Result), new TypeNode(type)), new VariableNode(implementation.Function!), position),
				new DataPointer(implementation)
			);

			container.Node.Add(function_pointer_assignment);

			foreach (var capture in implementation.Captures)
			{
				var assignment = new OperatorNode(Operators.ASSIGN, position).SetOperands
				(
					new LinkNode(new CastNode(new VariableNode(container.Result), new TypeNode(type)), new VariableNode(capture), position),
					new VariableNode(capture.Captured)
				);

				container.Node.Add(assignment);
			}

			container.Node.Add(new VariableNode(container.Result));
			container.Destination.Replace(container.Node);
		}
	}

	/// <summary>
	/// Finds statements which can not be reached and removes them
	/// </summary>
	public static bool RemoveUnreachableStatements(Node root)
	{
		var return_statements = root.FindAll(NodeType.RETURN);
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
		var links = root.FindAll(NodeType.LINK);

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
	/// Creates all member accessors that represent all non-pack members
	/// Example (start = object.pack, type = { a: large, other: { b: large, c: large } })
	/// => { object.pack.a, object.pack.other.b, object.pack.other.c }
	/// </summary>
	private static List<Node> CreatePackMemberAccessors(Node start, Type type, Position position)
	{
		var result = new List<Node>();

		foreach (var member in type.Variables.Values)
		{
			var accessor = new LinkNode(start.Clone(), new VariableNode(member), position);

			if (member.Type!.IsPack)
			{
				result.AddRange(CreatePackMemberAccessors(accessor, member.Type!, position));
				continue;
			}

			result.Add(accessor);
		}

		return result;
	}
	
	/// <summary>
	/// Finds all usages of packs and rewrites them to be more suitable for compilation
	/// Example (Here $-prefixes indicate generated hidden variables):
	/// a = b
	/// c = f()
	/// g(c)
	/// Direct assignments are expanded:
	/// a.x = b.x
	/// a.y = b.y
	/// c = f() <- The original assignment is not removed, because it is needed by the placeholders
	/// c.x = [Placeholder 1] -> c.x
	/// c.y = [Placeholder 2] -> c.y
	/// g(c)
	/// Pack values are replaced with pack nodes:
	/// a.x = b.x
	/// a.y = b.y
	/// c = f() <- The original assignment is not removed, because it is needed by the placeholders
	/// c.x = [Placeholder 1] -> c.x
	/// c.y = [Placeholder 2] -> c.y
	/// g({ $c.x, $c.y }) <- Here a pack node is created, which creates a pack handle in the back end from the child values
	/// Member accessors are replaced with local variables:
	/// $a.x = $b.x
	/// $a.y = $b.y
	/// c = f() <- The original assignment is not removed, because it is needed by the placeholders
	/// c.x = [Placeholder 1] -> c.x
	/// c.y = [Placeholder 2] -> c.y
	/// g({ $c.x, $c.y })
	/// Finally, the placeholders are replaced with the actual nodes:
	/// $a.x = $b.x
	/// $a.y = $b.y
	/// c = f() <- The original assignment is not removed, because it is needed by the placeholders
	/// $c.x = c.x
	/// $c.y = c.y
	/// g({ $c.x, $c.y })
	/// </summary>
	private static void RewritePackUsages(FunctionImplementation implementation, Node root)
	{
		var placeholders = new List<KeyValuePair<Node, Node>>();

		// Direct assignments are expanded:
		var assignments = root.FindAll(i => i.Is(Operators.ASSIGN));

		for (var i = assignments.Count - 1; i >= 0; i--)
		{
			var assignment = assignments[i];

			var destination = assignment.Left;
			var source = assignment.Right;

			var type = destination.GetType();

			if (!type.IsPack)
			{
				assignments.RemoveAt(i);
				continue;
			}

			var container = assignment.Parent!;
			var position = assignment.Position!;

			var destinations = CreatePackMemberAccessors(destination, type, position);
			var sources = (List<Node>?)null;

			var is_function_assignment = assignment.Right.Is(NodeType.CALL, NodeType.FUNCTION);

			// The sources of function assignments must be replaced with placeholders, so that they do not get overriden by the local representives of the members
			if (is_function_assignment)
			{
				var loads = CreatePackMemberAccessors(destination, type, position);
				sources = new List<Node>();
				
				for (var j = 0; j < loads.Count; j++)
				{
					var placeholder = new Node();
					sources.Add(placeholder);

					placeholders.Add(new KeyValuePair<Node, Node>(placeholder, loads[j]));
				}
			}
			else
			{
				sources = CreatePackMemberAccessors(source, type, position);
			}

			for (var j = destinations.Count - 1; j >= 0; j--)
			{
				container.Insert(assignment.Next, new OperatorNode(Operators.ASSIGN, position).SetOperands(destinations[j], sources[j]));
			}

			// The assigment must be removed, if its source is not a function call
			/// NOTE: The function call assignment must be left intact, because it must assign the disposable pack handle, whose usage is demonstrated above
			if (!is_function_assignment) { assignment.Remove(); }

			assignments.RemoveAt(i);
		}

		// Pack values are replaced with pack nodes:
		// Find all local variables, which are packs
		var packs = implementation.Locals.Concat(implementation.Parameters).Concat(implementation.Variables.Values).Where(i => i.Type!.IsPack).ToList();
		foreach (var pack in packs) { Common.GetPackRepresentives(pack); }

		var usages = root.FindAll(NodeType.VARIABLE).Where(i => packs.Contains(i.To<VariableNode>().Variable)).ToList();

		for (var i = usages.Count - 1; i >= 0; i--)
		{
			var usage = usages[i];
			var type = usage.GetType();

			// Leave the function assignments intact
			if (Analyzer.IsEdited(usage)) continue;

			// Skip usages, which are used to access members
			if (usage.Parent!.Is(NodeType.LINK) && usage.Next!.Is(NodeType.VARIABLE)) continue;

			// Remove the usage from the list, because it will be replaced with a pack node
			usages.RemoveAt(i);
			
			var packer = new PackNode(type);
			foreach (var accessor in CreatePackMemberAccessors(usage, type, usage.Position!)) { packer.Add(accessor); }
			usage.Replace(packer);
		}

		// Member accessors are replaced with local variables:
		usages = root.FindAll(NodeType.VARIABLE).Where(i => packs.Contains(i.To<VariableNode>().Variable)).ToList();

		/// NOTE: All usages are used to access a member here
		foreach (var usage in usages)
		{
			// Leave the function assignments intact
			if (Analyzer.IsEdited(usage)) continue;

			/// NOTE: Add a prefix, because name could conflict with hidden variables
			var name = '.' + usage.To<VariableNode>().Variable.Name;
			var iterator = usage;
			var type = (Type?)null;

			while (iterator != null)
			{
				// The parent node must be a link, since a member access is expected
				var parent = iterator.Parent;
				if (parent == null || parent.Instance != NodeType.LINK) break;

				// Ensure the current iterator is used for member access
				var next = iterator.Next!;
				if (next.Instance != NodeType.VARIABLE) break;

				// Append the member to the name
				var member = next.To<VariableNode>().Variable;
				name += '.' + member.Name;

				iterator = parent;
				type = member.Type!;

				if (!type.IsPack) break;
			}

			if (type == null) throw new ApplicationException("Pack member did not have a type");

			// Find or create the representive for the member access
			var context = usage.To<VariableNode>().Variable.Context;
			var representive = context.GetVariable(name) ?? throw new ApplicationException("Missing pack member");

			iterator!.Replace(new VariableNode(representive, usage.Position));
		}

		// Returned packs from function calls are handled last:
		foreach (var placeholder in placeholders)
		{
			placeholder.Key.Replace(placeholder.Value);
		}
	}

	/// <summary>
	/// Rewrites nodes under the specified node to match the requirements to be analyzed and passed to the back end
	/// </summary>
	public static void Reconstruct(FunctionImplementation implementation, Node root)
	{
		StripLinks(root);
		RemoveRedundantParentheses(root);
		RemoveCancellingNegations(root);
		RemoveCancellingNots(root);
		RemoveRedundantCasts(root);
		RewriteDiscardedIncrements(root);
		ExtractExpressions(root);
		AddAssignmentCasts(root);
		RewriteSupertypeAccessors(root);
		RewriteWhenExpressions(root);
		RewriteIsExpressions(root);
		RewriteLambdaConstructions(root);
		RewriteConstructions(root);
		ExtractBoolValues(root);
		RewriteEditsAsAssignments(root);
		RewriteRemainderOperations(root);
		CastMemberCalls(root);

		if (Analysis.IsFunctionInliningEnabled)
		{
			Inlines.Build(implementation, root);
		}

		RemoveRedundantInlineNodes(root);
		RewritePackUsages(implementation, root);
	}

	/// <summary>
	/// Do some finishing touches to the nodes under the specified node
	/// </summary>
	public static void Finish(Node root)
	{
		ConstructActionOperations(root);
		RemoveRedundantInlineNodes(root);
		RewriteRemainderOperations(root);

		/// NOTE: Inline nodes can be directly under logical operators now, so outline possible booleans values which represent conditions
		ExtractBoolValues(root);
	}
}