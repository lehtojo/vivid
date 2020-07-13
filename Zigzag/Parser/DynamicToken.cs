/// <summary>
/// Represents a result produced by a pattern. 
/// A dynamic token can be thought of as an intermediate form between a token and a node, since it represents a node that can be placed among other tokens
/// </summary>
public class DynamicToken : Token
{
	public Node Node { get; private set; }

	public DynamicToken(Node node) : base(TokenType.DYNAMIC)
	{
		Node = node;
	}
}
