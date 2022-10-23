using System;
using System.Collections.Generic;
using System.Linq;

public class TypeNode : Node, IResolvable
{
	public Type Type { get; private set; }

	public TypeNode(Type type)
	{
		Type = type;
		Instance = NodeType.TYPE;
	}

	public TypeNode(Type type, Position? position)
	{
		Type = type;
		Position = position;
		Instance = NodeType.TYPE;
	}

	public Node? Resolve(Context context)
	{
		if (Type.IsResolved()) return null;

		var replacement = Resolver.Resolve(context, Type);
		if (replacement == null) return null;

		Type = replacement;
		return null;
	}

	public override Type TryGetType()
	{
		return Type;
	}

	public override bool Equals(object? other)
	{
		return other is TypeNode node && base.Equals(other) && EqualityComparer<Type>.Default.Equals(Type, node.Type);
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Type.Identity);
		return hash.ToHashCode();
	}

	public Status GetStatus()
	{
		if (Parent!.Is(NodeType.COMPILES, NodeType.INSPECTION, NodeType.LINK) || (Parent.Instance == NodeType.CAST && Next == null))
		{
			return Status.OK;
		}

		return Status.Error(Position, "Can not understand");
	}

	public override string ToString() => $"Type {Type}";
}
