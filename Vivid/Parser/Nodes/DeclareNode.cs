using System;

public class DeclareNode : Node
{
	public Variable Variable { get; set; }

	public DeclareNode(Variable variable)
	{
		Variable = variable;
		Instance = NodeType.DECLARE;
	}

	public DeclareNode(Variable variable, Position? position)
	{
		Variable = variable;
		Instance = NodeType.DECLARE;
		Position = position;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Instance, Position, Variable);
	}
}