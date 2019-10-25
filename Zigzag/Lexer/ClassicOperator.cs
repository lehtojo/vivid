public class ClassicOperator : Operator
{
	public bool IsShared { get; private set; }

	public ClassicOperator(string identifier, int priority, bool shared = true) : base(identifier, OperatorType.CLASSIC, priority)
	{
		IsShared = shared;
	}
}