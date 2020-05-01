using System;
using System.Collections.Generic;

public class UnresolvedIdentifier : Node, IResolvable
{
	private string Value { get; set; }

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