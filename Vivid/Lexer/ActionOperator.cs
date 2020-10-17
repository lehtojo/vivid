using System;
using System.Collections.Generic;

public class ActionOperator : Operator
{
	public Operator? Operator { get; private set; }

	public ActionOperator(string identifier, Operator? operation, int priority) : base(identifier, OperatorType.ACTION, priority)
	{
		Operator = operation;
	}

	public override bool Equals(object? other)
	{
		return other is ActionOperator operation &&
			   base.Equals(other) &&
			   EqualityComparer<Operator>.Default.Equals(Operator, operation.Operator);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Operator);
	}
}