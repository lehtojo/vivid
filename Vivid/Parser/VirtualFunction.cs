using System.Collections.Generic;

public class VirtualFunction : Function
{
	public Type Type { get; private set; }
	public Type ReturnType { get; private set; }
	public int Ordinal { get; set; } = -1;
	public long Alignment => (Ordinal + 1) * Parser.Bytes;

	public VirtualFunction(Type type, string name, Type return_type) : base(type, AccessModifier.PUBLIC, name, new List<Token>()) 
	{
		Type = type;
		ReturnType = return_type;
	}

	public long GetAlignment()
	{
		return (Ordinal + 1) * Parser.Bytes;
	}
}