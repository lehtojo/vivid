using System;
using System.Collections.Generic;

public class ExternalFunctionPattern : Pattern
{
	private const int PRIORITY = 20;

	private const int IMPORT = 0;
	private const int TYPE = 1;
	private const int HEAD = 2;

	public ExternalFunctionPattern() : base(TokenType.KEYWORD, /* import */
											TokenType.KEYWORD | TokenType.IDENTIFIER | TokenType.DYNAMIC, /* func / Type / Type.Subtype */
											TokenType.FUNCTION | TokenType.IDENTIFIER, /* name (...) */
											TokenType.END) /* \n */
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(List<Token> tokens)
	{
		KeywordToken keyword = (KeywordToken)tokens[IMPORT];

		if (keyword.Keyword != Keywords.IMPORT)
		{
			return false;
		}

		Token token = tokens[TYPE];

		switch (token.Type)
		{

			case TokenType.KEYWORD:
			{
				return ((KeywordToken)token).Keyword == Keywords.FUNC;
			}

			case TokenType.IDENTIFIER:
			{
				return true;
			}

			case TokenType.DYNAMIC:
			{
				Node node = ((DynamicToken)token).Node;
				return node.GetNodeType() == NodeType.TYPE_NODE || node.GetNodeType() == NodeType.LINK_NODE;
			}
		}

		return false;
	}

	private Type getReturnType(Context context, List<Token> tokens)
	{
		Token token = tokens[TYPE];

		switch (token.Type)
		{

			case TokenType.KEYWORD:
			{
				return Types.UNKNOWN;
			}

			case TokenType.IDENTIFIER:
			{
				IdentifierToken id = (IdentifierToken)token;

				if (context.IsTypeDeclared(id.Value))
				{
					return context.GetType(id.Value);
				}
				else
				{
					return new UnresolvedType(context, id.Value);
				}
			}

			case TokenType.DYNAMIC:
			{
				DynamicToken dynamic = (DynamicToken)token;
				Node node = dynamic.Node;

				if (node.GetNodeType() == NodeType.TYPE_NODE)
				{
					TypeNode type = (TypeNode)node;
					return type.Type;
				}
				else if (node is Resolvable resolvable)
				{
					return new UnresolvedType(context, resolvable);
				}

				break;
			}
		}

		throw new Exception("INTERNAL_ERROR");
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		Function function;

		Type result = getReturnType(context, tokens);
		Token token = tokens[HEAD];

		if (token.Type == TokenType.FUNCTION)
		{
			FunctionToken head = (FunctionToken)token;
			function = new Function(context, head.Name, AccessModifier.EXTERNAL, result);
			function.SetParameters(head.GetParsedParameters(function));
		}
		else
		{
			IdentifierToken name = (IdentifierToken)token;
			function = new Function(context, name.Value, AccessModifier.EXTERNAL, result);
		}

		return new FunctionNode(function);
	}
}