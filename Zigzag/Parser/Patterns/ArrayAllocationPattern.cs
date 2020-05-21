using System.Collections.Generic;
using System;

public class ArrayAllocationPattern : Pattern
{
	public const int PRIORITY = 19;

	public const int TYPE = 0;
	public const int BRACKETS = 1;

	// Example: Type[...]
	public ArrayAllocationPattern() : base
	(
		TokenType.IDENTIFIER,
		TokenType.CONTENT
	) {}

	public override bool Passes(Context context, List<Token> tokens)
	{
		var type = tokens[TYPE].To<IdentifierToken>().Value;

		if (!context.IsTypeDeclared(type))
		{
			return false;
		}

		var brackets = tokens[BRACKETS].To<ContentToken>();

		if (brackets.Type != ParenthesisType.BRACKETS)
		{
			return false;
		}

		return !brackets.IsEmpty;
	}

	public override Node? Build(Context context, List<Token> tokens)
	{
		var type = context.GetType(tokens[TYPE].To<IdentifierToken>().Value) ?? throw new ApplicationException("Array allocation type was confirmed to be valid before the current state");
		var length = Parser.Parse(context, tokens[BRACKETS].To<ContentToken>().GetTokens()).First ?? throw new ApplicationException("Array didn't have length specifier");
		
		if (length.Is(NodeType.NUMBER_NODE))
		{
			var bytes = (long)length.To<NumberNode>().Value * type.ReferenceSize;
			length = new NumberNode(Format.INT32, bytes);
		}
		else
		{
			length = new OperatorNode(Operators.MULTIPLY, length, new NumberNode(Format.INT32, type.ReferenceSize));   
		}

		return new ArrayAllocationNode(type, length);
	}

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}
}