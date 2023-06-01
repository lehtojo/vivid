using System;

public class IncrementNode : Node, IResolvable
{
	public bool Post { get; private set; }
	public Node Object => First!;

	public IncrementNode(Node destination, Position? position, bool post = false)
	{
		Add(destination);
		Position = position;
		Post = post;
		Instance = NodeType.INCREMENT;
	}

	public override Type? TryGetType()
	{
		return Object.TryGetType();
	}

	public Node? Resolve(Context context)
	{
		Resolver.Resolve(context, Object);
		return null;
	}

	public Status GetStatus()
	{
		// Ensure the object is a number
		return TryGetType() is Number ? Status.OK : new Status(Position, "Can not resolve the increment operation");
	}

	public override bool Equals(object? other)
	{
		return other is IncrementNode node &&
				base.Equals(other) &&
				Post == node.Post;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Post);
	}

	public override string ToString() => "Increment";
}
