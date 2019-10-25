public class IdentifierToken : Token
{
	public string Value { get; private set; }

	public IdentifierToken(string value) : base(TokenType.IDENTIFIER)
	{
		Value = value;
	}
}
