using System.Collections.Generic;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context, Position? start, Position? end)
	{
		var constructor = new Constructor(context, Modifier.DEFAULT, start, end, true);
		constructor.Implement(new List<Type>());

		return constructor;
	}

	public Constructor(Context context, int modifiers, Position? start, Position? end, bool is_default = false) : base(context, modifiers, Keywords.INIT.Identifier, start, end)
	{
		IsDefault = is_default;
	}
}

public class Destructor : Function
{
	public bool IsDefault { get; private set; }

	public static Destructor Empty(Context context, Position? start, Position? end)
	{
		var destructor = new Destructor(context, Modifier.DEFAULT, start, end, true);
		destructor.Implement(new List<Type>());

		return destructor;
	}

	public Destructor(Context context, int modifiers, Position? start, Position? end, bool is_default = false) : base(context, modifiers, Keywords.DEINIT.Identifier, start, end)
	{
		IsDefault = is_default;
	}
}