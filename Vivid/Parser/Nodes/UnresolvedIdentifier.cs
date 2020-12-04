using System;

public class UnresolvedIdentifier : Node, IResolvable, IType
{
	public string Value { get; private set; }

	public UnresolvedIdentifier(string value, Position position)
	{
		Value = value;
		Position = position;
	}

	public Node? GetResolvedNode(Context context)
	{
		return Singleton.GetIdentifier(context, new IdentifierToken(Value), Parent?.Is(NodeType.LINK) ?? false);
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
		return Status.Error($"Could not resolve identifier '{Value}'");
	}

	public override string ToString()
	{
		return "?";
	}

	public override bool Equals(object? other)
	{
		return other is UnresolvedIdentifier identifier &&
				base.Equals(other) &&
				Value == identifier.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value);
	}
}