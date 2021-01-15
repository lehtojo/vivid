public class ConstructionNode : Node
{
	public FunctionNode Constructor => First!.To<FunctionNode>();

	public ConstructionNode(FunctionNode constructor, Position? position = null)
	{
		Position = position;
		Add(constructor);
	}

	public override Type? TryGetType()
	{
		return Constructor.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONSTRUCTION;
	}
}