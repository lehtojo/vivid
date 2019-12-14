public static class Assign
{
	/// <summary>
	/// Builds assign operation
	/// </summary>
	/// <param name="unit">Unit used to operate</param>
	/// <param name="node">Assign node</param>
	/// <returns>Instructions for assign operation</returns>
	public static Instructions Build(Unit unit, OperatorNode node)
	{
		var instructions = new Instructions();

		//var references = References.Get(unit, instructions, node.Left, node.Right, ReferenceType.DIRECT, ReferenceType.VALUE);
		References.Get(unit, instructions, node.Left, node.Right, ReferenceType.DIRECT, ReferenceType.VALUE, out Reference destination, out Reference source);
		
		Memory.Move(unit, instructions, source, destination);

		if (node.Left.GetNodeType() == NodeType.VARIABLE_NODE)
		{
			var variable = (node.Left as VariableNode).Variable;
			instructions.SetReference(Value.GetVariable(source, variable));
		}

		return instructions;
	}
}