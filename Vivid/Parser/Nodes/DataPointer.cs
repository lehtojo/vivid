using System;

public class DataPointerNode : Node
{
	public object Data { get; private set; }
	public long Offset { get; private set; }

	public DataPointerNode(object data, long offset = 0, Position? position = null)
	{
		Data = data;
		Offset = offset;
		Instance = NodeType.DATA_POINTER;
		Position = position;
	}

	public override Type TryGetType()
	{
		return Link.GetVariant(Primitives.CreateNumber(Primitives.LARGE, Format.INT64));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Data, Offset);
	}

	public override string ToString() => $"Data Pointer ({Data}+{Offset})";
}
