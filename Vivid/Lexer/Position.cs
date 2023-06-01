public class Position
{
	public SourceFile? File { get; set; }
	public int Line { get; private set; }
	public int Character { get; private set; }
	public int Local { get; private set; }
	public int Absolute { get; private set; }
	public bool IsCursor { get; set; } = false;

	public int FriendlyLine => Line + 1;
	public int FriendlyCharacter => Character + 1;
	public int FriendlyLocal => Local + 1;
	public int FriendlyAbsolute => Absolute + 1;

	public Position(SourceFile? file, int line, int character)
	{
		File = file;
		Line = line;
		Character = character;
		Local = 0;
		Absolute = 0;
	}

	public Position(SourceFile? file = null, int line = 0, int character = 0, int local = 0, int absolute = 0, bool cursor = false)
	{
		File = file;
		Line = line;
		Character = character;
		Local = local;
		Absolute = absolute;
		IsCursor = cursor;
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

	public Position Translate(int characters)
	{
		return new Position(File, Line, Character + characters, Local + characters, Absolute + characters, IsCursor);
	}

	public Position Clone()
	{
		return new Position(File, Line, Character, Local, Absolute, IsCursor);
	}
}
