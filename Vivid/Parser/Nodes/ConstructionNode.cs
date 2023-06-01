public class ConstructionNode : Node, IResolvable
{
	public Node Allocator => First!;
	public FunctionNode Constructor => Last!.To<FunctionNode>();
	public bool HasAllocator => First != Last;

	public bool IsStackAllocated { get; set; } = false;

	public ConstructionNode(FunctionNode constructor, Position? position = null)
	{
		Position = position;
		Instance = NodeType.CONSTRUCTION;
		Add(constructor);
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Constructor);
		return null;
	}

	public override Type? TryGetType()
	{
		return Constructor.Function.FindTypeParent();
	}

	public Status GetStatus()
	{
		var type = TryGetType();
		if (type == null) return Status.OK;

		if (type.IsStatic) return new Status(Position, "Namespaces can not be created as objects");
		if (type.IsTemplateType && !type.IsTemplateTypeVariant) return new Status(Position, "Can not create template type without template arguments");

		return Status.OK;
	}

	public override string ToString() => "Construction";
}