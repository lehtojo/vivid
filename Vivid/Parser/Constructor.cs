using System.Collections.Generic;
using System;

public class Constructor : Function
{
	public bool IsDefault { get; private set; }

	public static Constructor Empty(Context context, Position? start, Position? end)
	{
		return new Constructor(context, Modifier.DEFAULT, start, end, true);
	}

	public Constructor(Context context, int modifiers, Position? start, Position? end, bool is_default = false) : base(context, modifiers, Keywords.INIT.Identifier, start, end)
	{
		IsDefault = is_default;
	}

	public override FunctionImplementation Implement(IEnumerable<Type> types)
	{
		// Implement the constructor and then add the parent type initializations to the beginning of the function body
		var implementation = base.Implement(types);
		var root = implementation.Node!;
		var parent = FindTypeParent() ?? throw new ApplicationException("Missing parent type");

		for (var i = parent.Initialization.Length - 1; i >= 0; i--)
		{
			root.Insert(root.First, parent.Initialization[i]);
		}

		return implementation;
	}
}

public class Destructor : Function
{
	public bool IsDefault { get; private set; }

	public static Destructor Empty(Context context, Position? start, Position? end)
	{
		return new Destructor(context, Modifier.DEFAULT, start, end, true);
	}

	public Destructor(Context context, int modifiers, Position? start, Position? end, bool is_default = false) : base(context, modifiers, Keywords.DEINIT.Identifier, start, end)
	{
		IsDefault = is_default;
	}
}