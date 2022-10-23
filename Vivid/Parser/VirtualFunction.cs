using System.Collections.Generic;

public class VirtualFunction : Function
{
	public VirtualFunction(Type type, string name, Type? return_type, Position? start, Position? end) : base(type, Modifier.DEFAULT, name, new List<Token>(), start, end)
	{
		ReturnType = return_type;
	}
}