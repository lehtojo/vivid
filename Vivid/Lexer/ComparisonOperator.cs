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

	public override bool Equals(object? other)
	{
		if (other is ComparisonOperator operation)
		{
			var a = Counterpart?.Identifier;
			var b = operation.Counterpart?.Identifier;

			return base.Equals(other) && a == b;
		}

		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Counterpart?.Identifier);
	}
}