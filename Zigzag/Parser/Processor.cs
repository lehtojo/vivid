public class Processor
{
	/**
     * Connects if statements with else if or else statements
     * @param node Node to start connecting
     */
	private static void Connect(IfNode node)
	{
		Node next = node.Next;

		if (next != null)
		{
			if (next.GetNodeType() == NodeType.ELSE_IF_NODE)
			{
				Processor.Connect((IfNode)next);
				node.SetSuccessor(next);
			}
			else if (next.GetNodeType() == NodeType.ELSE_NODE)
			{
				node.SetSuccessor(next);
			}
		}
	}

	/**
     * Looks for unconnected statements and connects them
     * @param node Node tree to scan
     */
	private static void Conditionals(Node node)
	{
		Node iterator = node.First;

		while (iterator != null)
		{
			Node next = iterator.Next;

			if (iterator.GetNodeType() == NodeType.IF_NODE)
			{
				Processor.Connect((IfNode)iterator);
			}

			Processor.Conditionals(iterator);

			iterator = next;
		}
	}

	public static void Process(Node node)
	{
		Processor.Conditionals(node);
	}
}