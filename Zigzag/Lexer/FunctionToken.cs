using System;
using System.Collections.Generic;

public class FunctionToken : Token
{
	public IdentifierToken Identifier { get; private set; }
	public ContentToken Parameters { get; private set; }

	public string Name => Identifier.Value;

	public FunctionToken(IdentifierToken name, ContentToken parameters) : base(TokenType.FUNCTION)
	{
		Identifier = name;
		Parameters = parameters;
	}

	public Node GetParsedParameters(Context context)
	{
		Node node = new Node();

		for (int i = 0; i < Parameters.SectionCount; i++)
		{
			List<Token> tokens = Parameters.GetTokens(i);
			Parser.Parse(node, context, tokens);
		}

		return node;
	}

	public override bool Equals(object obj)
	{
		return obj is FunctionToken token &&
			   base.Equals(obj) &&
			   EqualityComparer<IdentifierToken>.Default.Equals(Identifier, token.Identifier) &&
			   EqualityComparer<ContentToken>.Default.Equals(Parameters, token.Parameters) &&
			   Name == token.Name;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Identifier, Parameters, Name);
	}
}
