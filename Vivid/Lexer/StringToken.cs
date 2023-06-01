using System;

public class StringToken : Token
{
	public string Text { get; set; }
	public char Opening { get; set; }
	public Position End => Position.Translate(Text.Length + 2);

	public StringToken(string text) : base(TokenType.STRING)
	{
		Text = text[1..^1];
		Opening = text[0];
	}

	public override bool Equals(object? other)
	{
		return other is StringToken token && base.Equals(other) && Text == token.Text && Opening == token.Opening;
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
		return Opening.ToString() + Text + Opening;
	}
}