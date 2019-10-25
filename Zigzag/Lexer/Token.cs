public class Token
{
	public int Type { get; private set; }
	public Position Position { get; set; }

	public Token(int type)
	{
		Type = type;
	}
}
