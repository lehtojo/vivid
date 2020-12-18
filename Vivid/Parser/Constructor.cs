using System.Collections.Generic;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context, Position position)
	{
		return new Constructor(context, AccessModifier.PUBLIC, new List<Token>(), position, true);
	}

	public Constructor(Context context, int modifiers, List<Token> blueprint, Position position, bool is_default = false) : base(context, modifiers, Keywords.INIT.Identifier, blueprint)
	{
		Position = position;
		IsDefault = is_default;
	}
}