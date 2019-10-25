public class DynamicToken : Token
{
	public Node Node { get; private set; }

	public DynamicToken(Node node) : base(TokenType.DYNAMIC)
	{
		Node = node;
	}
}
