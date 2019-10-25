public class Conditionals
{
	/**
     * Builds an if statement node into instructions
     * @param unit Unit used to assemble
     * @param root If statements represented in node form
     * @param end Label that is used as an exit from the if statement's body
     * @return If statement built into instructions
     */
	private static Instructions build(Unit unit, IfNode root, string end)
	{
		Instructions instructions = new Instructions();
		string next = root.Successor != null ? unit.NextLabel : end;

		Node condition = root.Condition;

		// Assemble the condition
		if (condition.GetNodeType() == NodeType.OPERATOR_NODE)
		{
			OperatorNode @operator = (OperatorNode)condition;

			Instructions jump = Comparison.Jump(unit, @operator, true, next);
			instructions.Append(jump);

			unit.Step();
		}

		Instructions successor = null;

		// Assemble potential successor
		if (root.Successor != null)
		{
			Node node = root.Successor;

			if (node.GetNodeType() == NodeType.IF_NODE)
			{
				successor = Conditionals.build(unit, (IfNode)node, end);
			}
			else
			{
				successor = unit.Assemble(node);
			}
		}

		// Clone the unit since if statements may have multiple sections that don't affect each other
		Unit clone = unit.Clone();

		Instructions body = clone.Assemble(root.Body);
		instructions.Append(body);

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
		string end = unit.NextLabel;

		Instructions instructions = Conditionals.build(unit, node, end);
		instructions.Append("{0}: ", end);

		unit.Reset();

		return instructions;
	}
}