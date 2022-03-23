using System.Collections.Generic;
using System.Linq;

public class LoopUnwrapDescriptor
{
	public Variable Iterator { get; }
	public long Steps { get; }
	public List<Component> Start { get; }
	public List<Component> Step { get; }

	public LoopUnwrapDescriptor(Variable iterator, long steps, List<Component> start, List<Component> step)
	{
		Iterator = iterator;
		Steps = steps;
		Start = start;
		Step = step;
	}
}

public class LoopConditionalStatementLiftupDescriptor
{
	public IfNode Statement { get; set; }
	public List<Variable> Dependencies { get; }
	public bool IsConditionPredictable { get; set; }
	public bool IsPotentiallyLiftable => Dependencies.Any() && IsConditionPredictable;

	public LoopConditionalStatementLiftupDescriptor(IfNode statement)
	{
		Statement = statement;

		// Find all local variables, which affect the condition
		var condition = statement.Condition;

		if (condition.Instance == NodeType.VARIABLE)
		{
			var variable = condition.To<VariableNode>().Variable;

			if (variable.IsPredictable && Primitives.IsPrimitive(variable.Type, Primitives.BOOL))
			{
				Dependencies = new List<Variable>();
				Dependencies.Add(variable);
			}
			else
			{
				Dependencies = new List<Variable>();
			}
		}
		else
		{
			Dependencies = condition.FindAll(NodeType.VARIABLE).Select(i => i.To<VariableNode>().Variable).Where(i => Primitives.IsPrimitive(i.Type, Primitives.BOOL) && i.IsPredictable).ToList();
		}

		// The condition is predictable when it is not dependent on external factors such as function calls
		IsConditionPredictable = condition.Find(NodeType.CALL, NodeType.CONSTRUCTION, NodeType.FUNCTION, NodeType.LINK, NodeType.OFFSET) == null;
	}
}

public class EditorDescriptor
{
	public Node Editor { get; set; }
	public Node Edited { get; set; }
	public Variable? Local { get; set; }

	public EditorDescriptor(Node editor)
	{
		Editor = editor;
		Edited = Analyzer.GetEdited(editor);
		Local = Edited.Instance == NodeType.VARIABLE && Edited.To<VariableNode>().Variable.IsPredictable ? Edited.To<VariableNode>().Variable : null;
	}
}

public static class UnwrapmentAnalysis
{
	public const int MAXIMUM_LOOP_UNWRAP_STEPS = 100;

	/// <summary>
	/// Removes the specified conditional branch while taking into account other branches
	/// </summary>
	public static void RemoveConditionalBranch(Node branch)
	{
		if (branch.Is(NodeType.IF, NodeType.ELSE_IF))
		{
			var statement = branch.To<IfNode>().Successor;

			// If there is no successor, this statement can be removed completely
			if (statement == null)
			{
				branch.Remove();
				return;
			}

			if (statement.Instance == NodeType.ELSE_IF)
			{
				var successor = statement.To<ElseIfNode>();

				// Create a conditional statement identical to the successor but as an if-statement
				var replacement = new IfNode();
				successor.ForEach(i => replacement.Add(i));

				// Since the specified branch will not be executed, replace it with its successor
				successor.Replace(replacement);

				// Continue to execute the code below, so that the if-statement is removed
			}
			else
			{
				// Replace the specified branch with the body of the successor
				statement.ReplaceWithChildren(statement.To<ElseNode>().Body);
				return;
			}
		}

		branch.Remove();
	}

