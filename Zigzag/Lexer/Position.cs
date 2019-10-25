public class Position
{
	public int Line { get; private set; }
	public int Character { get; private set; }
	public int Absolute { get; private set; }

	public int FriendlyLine => Line + 1;
	public int FriendlyCharacter => Character + 1;
	public int FriendlyAbsolute => Absolute + 1;

	public Position(int line = 0, int character = 0, int absolute = 0)
	{
		Line = line;
		Character = character;
		Absolute = absolute;
	}

	public static Position operator +(Position a, Position b)
	{
		return new Position(a.Line + b.Line, a.Character + b.Character, a.Absolute + b.Absolute);
	}
	
	public Position NextLine()
	{
		Line++;
		Character = 0;
		Absolute++;
		return this;
	}

	public Position NextCharacter()
	{
		Character++;
		Absolute++;
		return this;
	}
	
	public Position Clone()
	{
		return new Position(Line, Character, Absolute);
	}
}
