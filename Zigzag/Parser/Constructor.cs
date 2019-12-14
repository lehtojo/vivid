using System.Collections.Generic;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context)
	{
		return new Constructor(context, AccessModifier.PUBLIC, new List<string>(), new List<Token>(), true);
	}

	public Constructor(Context context, int modifiers, List<string> parameters, List<Token> blueprint, bool @default = false) 
		: base(context, modifiers, string.Empty, parameters, blueprint)
	{
		IsDefault = @default;
		Prefix = "Constructor";
	}
}