public class LoopNode : Node
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

	public override NodeType GetNodeType()
	{
		return NodeType.LOOP_NODE;
	}
}
