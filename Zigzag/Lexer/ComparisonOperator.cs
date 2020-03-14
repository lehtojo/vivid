using System;
using System.Collections.Generic;

public class ComparisonOperator : Operator
{
	public ComparisonOperator? Counterpart { get; set; }

	public ComparisonOperator(string identifier, int priority) : base(identifier, OperatorType.COMPARISON, priority) { }

	public ComparisonOperator SetCounterpart(ComparisonOperator counterpart)
	{
		Counterpart = counterpart;
		return this;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ComparisonOperator @operator)
		{
			var a = Counterpart?.Identifier;
			var b = @operator.Counterpart?.Identifier;

			return base.Equals(obj) && a == b;
		}

		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Counterpart?.Identifier);
	}
}