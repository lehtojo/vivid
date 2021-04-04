using System.Collections.Generic;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context, Position position)
	{
		var constructor = new Constructor(context, Modifier.DEFAULT, position, true);
		constructor.Implement(new List<Type>());

		return constructor;
	}

	public Constructor(Context context, int modifiers, Position? position, bool is_default = false) : base(context, modifiers, Keywords.INIT.Identifier)
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
		var destructor = new Destructor(context, Modifier.DEFAULT, position, true);
		destructor.Implement(new List<Type>());

		return destructor;
	}

	public Destructor(Context context, int modifiers, Position? position, bool is_default = false) : base(context, modifiers, Keywords.DEINIT.Identifier)
	{
		Position = position;
		IsDefault = is_default;
	}
}