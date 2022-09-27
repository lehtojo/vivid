using System;

public class AssignmentOperator : Operator
{
	public Operator? Operator { get; private set; }

	public AssignmentOperator(string identifier, Operator? operation, int priority) : base(identifier, OperatorType.ASSIGNMENT, priority)
	{
		Operator = operation;
	}

	public override bool Equals(object? other)
	{
		return ReferenceEquals(this, other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Operator);
	}
}