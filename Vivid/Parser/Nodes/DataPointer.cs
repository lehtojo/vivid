public class DataPointer : Node
{
	public object Data { get; private set; }
	public long Offset { get; private set; }

	public DataPointer(object data, long offset = 0)
	{
		Data = data;
		Offset = offset;
	}

	public override Type? TryGetType()
	{
		return Types.LINK;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.DATA_POINTER;
	}
}
