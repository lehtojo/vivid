public class VariableNode : Node, Contextable
{
	public Variable Variable { get; private set; }

	public VariableNode(Variable variable)
	{
		Variable = variable;
		Variable.References.Add(this);
	}

	public Type GetContext()
	{
		return Variable.Type;
	}

	public override NodeType GetNodeType()
	{
		return NodeType.VARIABLE_NODE;
	}
}
