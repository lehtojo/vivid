public class Assign
{
	private static bool isVariable(Node node)
	{
		return node.GetNodeType() == NodeType.VARIABLE_NODE;
	}

	/**
     * Builds an assign operation into instructions
     * @param node Assign node
     * @return Assign instructions
     */
	public static Instructions build(Unit unit, OperatorNode node)
	{
		Instructions instructions = new Instructions();

		Instructions right = References.Value(unit, node.Right);
		Instructions left = References.Direct(unit, node.Left);

		instructions.Append(right, left);
		instructions.Append(new Instruction("mov", left.Reference, right.Reference, left.Reference.GetSize()));

		if (isVariable(node.Left))
		{

			Variable variable = ((VariableNode)node.Left).Variable;
			instructions.SetReference(Value.GetVariable(right.Reference, variable));
		}

		return instructions;
	}
}