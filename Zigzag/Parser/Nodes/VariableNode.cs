using System;
using System.Collections.Generic;

public class VariableNode : Node, IType
{
	public Variable Variable { get; set; }

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

	public override bool Equals(object? obj)
	{
		return obj is VariableNode node &&
				base.Equals(obj) &&
				EqualityComparer<Variable>.Default.Equals(Variable, node.Variable);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Variable);
	}
}
