public class Condition
{
	public Result Left { get; }
	public Result Right { get; }
	public ComparisonOperator Operator { get; }

	public bool IsDecimal => Left.Format.IsDecimal() || Right.Format.IsDecimal();

	public Condition(Result left, Result right, ComparisonOperator operation)
	{
		Left = left;
		Right = right;
		Operator = operation;
	}
}