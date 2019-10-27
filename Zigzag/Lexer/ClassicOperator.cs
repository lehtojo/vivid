using System;

public class ClassicOperator : Operator
{
	public bool IsShared { get; private set; }

	public ClassicOperator(string identifier, int priority, bool shared = true) : base(identifier, OperatorType.CLASSIC, priority)
	{
		IsShared = shared;
	}

	public override bool Equals(object obj)
	{
		return obj is ClassicOperator @operator &&
			   base.Equals(obj) &&
			   IsShared == @operator.IsShared;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), IsShared);
	}
}