	/// <summary>
	/// Finds all statements, which do not have an effect on the flow, and removes them
	/// </summary>
	public static void RemoveAbandonedExpressions(Node root)
	{
		var statements = root.FindAll(i => i.Is(NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE, NodeType.LOOP, NodeType.INLINE, NodeType.SCOPE, NodeType.NORMAL));
		statements.Insert(0, root);
		statements.Reverse(); // Start from the very last statement

		// Contains all conditions and their node types. The node types are needed because the nodes are disabled temporarily
		var conditions = new List<KeyValuePair<Node, NodeType>>();

		// Disable all conditions, so that they are categorized as affectors
		/// NOTE: Categorizing conditions as affectors saves us from doing some node tree lookups
		foreach (var statement in statements)
		{
			if (statement.Is(NodeType.IF, NodeType.ELSE_IF))
			{
				var condition = statement.To<IfNode>().Condition;
				var type = condition.Instance;
				condition.Instance = NodeType.DISABLED;
				conditions.Add(new KeyValuePair<Node, NodeType>(condition, type));
			}
			else if (statement.Instance == NodeType.LOOP && !statement.To<LoopNode>().IsForeverLoop)
			{
				var condition = statement.To<LoopNode>().Condition;
				var type = condition.Instance;
				condition.Instance = NodeType.DISABLED;
				conditions.Add(new KeyValuePair<Node, NodeType>(condition, type));
			}
		}

		foreach (var statement in statements)
		{
			if (statement.Is(NodeType.SCOPE, NodeType.NORMAL))
			{
				var iterator = statement.First;

				while (iterator != null)
				{
					// If the iterator represents a statement, it means it contains affectors, because otherwise it would not exist (statements without affectors are removed below)
					if (iterator.Is(NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE, NodeType.LOOP, NodeType.INLINE, NodeType.SCOPE, NodeType.NORMAL))
					{
						iterator = iterator.Next;
						continue;
					}

					// Do not remove return values of scopes
					if (iterator == statement.Last && statement.Instance == NodeType.SCOPE && statement.To<ScopeNode>().IsValueReturned) break;

					// 1. If the statement does not contain any node, which has an effect on the flow (affector), it can be removed
					// 2. Remove abandoned allocations
					if ((!ReconstructionAnalysis.IsAffector(iterator) && iterator.Find(i => ReconstructionAnalysis.IsAffector(i)) == null) || (iterator.Instance == NodeType.FUNCTION && iterator.To<FunctionNode>().Function == Parser.AllocationFunction)) iterator.Remove();

					iterator = iterator.Next;
				}

				continue;
			}
			else if (statement.Is(NodeType.IF, NodeType.ELSE_IF))
			{
				// 1. The statement can not be removed, if its body is not empty
				// 2. The statement can not be removed, if it has a successor
				if (statement.To<IfNode>().Body.First != null || statement.To<IfNode>().Successor != null) continue;

				// If the condition has multiple steps, the statement can not be removed
				var step = statement.To<IfNode>().GetConditionStep();
				if (step.First != step.Last) continue;

				// If the condition contains affectors, the statement can not be removed
				if (step.Find(i => ReconstructionAnalysis.IsAffector(i)) == null) continue;

				RemoveConditionalBranch(statement);
				continue;
			}
			else if (statement.Instance == NodeType.ELSE)
			{
				// The statement can not be removed, if its body is not empty
				if (statement.To<ElseNode>().Body.First != null) continue;

				RemoveConditionalBranch(statement);
				continue;
			}
			else if (statement.Instance == NodeType.LOOP)
			{
				// TODO: Support detecting empty loops
				continue;
			}
			else if (statement.Instance == NodeType.INLINE)
			{
				// The statement can not be removed, if it is not empty
				if (statement.First != null) continue;
			}

			statement.Remove();
		}

		// Restore the condition node types
		foreach (var condition in conditions) { condition.Key.Instance = condition.Value; }
	}

	/// <summary>
	/// Find conditional statements, which can be pulled out of loops.
	/// These conditional statements usually have a condition, which is only dependent constants and local variables, which are not altered inside the loop.
	/// Example:
	/// a = random() > 0.5
	/// loop (i = 0, i < n, i++) { if a { ... } }
	/// =>
	/// a = random() > 0.5
	/// if a { loop (i = 0, i < n, i++) { ... }
	/// else { loop (i = 0, i < n, i++) { # If-statement is optimized out } }
	/// </summary>
	public static void LiftupConditionalStatementsFromLoops(Node root)
	{
		var loops = root.FindAll(i => i.Is(NodeType.LOOP));
		var conditionals = root.FindAll(i => i.Is(NodeType.IF, NodeType.ELSE_IF)).Select(i => new LoopConditionalStatementLiftupDescriptor((IfNode)i)).ToList();

		// Remove all conditions, which can not be lifted at all currently
		conditionals.RemoveAll(i => !i.IsPotentiallyLiftable);

		for (var i = 0; i < loops.Count; i++)
		{
			var loop = loops[i];
			var position = loop.Position;
			var inner_conditionals = conditionals.Where(i => i.Statement.IsUnder(loop)).ToList();
			var editors = loop.FindAll(i => i.Is(Operators.ASSIGN)).Select(i => new EditorDescriptor(i)).ToList();
			var edited = new HashSet<Variable>(editors.Where(i => i.Local != null).Select(i => i.Local!));

			for (var j = inner_conditionals.Count - 1; j >= 0; j--)
			{
				var conditional = inner_conditionals[j];

				// Now, try to find the first (local variable) dependency, which is not altered inside the loop
				var dependency = conditional.Dependencies.FirstOrDefault(i => !edited.Contains(i));
				if (dependency == null) continue;

				var environment = loop.GetParentContext();
				
				// Create a branch, which represents the situation where the dependency is true
				var condition = new Node();
				condition.Add(new VariableNode(dependency, position));
				
				var body = new ScopeNode(new Context(environment), position, null, false);
				var positive = new IfNode(environment, condition, body, position, null);

				loop.Replace(positive);

				// Clone the loop and replace all usages of the dependency with value 'true'
				var modified_loop = loop.Clone();
				
				// Process the cloned loop later
				loops.Add(modified_loop);

				var dependency_usages = modified_loop.FindAll(NodeType.VARIABLE).Where(i => i.Is(dependency)).ToList();
				var dependency_usage_replacement = new CastNode(new NumberNode(Parser.Format, 1L), new TypeNode(Primitives.CreateBool()));

				foreach (var dependency_usage in dependency_usages)
				{
					dependency_usage.Replace(dependency_usage_replacement.Clone());
				}

				positive.Body.Add(modified_loop);

				// Create a branch, which represents the situation where the dependency is false
				body = new ScopeNode(new Context(environment), position, null, false);
				var negative = new ElseNode(environment, body, position, null);

				positive.Parent!.Insert(positive.Next, negative);

				// Clone the loop and replace all usages of the dependency with value 'true'
				modified_loop = loop;

				/// NOTE: Do not add the loop, because it is being processed now

				dependency_usages = modified_loop.FindAll(NodeType.VARIABLE).Where(i => i.Is(dependency)).ToList();
				dependency_usage_replacement = new CastNode(new NumberNode(Parser.Format, 0L), new TypeNode(Primitives.CreateBool()));

				foreach (var dependency_usage in dependency_usages)
				{
					dependency_usage.Replace(dependency_usage_replacement.Clone());
				}

				negative.Body.Add(modified_loop);

				inner_conditionals.RemoveAt(j);
			}
		}
	}

