public class ActionOperator : Operator
{
	public Operator Operator { get; private set; }

	public ActionOperator(string identifier, Operator @operator, int priority) : base(identifier, OperatorType.ACTION, priority)
	{
		Operator = @operator;
	}
}