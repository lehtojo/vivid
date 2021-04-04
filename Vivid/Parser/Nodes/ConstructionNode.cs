public class ConstructionNode : Node
{
	public FunctionNode Constructor => First!.To<FunctionNode>();

	public ConstructionNode(FunctionNode constructor, Position? position = null)
	{
		Position = position;
		Instance = NodeType.CONSTRUCTION;
		Add(constructor);
	}

	public override Type? TryGetType()
	{
		return Constructor.Function.GetTypeParent();
	}
}