	public static bool UnwrapStatements(FunctionImplementation implementation, Node root)
	{
		var unwrapped = false;
		var statements = new Queue<Node>(root.FindAll(NodeType.IF, NodeType.LOOP));

		while (statements.Any())
		{
			var iterator = statements.Dequeue();

			if (iterator.Is(NodeType.IF))
			{
				var statement = iterator.To<IfNode>();
				var condition = Analyzer.GetSource(statement.Condition);
				if (!condition.Is(NodeType.NUMBER)) continue;

				var successors = statement.GetSuccessors();

				if (!Equals(condition.To<NumberNode>().Value, 0L))
				{
					// Disconnect all the successors
					successors.ForEach(i => i.Remove());

					// Replace the conditional statement with the body
					statement.ReplaceWithChildren(statement.Body);

					unwrapped = true;
					continue;
				}

				// If there is no successor, this statement can be removed completely
				if (statement.Successor == null)
				{
					statement.Remove();
					continue;
				}

				if (statement.Successor.Is(NodeType.ELSE))
				{
					// Replace the conditional statement with the body of the successor
					statement.ReplaceWithChildren(statement.Successor.To<ElseNode>().Body);

					unwrapped = true;
					continue;
				}

				var successor = statement.Successor.To<ElseIfNode>();

				// Create a conditional statement identical to the successor but as an if-statement
				var replacement = new IfNode();
				successor.ForEach(i => replacement.Add(i));

				// Process the replacement later
				statements.Enqueue(replacement);

				// Since the statement will not be executed, replace it with its successor
				successor.Remove();
				statement.Replace(replacement);

				unwrapped = true;
			}
			else if (iterator.Is(NodeType.LOOP))
			{
				var statement = iterator.To<LoopNode>();
				if (statement.IsForeverLoop) continue;

				var condition = Analyzer.GetSource(statement.Condition);

				if (!condition.Is(NodeType.NUMBER))
				{
					if (TryUnwrapLoop(implementation, statement))
					{
						// Statements must be reloaded, since the unwrap was successful
						unwrapped = true;
						statements = new Queue<Node>(root.FindAll(NodeType.IF, NodeType.LOOP));
					}

					continue;
				}

				// NOTE: Here the condition of the loop must be a number node
				// Basically if the number node represents a non-zero value it means the loop should be reconstructed as a forever loop
				if (!Equals(condition.To<NumberNode>().Value, 0L))
				{
					// If there are nodes which are executed before the condition, insert them as well
					var initialization = statement.GetConditionInitialization();

					// Even though the loop will not be executed the initialization will be
					statement.Insert(statement.Initialization);

					if (!initialization.IsEmpty)
					{
						statement.Insert(initialization);
					}

					var replacement = new LoopNode(statement.Context, null, statement.Body, statement.Position);

					statement.Insert(replacement);
					statement.Remove();

					if (!initialization.IsEmpty)
					{
						// Reload is needed, since the condition initialization is cloned
						statements = new Queue<Node>(root.FindAll(NodeType.IF, NodeType.LOOP));
					}
				}
				else
				{
					// If there are nodes which are executed before the condition, insert them as well
					var initialization = statement.GetConditionInitialization();

					// Even though the loop will not be executed the initialization will be
					statement.Insert(statement.Initialization);

					if (!initialization.IsEmpty)
					{
						// Reload is needed, since the condition initialization is cloned
						statement.Replace(initialization);
						statements = new Queue<Node>(root.FindAll(NodeType.IF, NodeType.LOOP));
					}
					else
					{
						statement.Remove();
					}
				}

				unwrapped = true;
			}
		}

		return unwrapped;
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

		if (loop.GetConditionInitialization().Any()) return null;

		// Unwrapping loops which have loop control nodes, is currently too complex
		if (loop.FindAll(NodeType.LOOP_CONTROL).Cast<LoopControlNode>().Any(i => ReferenceEquals(i.Loop, loop))) return null;

		if (!condition.Is(OperatorType.COMPARISON) || !Analysis.IsPrimitive(condition)) return null;

		// Ensure that the initialization is empty or it contains a definition of an integer variable
		var initialization = loop.Initialization;

		if (initialization.IsEmpty || initialization.First != initialization.Last) return null;

		initialization = initialization.First!;

		if (!initialization.Is(Operators.ASSIGN) || !initialization.First!.Is(NodeType.VARIABLE)) return null;

		// Make sure the variable is predictable and it is an integer
		var variable = initialization.First!.To<VariableNode>().Variable;

		if (!variable.IsPredictable || initialization.First.To<VariableNode>().Variable.Type is not Number || !initialization.Last!.Is(NodeType.NUMBER))
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
				step_value = Analysis.CollectComponents(statement.Right);
			}
			else if (statement.Operator == Operators.ASSIGN_SUBTRACT)
			{
				step_value = Analysis.Negate(Analysis.CollectComponents(statement.Right));
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
		var left = Analysis.CollectComponents(condition.First!);

		// Abort the optimization if the comparison contains complex variable components
		// Examples (x is the iterator variable):
		// x^2 < 10
		// x < ax + 10
		if (left.Exists(c => c is VariableComponent x && x.Variable == variable && x.Order != 1 ||
			c is VariableProductComponent y && y.Variables.Exists(i => i.Variable == variable)))
		{
			return null;
		}

		var right = Analysis.CollectComponents(condition.Last!);

		if (right.Exists(c => c is VariableComponent x && x.Variable == variable && x.Order != 1 ||
			c is VariableProductComponent y && y.Variables.Exists(i => i.Variable == variable)))
		{
			return null;
		}

		// Ensure that the condition contains at least one initialization variable
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
		var range = Analysis.SimplifySubtraction(right, new List<Component> { new NumberComponent(start_value) });
		var result = Analysis.SimplifyDivision(range, step_value);

		if (result.Count != 1)
		{
			return null;
		}

		if (result.First() is NumberComponent steps)
		{
			if (steps.Value is double) return null;

			return new LoopUnwrapDescriptor(variable, (long)steps.Value, new List<Component> { new NumberComponent(start_value) }, step_value);
		}

		// If the amount of steps is not a constant, it means the length of the loop varies, therefore the loop can not be unwrapped
		return null;
	}

	public static bool TryUnwrapLoop(FunctionImplementation implementation, LoopNode loop)
	{
		var descriptor = TryGetLoopUnwrapDescriptor(loop);
		if (descriptor == null || descriptor.Steps > MAXIMUM_LOOP_UNWRAP_STEPS) return false;

		var environment = loop.GetParentContext();

		loop.InsertChildren(loop.Initialization.Clone());

		var action = ReconstructionAnalysis.TryRewriteAsAssignmentOperation(loop.Action.First!) ?? loop.Action.First!.Clone();

		for (var i = 0; i < descriptor.Steps; i++)
		{
			// Clone the body and localize its content
			var clone = loop.Body.Clone();
			Inlines.LocalizeLabels(implementation, clone);

			loop.InsertChildren(clone);

			// Clone the action and localize its content
			clone = action.Clone();
			Inlines.LocalizeLabels(implementation, clone);

			loop.Insert(action.Clone());
		}

		loop.Remove();

		return true;
	}

	public static void Start(FunctionImplementation implementation, Node root)
	{
		LiftupConditionalStatementsFromLoops(root);
		RemoveAbandonedExpressions(root);
		UnwrapStatements(implementation, root);
	}
}