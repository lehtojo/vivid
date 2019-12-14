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

	/// <summary>
	/// Returns function parameters as node tree
	/// </summary>
	/// <param name="context">Context used to parse</param>
	/// <returns>Parameters as node tree</returns>
	public Node GetParsedParameters(Context context)
	{
		var node = new Node();

		for (int i = 0; i < Parameters.SectionCount; i++)
		{
			var tokens = Parameters.GetTokens(i);
			Parser.Parse(node, context, tokens);
		}

		return node;
	}

	/// <summary>
	/// Returns the parameter names
	/// </summary>
	/// <returns>List of parameter names</returns>
	public List<string> GetParameterNames()
	{
		var names = new List<string>();

		if (Parameters.IsEmpty)
		{
			return names;
		}

		for (int i = 0; i < Parameters.SectionCount; i++)
		{
			var tokens = Parameters.GetTokens(i);

			if (tokens.Count > 1)
			{
				throw Errors.Get(tokens[0].Position, "Advanced parameters aren't supported yet!");
			}

			var token = tokens[0];

			if (!(token is IdentifierToken name))
			{
				throw Errors.Get(token.Position, "Invalid parameter");
			}

			names.Add(name.Value);
		}

		return names;
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
