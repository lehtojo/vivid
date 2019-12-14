using System;

public class IfNode : Node
{
	public Context Context { get; set; }
	public Node Successor { get; private set; }

	public Node Condition => First;
	public Node Body => Last;

	public IfNode(Context context, Node condition, Node body)
	{
		Context = context;

		Add(condition);
		Add(body);
	}

	public void AddSuccessor(Node successor)
	{
		if (Successor == null)
		{
			Successor = successor;
			Insert(Last, Successor);
		}
		else if (Successor is IfNode node)
		{
			node.AddSuccessor(successor);
		}
		else
		{
			throw new Exception("Couldn't add successor to a (else) if node");
		}
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IF_NODE;
	}
}