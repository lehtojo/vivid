using System;

public class Token : ICloneable
{
	public int Type { get; private set; }
	public Position Position { get; set; } = new Position();

	public Token(int type)
	{
		Type = type;
	}

	public T To<T>() where T : Token
	{
		return (T)this ?? throw new ApplicationException($"Could not convert 'Token' to '{typeof(T).Name}'");
	}

	public override bool Equals(object? other)
	{
		return other is Token token &&
			   Type == token.Type;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Type);
	}

	public virtual object Clone()
	{
		return new Token(Type)
		{
			Position = Position.Clone()
		};
	}

	public override string ToString()
	{
		return Type == TokenType.END ? "\n" : string.Empty;
	}
}
