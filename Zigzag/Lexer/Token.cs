using System;

public class Token : ICloneable
{
	public int Type { get; private set; }
	public bool IsFirst { get; set; } = false;
	public Position Position { get; set; } = new Position();

	public Token(int type)
	{
		Type = type;
	}

	public T To<T>() where T : Token
	{
		return (T)this ?? throw new ApplicationException($"Couldn't convert 'Token' to '{typeof(T).Name}'");
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

	public virtual object Clone()
	{
		return new Token(Type)
		{
			IsFirst = IsFirst,
			Position = Position.Clone()
		};
	}
}
