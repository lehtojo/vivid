using System;
using System.Collections.Generic;

public class VariableNode : Node
{
	public Variable Variable { get; set; }

	public VariableNode(Variable variable)
	{
		Variable = variable;
		Instance = NodeType.VARIABLE;
		Variable.Usages.Add(this);
	}

	public VariableNode(Variable variable, Position? position)
	{
		Variable = variable;
		Instance = NodeType.VARIABLE;
		Position = position;
		Variable.Usages.Add(this);
	}

	public override Type? TryGetType()
	{
		var type = Variable.Type;

		if (type != null && type is ArrayType)
		{
			return type.To<ArrayType>().UsageType;
		}

		return Variable.Type;
	}

	public override bool Equals(object? other)
	{
		return other is VariableNode node &&
				base.Equals(other) &&
				EqualityComparer<Variable>.Default.Equals(Variable, node.Variable);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Variable);
	}

	public override string ToString() => $"Variable {Variable}";
}
