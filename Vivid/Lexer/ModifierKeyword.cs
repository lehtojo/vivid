public class ModifierKeyword : Keyword
{
	public int Modifier { get; }

	public ModifierKeyword(string identifier, int modifier) : base(KeywordType.MODIFIER, identifier)
	{
		Modifier = modifier;
	}
}
