public enum KeywordType
{
	MODIFIER,
	FLOW,
	NORMAL
}

public class Keyword
{
	public KeywordType Type { get; }
	public string Identifier { get; }

	public Keyword(string identifier, KeywordType type = KeywordType.NORMAL) : this(type, identifier) { }

	public Keyword(KeywordType type, string identifier)
	{
		Type = type;
		Identifier = identifier;
	}

	public T To<T>() where T : Keyword
	{
		return (T)this;
	}
}