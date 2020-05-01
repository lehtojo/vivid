using System;
using System.Collections.Generic;

public class ArrayAllocationNode : Node, IType
{
    public Type Type { get; private set; }
    public Node Length => First!;

	public ArrayAllocationNode(Type type, Node length)
	{
        Type = type;
		Add(length);
	}

    public new Type? GetType()
    {
        return Type;
    }

	public override NodeType GetNodeType()
	{
		return NodeType.ARRAY_ALLOCATION;
	}

    public override bool Equals(object? obj)
    {
        return obj is ArrayAllocationNode node &&
               base.Equals(obj) &&
               EqualityComparer<Type>.Default.Equals(Type, node.Type);
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Type);
        return hash.ToHashCode();
    }
}