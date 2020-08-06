using System.Collections.Generic;

public class ElseNode : Node, IResolvable, IContext
{
	public Node? Predecessor => (Previous?.Is(NodeType.IF_NODE, NodeType.ELSE_IF_NODE) ?? false) ? Previous : null;

	public Context Context { get; set; }
	public Node Body => First!;

	public ElseNode(Context context, Node body)
	{
		Context = context;
		Add(body);
	}

	public IfNode GetRoot()
	{
		var iterator = Predecessor;

		while (!iterator!.Is(NodeType.IF_NODE))
		{
			iterator = iterator.To<ElseIfNode>().Predecessor;
		}

		return iterator.To<IfNode>();
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(Context, Body);

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.ELSE_NODE;
	}

   public Context GetContext()
   {
		return Context;
   }
}