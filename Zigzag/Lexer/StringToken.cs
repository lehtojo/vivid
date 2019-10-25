public class StringToken : Token
{
	public string Text { get; private set; }

	public StringToken(string text) : base(TokenType.STRING)
	{
		Text = text.Substring(1, text.Length - 2);
	}
}