using System;
using System.Collections.Generic;

public class ConstructionNode : Node, IResolvable, IType
{
	public Node? Parameters => Last;
	public Type Type => GetConstructor()!.GetTypeParent()!;

	public ConstructionNode(Node constructor)
	{
		Add(constructor);
	}

	public FunctionImplementation? GetConstructor()
	{
		if (!First!.Is(NodeType.FUNCTION))
		{
			return null;
		}

		return First.To<FunctionNode>().Function;
	}

	public Type? GetConstructionType()
	{
		var constructor = GetConstructor();

		if (constructor != null)
		{
			return constructor.GetTypeParent();
		}

		return Types.UNKNOWN;
	}

	public Node? Resolve(Context context)
	{
		if (First is IResolvable resolvable)
		{
			var resolved = resolvable.Resolve(context);

			if (resolved != null)
			{
				First.Replace(resolved);
			}
		}

		return null;
	}

	public new Type? GetType()
	{
		return GetConstructionType();
	}

	public override NodeType GetNodeType()
	{
		return NodeType.CONSTRUCTION;
	}

	public Status GetStatus()
	{
		return Status.OK;
	}

	public override bool Equals(object? other)
	{
		return other is ConstructionNode node &&
			   base.Equals(other) &&
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