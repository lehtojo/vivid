public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context)
	{
		Constructor constructor = new Constructor(context, AccessModifier.PUBLIC, true);
		constructor.SetParameters(new Node());

		return constructor;
	}

	public Constructor(Context context, int modifiers, bool @default = false) : base(context, modifiers)
	{
		IsDefault = @default;
		Prefix = "Constructor";
	}
}