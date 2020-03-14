using System;

public class Token
{
	public int Type { get; private set; }
	public Position Position { get; set; } = new Position();

	public Token(int type)
	{
		Type = type;
	}

	public override bool Equals(object? obj)
	{
		return obj is Token token &&
			   Type == token.Type;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}
}
