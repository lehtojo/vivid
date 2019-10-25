using System;

public class UnresolvedIdentifier : Node, Resolvable
{
	private string Value { get; set; }

	public UnresolvedIdentifier(string value)
	{
		Value = value;
	}

	public Node GetResolvedNode(Context context)
	{
		if (context.IsTypeDeclared(Value))
		{
			return new TypeNode(context.GetType(Value));
		}
		else if (context.IsVariableDeclared(Value))
		{
			return new VariableNode(context.GetVariable(Value));
		}
		else
		{
			throw new Exception($"Couldn't resolve identifier '{Value}'");
		}
	}

	public Node Resolve(Context context)
	{
		return GetResolvedNode(context);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.UNRESOLVED_IDENTIFIER;
	}
}