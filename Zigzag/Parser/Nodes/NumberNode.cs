public class NumberNode : Node, IType
{
	public NumberType Type { get; private set; }
	public object Value { get; set; }

	public NumberNode(NumberType type, object value)
	{
		Type = type;
		Value = value;
	}

	public void Negate()
	{
		if (Type == NumberType.DECIMAL32)
		{
			Value = -(double)Value;
		}
		else
		{
			Value = -(long)Value;
		}
	}

	public new Type GetType()
	{
		return Numbers.Get(Type);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.NUMBER_NODE;
	}
}
