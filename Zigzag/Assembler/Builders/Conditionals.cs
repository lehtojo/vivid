public class Conditionals
{
	/**
     * Builds an if statement node into instructions
     * @param unit Unit used to assemble
     * @param root If statements represented in node form
     * @param end Label that is used as an exit from the if statement's body
     * @return If statement built into instructions
     */
	private static Instructions Build(Unit unit, IfNode root, string end)
	{
		var instructions = new Instructions();
		var next = root.Successor != null ? unit.NextLabel : end;

		var condition = root.Condition;

		// Assemble the condition
		if (condition.GetNodeType() == NodeType.OPERATOR_NODE)
		{
			var operation = (OperatorNode)condition;

			var jump = Comparison.Jump(unit, operation, true, next);
			instructions.Append(jump);

			//unit.Step(instructions);
		}

		Instructions? successor = null;
		var clone = unit.Clone();

		// Assemble potential successor
		if (root.Successor != null)
		{
			var node = root.Successor;

			if (node.GetNodeType() == NodeType.ELSE_IF_NODE)
			{
				successor = Conditionals.Build(clone, (IfNode)node, end);
			}
			else
			{
				successor = clone.Assemble(node);
			}
		}

		// Clone the unit since if statements may have multiple sections that don't affect each other
		clone = unit.Clone();

		var body = clone.Assemble(root.Body);
		instructions.Append(body);

		clone.Stack.Restore(instructions);

		// Merge all assembled sections together
		if (successor != null)
		{
			instructions.Append("jmp {0}", end);
			instructions.Label(next);
			instructions.Append(successor);
		}

		return instructions;
	}

	/**
     * Builds an if statement into instructions
     * @param unit Unit used to assemble
     * @param node If statement represented in node form
     * @return If statement built into instructions
     */
	public static Instructions start(Unit unit, IfNode node)
	{
		var end = unit.NextLabel;

		var instructions = Conditionals.Build(unit, node, end);
		instructions.Append("{0}: ", end);

		unit.Reset();

		return instructions;
	}
}