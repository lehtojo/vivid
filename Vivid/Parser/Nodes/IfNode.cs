using System.Collections.Generic;
using System.Linq;

public class IfNode : Node, IResolvable
{
	public Context Context { get; set; }
	public Node? Successor => (Next?.Is(NodeType.ELSE_IF, NodeType.ELSE) ?? false) ? Next : null;
	public Node? Predecessor => (Is(NodeType.ELSE_IF) && (Previous?.Is(NodeType.IF, NodeType.ELSE_IF) ?? false)) ? Previous : null;

	public Node Condition => First!.Last!;
	public Node Body => Last!;

	public IfNode(Context context, Node condition, Node body)
	{
		Context = context;

		Add(new Node());
		Add(new ContextNode(Context));

		body.ForEach(i => Body.Add(i));

		First!.Add(condition);
	}

	public IfNode(Context context)
	{
		Context = context;
	}

	public IEnumerable<Node> GetConditionInitialization()
	{
		return First!.Where(i => i != Condition).ToArray();
	}

	public List<Node> GetSuccessors()
	{
		var successors = new List<Node>();
		var iterator = Successor;

		while (iterator != null)
		{
			if (iterator.Is(NodeType.ELSE_IF))
			{
				successors.Add(iterator);
				iterator = iterator.To<ElseIfNode>().Successor;
			}
			else
			{
				successors.Add(iterator);
				break;
			}
		}

		return successors;
	}

	public List<Node> GetBranches()
	{
		var branches = new List<Node>() { this };

		if (Successor == null)
		{
			return branches;
		}

		if (Successor.Is(NodeType.ELSE_IF))
		{
			branches.AddRange(Successor.To<ElseIfNode>().GetBranches());
		}
		else
		{
			branches.Add(Successor);
		}

		return branches;
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Condition);
		Resolver.Resolve(Context, Body);

		if (Successor != null)
		{
			Resolver.Resolve(context, Successor);
		}

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.IF;
	}
}