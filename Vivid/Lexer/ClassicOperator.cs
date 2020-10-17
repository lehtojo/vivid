using System;

public class ClassicOperator : Operator
{
	public bool IsShared { get; private set; }

	public ClassicOperator(string identifier, int priority, bool shared = true) : base(identifier, OperatorType.CLASSIC, priority)
	{
		IsShared = shared;
	}

	public override bool Equals(object? other)
	{
		return other is ClassicOperator operation &&
			   base.Equals(other) &&
			   IsShared == operation.IsShared;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), IsShared);
	}
}