public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context, Position position)
	{
		return new Constructor(context, Modifier.PUBLIC, position, true);
	}

	public Constructor(Context context, int modifiers, Position position, bool is_default = false) : base(context, modifiers, Keywords.INIT.Identifier)
	{
		Position = position;
		IsDefault = is_default;
	}
}

public class Destructor : Function
{
	public bool IsDefault { get; private set; }

	public static Destructor Empty(Context context, Position position)
	{
		return new Destructor(context, Modifier.PUBLIC, position, true);
	}

	public Destructor(Context context, int modifiers, Position position, bool is_default = false) : base(context, modifiers, Keywords.DEINIT.Identifier)
	{
		Position = position;
		IsDefault = is_default;
	}
}