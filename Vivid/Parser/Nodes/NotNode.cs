public class NotNode : Node
{
	public bool IsBitwise { get; set; }
	public Node Object => First!;

	public NotNode(Node target, bool is_bitwise, Position? position)
	{
		Position = position;
		Instance = NodeType.NOT;
		IsBitwise = is_bitwise;
		Add(target);
	}

	public override Type? TryGetType()
	{
		return Object.TryGetType();
	}

	public override string ToString() => "Not";
}