using System.Collections.Generic;

public static class ParserExtensions
{
	public static IList<T> Sublist<T>(this List<T> list, int start, int end)
	{
		return new Sublist<T>(list, start, end);
	}

	public static bool Is(this Node node, Variable variable)
	{
		return node.Is(NodeType.VARIABLE_NODE) && node.To<VariableNode>().Variable == variable;
	}

	public static bool Is(this Node node, Operator operation)
	{
		return node.Is(NodeType.OPERATOR_NODE) && node.To<OperatorNode>().Operator == operation;
	}
}