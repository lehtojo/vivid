using System;

public class StringToken : Token
{
	public string Text { get; set; }
	public Position End => Position.Translate(Text.Length);

	public StringToken(string text) : base(TokenType.STRING)
	{
		Text = text[1..^1];
	}

	public override bool Equals(object? other)
	{
		return other is StringToken token &&
			   base.Equals(other) &&
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

	public override string ToString()
	{
		return Lexer.STRING + Text + Lexer.STRING;
	}
}