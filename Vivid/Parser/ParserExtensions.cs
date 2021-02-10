using System.Collections.Generic;

public static class ParserExtensions
{
	public static IList<T> Sublist<T>(this List<T> list, int start, int end)
	{
		return new Sublist<T>(list, start, end);
	}

	public static bool Is(this Node node, Variable variable)
	{
		return node.Is(NodeType.VARIABLE) && ReferenceEquals(node.To<VariableNode>().Variable, variable);
	}

	public static bool Is(this Node node, Operator operation)
	{
		return node.Is(NodeType.OPERATOR) && node.To<OperatorNode>().Operator == operation;
	}

	public static bool Is(this Node node, OperatorType type)
	{
		return node.Is(NodeType.OPERATOR) && node.To<OperatorNode>().Operator.Type == type;
	}

	public static bool Is(this Token token, params int[] types)
	{
		foreach (var type in types)
		{
			if (token.Type == type)
			{
				return true;
			}
		}

		return false;
	}

	public static bool Is(this Token token, ParenthesisType type)
	{
		return token.Type == TokenType.CONTENT && token.To<ContentToken>().Type == type;
	}

	public static bool Is(this Token token, Operator operation)
	{
		return token.Type == TokenType.OPERATOR && token.To<OperatorToken>().Operator == operation;
	}

	public static bool Is(this Token token, Keyword keyword)
	{
		return token.Type == TokenType.KEYWORD && token.To<KeywordToken>().Keyword == keyword;
	}
}