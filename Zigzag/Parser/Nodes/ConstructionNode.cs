public class ConstructionNode : Node, Resolvable, Contextable
{
	public Node Parameters => Last;
	public Type Type => GetConstructor()?.GetTypeParent();

	public ConstructionNode(Node constructor)
	{
		Add(constructor);
	}

	public Constructor GetConstructor()
	{
		if (First.GetNodeType() == NodeType.FUNCTION_NODE)
		{
			FunctionNode constructor = (FunctionNode)First;
			return (Constructor)constructor.Function;
		}

		return null;
	}

	public Type GetConstructionType()
	{
		Function constructor = GetConstructor();

		if (constructor != null)
		{
			return constructor.GetTypeParent();
		}

		return null;
	}

	public Node Resolve(Context context)
	{
		if (First is Resolvable resolvable)
		{
			Node resolved = resolvable.Resolve(context);
			First.Replace(resolved);
		}

		return null;
	}

	public Type GetContext()
	{
		return GetConstructionType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONSTRUCTION_NODE;
	}
}