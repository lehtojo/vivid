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

			var assignment_context = new Context(expression_context);

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
	/// Rewrites construction expressions so that they use nodes which can be compiled
	/// </summary>
	private static void RewriteConstructionExpressions(Node root)
	{
		var constructions = root.FindAll(i => i.Is(NodeType.CONSTRUCTION)).Cast<ConstructionNode>();

		foreach (var construction in constructions)
		{
			var editor = Analyzer.TryGetEditor(construction);

			if (editor == null)
			{
				continue;
			}

			var edited = Analyzer.GetEdited(editor);

			if (!edited.Is(NodeType.VARIABLE))
			{
				continue;
			}

			var variable = edited.To<VariableNode>().Variable;

			if (!variable.IsInlined || !variable.IsPredictable)
			{
				continue;
			}

			var replacement = Common.CreateStackConstruction(construction.Constructor);
			construction.Replace(replacement);
		}

		constructions = root.FindAll(i => i.Is(NodeType.CONSTRUCTION)).Cast<ConstructionNode>();

		foreach (var construction in constructions)
		{
			var replacement = Common.CreateHeapConstruction(construction.Constructor);
			construction.Replace(replacement);
		}
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

			var context = new Context(environment);

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
		}
	}

	public static bool IsValueUsed(Node value)
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
			var environment = expression.GetParentContext();
			var inline = new ContextInlineNode(new Context(environment), expression.Position);

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

	public static void LiftupInlineNodes(Node root)
	{
		var inlines = root.FindAll(i => i.Is(NodeType.INLINE));

		foreach (var inline in inlines)
		{
			while (true)
			{
				var parent = inline.Parent!;

				/// TODO: Casting is a problem
				if (!parent.Is(NodeType.OPERATOR) || inline.Last == null)
				{
					break;
				}

				Node? operation;
				var value = inline.Last;

				if (parent.First == inline)
				{
					operation = parent.Clone();
					operation.RemoveChildren();

					inline.Remove(value);

					operation.Add(value);
					operation.Add(parent.Last!);

					inline.Add(operation);

					parent.Replace(inline);
					continue;
				}

				/// TODO: It's possible the continue but the execution order must be preserved using hidden variables
				if (!Analysis.IsPrimitive(parent.First!))
				{
					break;
				}

				operation = parent.Clone();
				operation.RemoveChildren();

				inline.Remove(value);

				operation.Add(parent.First!);
				operation.Add(value);

				inline.Add(operation);

				parent.Replace(inline);
			}
		}
	}

	private static void RewriteRemainderOperation(OperatorNode remainder)
	{
		var environment = remainder.GetParentContext();
		var inline = new ContextInlineNode(new Context(environment), remainder.Position);

		var a = inline.Context.DeclareHidden(remainder.Left.GetType());
		var b = inline.Context.DeclareHidden(remainder.Right.GetType());

		inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(a),
			remainder.Left.Clone()
		));

		inline.Add(new OperatorNode(Operators.ASSIGN).SetOperands(
			new VariableNode(b),
			remainder.Right.Clone()
		));

		// Formula: a % b = a - (a / b) * b
		inline.Add(new OperatorNode(Operators.SUBTRACT).SetOperands(
			new VariableNode(a),
			new OperatorNode(Operators.MULTIPLY).SetOperands(
				new OperatorNode(Operators.DIVIDE).SetOperands(
					new VariableNode(a),
					new VariableNode(b)
				),
				new VariableNode(b)
			)
		));

		remainder.Replace(inline);
	}

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
			if (!link.Left.Is(NodeType.TYPE))
			{
				continue;
			}

			if (link.Right.Is(NodeType.FUNCTION))
			{
				var function = link.Right.To<FunctionNode>();

				if (!function.Function.IsMember)
				{
					continue;
				}

				var self = link.GetParentContext().GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

				link.Left.Replace(new VariableNode(self, link.Left.Position));
			}
			else if (link.Right.Is(NodeType.VARIABLE))
			{
				var variable = link.Right.To<VariableNode>();

				if (!variable.Variable.IsMember)
				{
					continue;
				}

				var self = link.GetParentContext().GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

				link.Left.Replace(new VariableNode(self, link.Left.Position));
			}
		}
	}

	public static void Reconstruct(Node root)
	{
		RemoveRedundantParenthesis(root);
		RemoveCancellingNegations(root);
		RemoveCancellingNots(root);
		RewriteSupertypeAccessors(root);
		RewriteIsExpressions(root);
		RewriteConstructionExpressions(root);
		OutlineBooleanValues(root);
		RewriteDiscardedIncrements(root);
		RewriteIncrements(root);
		RewriteAllEditsAsAssignOperations(root);
		SubstituteInlineNodes(root);
		//LiftupInlineNodes(root);
		RewriteRemainderOperations(root);
		
		if (Analysis.IsFunctionInliningEnabled)
		{
			//Inlines.Build(root);
		}
	}

	public static void Finish(Node root)
	{
		ConstructActionOperations(root);
		SubstituteInlineNodes(root);
	}
}