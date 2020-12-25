public class ConstructionNode : Node, IType
{
	public FunctionNode Constructor => First!.To<FunctionNode>();

	public ConstructionNode(FunctionNode constructor, Position? position = null)
	{
		Position = position;
		Add(constructor);
	}

	public new Type? GetType()
	{
		return Constructor.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONSTRUCTION;
	}
}