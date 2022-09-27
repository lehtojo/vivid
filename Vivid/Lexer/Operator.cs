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

	public T To<T>() where T : Operator
	{
		return (T)this;
	}

	public override bool Equals(object? other)
	{
		return other is Operator operation &&
			   Identifier == operation.Identifier &&
			   Type == operation.Type &&
			   Priority == operation.Priority;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Identifier, Type, Priority);
	}
}