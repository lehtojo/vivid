using System;

public class Keyword
{
	public KeywordType Type { get; }
	public string Identifier { get; }

	public Keyword(string identifier) : this(KeywordType.NORMAL, identifier) { }

	public Keyword(KeywordType type, string identifier)
	{
		Type = type;
		Identifier = identifier;
	}

	public T To<T>() where T : Keyword
	{
		return (T)this ?? throw new ApplicationException($"Could not convert 'Keyword' to '{typeof(T).Name}'");
	}
}
