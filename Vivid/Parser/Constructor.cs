using System.Collections.Generic;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }
	public bool IsEmpty => IsDefault && (GetTypeParent()!.Initialization?.IsEmpty ?? true);

	public static Constructor Empty(Context context)
	{
		return new Constructor(context, AccessModifier.PUBLIC, new List<Token>(), true);
	}

	public Constructor(Context context, int modifiers, List<Token> blueprint, bool is_default = false) : base(context, modifiers, "init", blueprint)
	{
		IsDefault = is_default;
	}
}