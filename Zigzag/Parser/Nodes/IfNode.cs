using System.Collections.Generic;
using System.Linq;

public class IfNode : Node, IResolvable, IContext
{
	public Context Context { get; set; }
	public Node? Successor => (Next?.Is(NodeType.ELSE_IF_NODE, NodeType.ELSE_NODE) ?? false) ? Next : null;
	public Node? Predecessor => (Is(NodeType.ELSE_IF_NODE) && (Previous?.Is(NodeType.IF_NODE, NodeType.ELSE_IF_NODE) ?? false)) ? Previous : null;

	public Node Condition => First!.Last!;
	public Node Body => Last!;

	public IfNode(Context context, Node condition, Node body)
	{
		Context = context;

		Add(new Node());
		Add(body);

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
			if (iterator.Is(NodeType.ELSE_IF_NODE))
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
		var branches = new List<Node>() { Body };

		if (Successor == null)
		{
			return branches;
		}
		
		if (Successor.Is(NodeType.ELSE_IF_NODE))
		{
			branches.AddRange(Successor.To<ElseIfNode>().GetBranches());
		}
		else
		{
			branches.Add(Successor.To<ElseNode>().Body);
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
		return NodeType.IF_NODE;
	}

	public void SetContext(Context context)
	{
		Context = context;
	}

   public Context GetContext()
   {
		return Context;
   }
}