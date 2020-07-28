public class ElseNode : Node, IResolvable, IContext
{
	public Context Context { get; set; }
	public Node Body => First!;

	public ElseNode(Context context, Node body)
	{
		Context = context;
		Add(body);
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