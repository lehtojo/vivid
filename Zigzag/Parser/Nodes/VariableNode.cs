public class VariableNode : Node, IType
{
	public Variable Variable { get; private set; }

	public VariableNode(Variable variable)
	{
		Variable = variable;
		Variable.References.Add(this);
	}

	public new Type? GetType()
	{
		return Variable.Type;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.VARIABLE_NODE;
	}
}
