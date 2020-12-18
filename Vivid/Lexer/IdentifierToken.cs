using System;

public class IdentifierToken : Token
{
	public string Value { get; set; }

	public IdentifierToken(string value) : base(TokenType.IDENTIFIER)
	{
		Value = value;
	}

	public IdentifierToken(string value, Position position) : base(TokenType.IDENTIFIER)
	{
		Value = value;
		Position = position;
	}

	public override bool Equals(object? obj)
	{
		return obj is IdentifierToken token &&
			   base.Equals(obj) &&
			   Value == token.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value);
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}
}
