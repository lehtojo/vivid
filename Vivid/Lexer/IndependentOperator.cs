public class IndependentOperator : Operator
{
	public IndependentOperator(string identifier) : base(identifier, OperatorType.INDEPENDENT, Parser.PRIORITY_NEVER) {}
}