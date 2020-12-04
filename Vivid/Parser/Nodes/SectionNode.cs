public class SectionNode : Node
{
	public int Modifiers { get; private set; }

	public SectionNode(int modifiers, Position? position)
	{
		Modifiers = modifiers;
		Position = position;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.SECTION;
	}
}