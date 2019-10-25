public class Loop
{
	private static Instructions GetForeverLoop(Unit unit, string start, Node node)
	{
		Instructions instructions = new Instructions();

		Instructions body = unit.Assemble(node);
		instructions.Append(body);

		instructions.Append("jmp {0}", start);

		unit.Reset();

		return instructions;
	}

	public static Instructions Build(Unit unit, LoopNode node)
	{
		Instructions instructions = new Instructions();

		unit.Reset();

		string start = unit.NextLabel;
		instructions.Label(start);

		if (node.IsForever)
		{
			return instructions.Append(GetForeverLoop(unit, start, node.Body));
		}

		string end = unit.NextLabel;

		Instructions condition = Comparison.Jump(unit, (OperatorNode)node.Condition, true, end);
		instructions.Append(condition);

		unit.Step();

		Instructions body = unit.Assemble(node.Body);
		instructions.Append(body);

		instructions.Append("jmp {0}", start);
		instructions.Label(end);

		unit.Reset();

		return instructions;
	}
}