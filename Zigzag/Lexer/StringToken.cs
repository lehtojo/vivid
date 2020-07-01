using System;

public class StringToken : Token
{
	public string Text { get; private set; }

	public StringToken(string text) : base(TokenType.STRING)
	{
		Text = text[1..^1];
	}

	public override bool Equals(object? obj)
	{
		return obj is StringToken token &&
			   base.Equals(obj) &&
			   Text == token.Text;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Text);
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}
}