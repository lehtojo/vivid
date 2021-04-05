using System;

public enum InspectionType
{
	NAME,
	SIZE
}

public class InspectionNode : Node, IResolvable
{
	public InspectionType Type { get; private set; }
	public Node Object => First!;

	public InspectionNode(InspectionType type, Node node)
	{
		Type = type;
		Instance = NodeType.INSPECTION;

		Add(node);
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Object);
		return null;
	}

	public Status GetStatus()
	{
		var type = TryGetType();
		return type == null || type.IsUnresolved ? Status.Error(Position, "Could not resolve the type of the inspected object") : Status.OK;
	}

	public override Type? TryGetType()
	{
		return Type == InspectionType.NAME ? Types.LINK : Types.LARGE;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Type);
	}
}