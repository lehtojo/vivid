using System;

public class DataPointer : Node
{
	public object Data { get; private set; }
	public long Offset { get; private set; }

	public DataPointer(object data, long offset = 0)
	{
		Data = data;
		Offset = offset;
		Instance = NodeType.DATA_POINTER;
	}

	public override Type TryGetType()
	{
		return Link.GetVariant(Primitives.CreateNumber(Primitives.LARGE, Format.INT64));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Data, Offset);
	}
}
