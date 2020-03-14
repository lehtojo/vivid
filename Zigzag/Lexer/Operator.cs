using System;

public class Operator
{
	public string Identifier { get; private set; }
	public OperatorType Type { get; private set; }
	public int Priority { get; private set; }

	public Operator(string identifier, OperatorType type, int priority)
	{
		Identifier = identifier;
		Type = type;
		Priority = priority;
	}

	public override bool Equals(object? obj)
	{
		return obj is Operator @operator &&
			   Identifier == @operator.Identifier &&
			   Type == @operator.Type &&
			   Priority == @operator.Priority;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Identifier, Type, Priority);
	}
}