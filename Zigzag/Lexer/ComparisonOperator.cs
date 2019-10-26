public class ComparisonOperator : Operator
{
	public ComparisonOperator Counterpart { get; set; }

	public ComparisonOperator(string identifier, int priority) : base(identifier, OperatorType.COMPARISON, priority) { }

	public ComparisonOperator SetCounterpart(ComparisonOperator counterpart)
	{
		Counterpart = counterpart;
		return this;
	}
}