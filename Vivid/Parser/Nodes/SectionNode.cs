using System;

public class SectionNode : Node
{
	public int Modifiers { get; private set; }

	public SectionNode(int modifiers, Position? position)
	{
		Modifiers = modifiers;
		Position = position;
		Instance = NodeType.SECTION;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Modifiers);
	}
}