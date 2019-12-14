using System;
using System.Collections.Generic;

public class ComparisonOperator : Operator
{
	public ComparisonOperator Counterpart { get; set; }

	public ComparisonOperator(string identifier, int priority) : base(identifier, OperatorType.COMPARISON, priority) { }

	public ComparisonOperator SetCounterpart(ComparisonOperator counterpart)
	{
		Counterpart = counterpart;
		return this;
	}

	public override bool Equals(object obj)
	{
		return obj is ComparisonOperator @operator &&
			   base.Equals(obj) &&
			   Counterpart.Identifier.Equals(@operator.Counterpart.Identifier);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Counterpart.Identifier);
	}
}