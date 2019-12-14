public class ManualReference : Reference
{
	private string Identifier;
	private bool Point;

	public ManualReference(string identifier, Size size, bool point) : base(size)
	{
		Identifier = identifier;
		Point = point;
	}

	public override string Use(Size size)
	{
		return Point ? $"{size} [{Identifier}]" : Identifier;
	}

	public override string Use()
	{
		return Point ? $"[{Identifier}]" : Identifier;
	}

	public override bool IsComplex()
	{
		return Point;
	}

	public override LocationType GetType()
	{
		return LocationType.MANUAL;
	}

	public static ManualReference String(string name, bool write)
	{
		return new ManualReference(name, Size.DWORD, write);
	}

	public static ManualReference Label(string name)
	{
		return new ManualReference(name, Size.DWORD, false);
	}

	public static ManualReference Global(string name, Size size)
	{
		return new ManualReference(name, size, true);
	}

	public static ManualReference Number(Number value, Size size)
	{
		return new ManualReference(value.ToString(), size, false);
	}

	public override bool Equals(object? obj)
	{
		return obj is ManualReference reference &&
			   Identifier == reference.Identifier &&
			   Point == reference.Point;
	}
}