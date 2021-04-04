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

public static class UnwrapmentAnalysis
{
	public const int MAXIMUM_LOOP_UNWRAP_STEPS = 100;
	
	public static bool UnwrapStatements(Node root)
	{
		var unwrapped = false;
		var statements = new Queue<Node>(root.FindAll(i => i.Is(NodeType.IF, NodeType.LOOP)));

		while (statements.Any())
		{
			var iterator = statements.Dequeue();

			if (iterator.Is(NodeType.IF))
			{
				var statement = iterator.To<IfNode>();

				if (!statement.Condition.Is(NodeType.NUMBER))
				{
					continue;
				}

				var successors = statement.GetSuccessors();

				if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
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

				if (statement.IsForeverLoop)
				{
					continue;
				}

				if (!statement.Condition.Is(NodeType.NUMBER))
				{
					if (TryUnwrapLoop(statement))
					{
						// Statements must be reloaded, since the unwrap was successful
						unwrapped = true;
						statements = new Queue<Node>(root.FindAll(i => i.Is(NodeType.IF, NodeType.LOOP)));
					}

					continue;
				}

				// NOTE: Here the condition of the loop must be a number node
				// Basically if the number node represents a non-zero value it means the loop should be reconstructed as a forever loop
				if (!Equals(statement.Condition.To<NumberNode>().Value, 0L))
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
						statements = new Queue<Node>(root.FindAll(i => i.Is(NodeType.IF, NodeType.LOOP)));
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
						statements = new Queue<Node>(root.FindAll(i => i.Is(NodeType.IF, NodeType.LOOP)));
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

		if (loop.GetConditionInitialization().Any())
		{
			return null;
		}

		if (!condition.Is(OperatorType.COMPARISON) || !Analysis.IsPrimitive(condition))
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

	public static bool TryUnwrapLoop(LoopNode loop)
	{
		var descriptor = TryGetLoopUnwrapDescriptor(loop);

		if (descriptor == null || descriptor.Steps > MAXIMUM_LOOP_UNWRAP_STEPS)
		{
			return false;
		}

		var environment = loop.GetParentContext();

		loop.InsertChildren(loop.Initialization.Clone());

		var action = ReconstructionAnalysis.TryRewriteAsAssignOperation(loop.Action.First!) ?? loop.Action.First!.Clone();

		for (var i = 0; i < descriptor.Steps; i++)
		{
			// Clone the body and localize its content
			var clone = loop.Body.Clone();
			Inlines.LocalizeLabels(environment, clone);

			loop.InsertChildren(clone);

			// Clone the action and localize its content
			clone = action.Clone();
			Inlines.LocalizeLabels(environment, clone);

			loop.Insert(action.Clone());
		}

		loop.Remove();

		return true;
	}
}