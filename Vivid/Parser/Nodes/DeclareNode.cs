public class DeclareNode : Node
{
	public Variable Variable { get; }

	public DeclareNode(Variable variable)
	{
		Variable = variable;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.DECLARE;
	}
}