﻿using System;

public class DecrementNode : Node
{
	public bool Post { get; private set; }
	public Node Object => First!;

	public DecrementNode(Node destination, bool post = false)
	{
		Add(destination);
		Post = post;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.DECREMENT_NODE;
	}

	public override bool Equals(object? obj)
	{
		return obj is IncrementNode node &&
			   base.Equals(obj) &&
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