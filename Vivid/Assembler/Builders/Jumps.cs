public static class Jumps
{
	/// <summary>
	/// Builds the specified jump node, while merging with its container scope
	/// </summary>
	public static Result Build(Unit unit, JumpNode node)
	{
		//var scope = FindContainerScope(unit, node);

		// Build the jump condition, if one is present
		if (node.Condition != null) Arithmetic.BuildCondition(unit, node.Condition);

		unit.Append(new LabelMergeInstruction(unit, node.Label));

		if (node.Condition != null) 
		{
			return new JumpInstruction(unit, node.Condition.Operator, false, !node.Condition.IsDecimal, node.Label).Execute();
		}

		return new JumpInstruction(unit, node.Label).Execute();
	}
}