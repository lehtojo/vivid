using System;
using System.Collections.Generic;

public class ActionOperator : Operator
{
	public Operator? Operator { get; private set; }

	public ActionOperator(string identifier, Operator? @operator, int priority) : base(identifier, OperatorType.ACTION, priority)
	{
		Operator = @operator;
	}

	public override bool Equals(object? obj)
	{
		return obj is ActionOperator @operator &&
			   base.Equals(obj) &&
			   EqualityComparer<Operator>.Default.Equals(Operator, @operator.Operator);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Operator);
	}
}