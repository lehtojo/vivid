using System;

public class UnresolvedIdentifier : Node, IResolvable, IType
{
	public string Value { get; private set; }

	public UnresolvedIdentifier(string value)
	{
		Value = value;
	}

	public Node? GetResolvedNode(Context context)
	{
		if (context.IsTypeDeclared(Value))
		{
			return new TypeNode(context.GetType(Value)!);
		}
		else if (context.IsVariableDeclared(Value))
		{
			return new VariableNode(context.GetVariable(Value)!);
		}
		else
		{
			return null;
		}
	}

	public Node? Resolve(Context context)
	{
		return GetResolvedNode(context);
	}

	public new Type? GetType()
	{
		return Types.UNKNOWN;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.UNRESOLVED_IDENTIFIER;
	}

	public Status GetStatus()
	{
		return Status.Error($"Couldn't resolve identifier '{Value}'");
	}

	public override bool Equals(object? obj)
	{
		return obj is UnresolvedIdentifier identifier &&
				base.Equals(obj) &&
				Value == identifier.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value);
	}
}