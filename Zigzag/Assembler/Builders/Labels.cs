public class Labels
{
	private const string PREFIX = "_label_";

	public static Instructions Build(Unit unit, LabelNode node)
	{
		Instructions instructions = new Instructions();

		Label label = node.Label;
		string fullname = unit.Prefix + PREFIX + label.Name;

		return instructions.Label(fullname);
	}

	public static Instructions Build(Unit unit, JumpNode node)
	{
		Instructions instructions = new Instructions();

		Label label = node.Label;
		string fullname = unit.Prefix + PREFIX + label.Name;

		return instructions.Append("jmp {0}", fullname);
	}
}