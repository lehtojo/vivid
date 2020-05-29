public class LoopNode : Node, IResolvable
{	
	public Context StepsContext { get; private set; }
	public Context BodyContext { get; private set; }
	public Node Initialization => First!.First!;
	public Node Condition => Initialization!.Next!;
	public Node Action => First!.Last!;
	public Node Body => Last!;
	public Label? Exit { get; set; } = null;

	public bool IsForeverLoop => First == Last;

	public LoopNode(Context steps_context, Context body_context, Node? steps, Node body)
	{
		StepsContext = steps_context;
		BodyContext = body_context;

		if (steps != null)
		{
			Add(steps);
		}

		Add(body);
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(StepsContext, Initialization);
		Resolver.Resolve(StepsContext, Condition);
		Resolver.Resolve(StepsContext, Action);
		Resolver.Resolve(BodyContext, Body);

		return null;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.LOOP_NODE;
	}
}
