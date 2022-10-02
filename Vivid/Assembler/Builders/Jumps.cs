public static class Jumps
{
	/// <summary>
	/// Builds the specified jump node, while merging with its container scope
	/// </summary>
	public static Result Build(Unit unit, JumpNode node)
	{
		return new JumpInstruction(unit, node.Label).Add();
	}
}