public class Operator
{
	public string Identifier { get; private set; }
	public OperatorType Type { get; private set; }
	public int Priority { get; private set; }

	public Operator(string identifier, OperatorType type, int priority)
	{
		Identifier = identifier;
		Type = type;
		Priority = priority;
	}
}