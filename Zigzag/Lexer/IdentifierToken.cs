using System;

public class IdentifierToken : Token
{
	public string Value { get; private set; }

	public IdentifierToken(string value) : base(TokenType.IDENTIFIER)
	{
		Value = value;
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
}
