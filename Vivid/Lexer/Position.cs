public class Position
{
	public int Line { get; private set; }
	public int Character { get; private set; }
	public int Local { get; private set; }
	public int Absolute { get; private set; }

	public int FriendlyLine => Line + 1;
	public int FriendlyCharacter => Character + 1;
	public int FriendlyLocal => Local + 1;
	public int FriendlyAbsolute => Absolute + 1;

	public Position(int line = 0, int character = 0, int local = 0, int absolute = 0)
	{
		Line = line;
		Character = character;
		Local = local;
		Absolute = absolute;
	}

	public Position Add(Position other)
	{
		return new Position(Line + other.Line, Character, Local, Absolute + other.Absolute);
	}

	public Position NextLine()
	{
		Line++;
		Character = 0;
		Local++;
		Absolute++;
		return this;
	}

	public Position NextCharacter()
	{
		Character++;
		Local++;
		Absolute++;
		return this;
	}

	public Position Clone()
	{
		return new Position(Line, Character, Local, Absolute);
	}
}
