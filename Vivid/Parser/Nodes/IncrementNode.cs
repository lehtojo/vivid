using System;

public class IncrementNode : Node, IType
{
	public bool Post { get; private set; }
	public Node Object => First!;

	public IncrementNode(Node destination, bool post = false)
	{
		Add(destination);
		Post = post;
	}

	public Type? GetType()
	{
		return Object.TryGetType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.INCREMENT;
	}

	public override bool Equals(object? other)
	{
		return other is IncrementNode node &&
			   base.Equals(other) &&
			   Post == node.Post;
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Post);
		return hash.ToHashCode();
	}
}
