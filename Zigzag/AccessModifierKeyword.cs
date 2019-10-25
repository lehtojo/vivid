public class AccessModifierKeyword : Keyword
{
	public int Modifier { get; private set; }

	public AccessModifierKeyword(string identifier, int modifier) : base(KeywordType.ACCESS_MODIFIER, identifier)
	{
		Modifier = modifier;
	}
}
