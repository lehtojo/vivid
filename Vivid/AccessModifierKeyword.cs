public class AccessModifierKeyword : Keyword
{
	public int Modifier { get; }

	public AccessModifierKeyword(string identifier, int modifier) : base(KeywordType.ACCESS_MODIFIER, identifier)
	{
		Modifier = modifier;
	}
}
