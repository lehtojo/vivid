public class NumberNode : Node, IType
{
	public Number Type { get; private set; }
	public object Value { get; set; }

	public NumberNode(NumberType type, object value)
	{
		Type = Numbers.Get(type);
		Value = value;
	}

	public new Type GetType()
	{
		return Type;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NUMBER_NODE;
	}
}
