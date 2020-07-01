using System;

public static class Conditionals
{
	/// <summary>
	/// Builds the body of an if-statement or an else-if-statement
	/// </summary>
	public static Result BuildBody(Unit unit, Context local_context, Node body)
	{
		var active_variables = Scope.GetAllActiveVariablesForScope(unit, body, local_context.Parent!, local_context);

		var state = unit.GetState(unit.Position);
		var result = (Result?)null;
		
		// Since this is a body of some statement is also has a scope
		using (var scope = new Scope(unit, active_variables))
		{
			// Merges all changes that happen in the scope with the outer scope
			var merge = new MergeScopeInstruction(unit);

			// Build the body
			result = Builders.Build(unit, body);

			// Restore the state after the body
			unit.Append(merge);
		}

		unit.Set(state);

		return result;
	}

	/// <summary>
	/// Builds an if-statement or an else-if-statement
	/// </summary>
	private static Result Build(Unit unit, IfNode node, OperatorNode condition, LabelInstruction end)
	{
		var left = References.Get(unit, condition.Left);
		var right = References.Get(unit, condition.Right);

		// Compare the two operands
		var comparison = new CompareInstruction(unit, left, right).Execute();

		// Set the next label to be the end label if there's no successor since then there wont be any other comparisons
		var interphase = node.Successor == null ? end.Label : unit.GetNextLabel();

		// Jump to the next label based on the comparison
		unit.Append(new JumpInstruction(unit, comparison, (ComparisonOperator)condition.Operator, true, interphase));

		// Get the current state of the unit for later recovery
		var recovery = new SaveStateInstruction(unit);
		unit.Append(recovery);

		// Build the body of this if-statement
		var result = BuildBody(unit, node.Context, node.Body);

		// Recover the previous state
		unit.Append(new RestoreStateInstruction(unit, recovery));

		// If the if-statement body is executed it must skip the potential successors
		if (node.Successor != null)
		{
			// Skip the next successor from this if-statement's body and add the interphase label
			unit.Append(new JumpInstruction(unit, end.Label));
			unit.Append(new LabelInstruction(unit, interphase));

			// Build the successor
			return Conditionals.Build(unit, node.Successor, end);
		}

		return result;
	}

	private static Result Build(Unit unit, Node node, LabelInstruction end)
	{
		if (node is IfNode if_node)
		{
			var condition = if_node.Condition;

			if (condition.Is(NodeType.OPERATOR_NODE))
			{
				var operation = (OperatorNode)condition;

				if (operation.Operator.Type == OperatorType.COMPARISON)
				{
					return Build(unit, if_node, operation, end);
				}
			}

			throw new NotImplementedException("Complex conditional statements not implemented");
		}
		else if (node is ElseNode else_node)
		{
			return BuildBody(unit, else_node.Context, node);
		}   
		else
		{
			throw new ApplicationException("Successor of an if-statement wasn't an else-if-statement or an else-statement");
		}
	}

	public static Result Start(Unit unit, IfNode node)
	{
		Scope.PrepareConditionallyChangingConstants(unit, node);

		unit.Append(new PrepareForConditionalExecutionInstruction(unit, node.GetAllBranches()));

		var end = new LabelInstruction(unit, unit.GetNextLabel());
		var result = Build(unit, node, end);
		unit.Append(end);

		return result;
	}
}