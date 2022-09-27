using System;

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
		return ReferenceEquals(this, other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Counterpart?.Identifier);
	}
}