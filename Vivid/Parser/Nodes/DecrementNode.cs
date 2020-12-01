using System;

public class DecrementNode : Node, IType
{
	public bool Post { get; private set; }
	public Node Object => First!;

	public DecrementNode(Node destination, bool post = false)
	{
		Add(destination);
		Post = post;
	}

	public new Type? GetType()
	{
		return Object.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.DECREMENT;
	}

	public override bool Equals(object? other)
	{
		return other is IncrementNode node &&
			   base.Equals(other) &&
			   Post == node.Post;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Post);
		return hash.ToHashCode();
	}
}
