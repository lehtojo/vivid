using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionToken : Token
{
	public IdentifierToken Identifier { get; private set; }
	public ContentToken Parameters { get; private set; }
	public Node Tree { get; private set; } = new Node();

	public string Name => Identifier.Value;

	public FunctionToken(IdentifierToken name, ContentToken parameters) : base(TokenType.FUNCTION)
	{
		Identifier = name;
		Parameters = parameters;
	}

	public FunctionToken(IdentifierToken name, ContentToken parameters, Position position) : base(TokenType.FUNCTION)
	{
		Identifier = name;
		Parameters = parameters;
		Position = position;
	}

	/// <summary>
	/// Returns function parameters as node tree
	/// </summary>
	/// <returns>Parameters as node tree</returns>
	public Node GetParsedParameters(Context context)
	{
		if (!Tree.IsEmpty)
		{
			return Tree;
		}

		// NOTE: It is important that the tokens are cloned, since consumed tokens in short functions for example, would corrupt with parsed dynamic tokens
		var parameters = Parser.Parse(context, new List<Token>(Parameters.Tokens), Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		if (parameters.First is ListNode list)
		{
			Tree = list;
		}
		else
		{
			Tree = parameters;
		}

		return Tree;
	}

	/// <summary>
	/// Returns the parameters of this token
	/// </summary>
	public List<Parameter> GetParameters(Context context)
	{
		var tokens = new List<Token>(Parameters.Tokens);
		var parameters = new List<Parameter>();

		while (tokens.Any())
		{
			var name = tokens.Pop();
			
			// Ensure the name is valid
			if (name == null) throw Errors.Get(Position, "Invalid parameters");
			if (!name.Is(TokenType.IDENTIFIER)) throw Errors.Get(name.Position, "Can not resolve the parameter declaration");

			// Try to consume a parameter type
			var next = tokens.Pop();

			if (next == null || next.Is(Operators.COMMA))
			{
				parameters.Add(new Parameter(name.To<IdentifierToken>().Value, name.Position, null));
				continue;
			}

			// If there are tokens left and the next token is not a comma, it must represent a parameter type
			if (!next.Is(Operators.COLON)) throw Errors.Get(name.Position, "Can not resolve the parameter declaration");

			var source = new Queue<Token>(tokens);
			var type = Common.ReadType(context, source);

			if (type == null) throw Errors.Get(next.Position, "Can not resolve the parameter type");

			// Remove the same number of tokens as the type consumed
			tokens.RemoveRange(0, tokens.Count - source.Count);

			parameters.Add(new Parameter(name.To<IdentifierToken>().Value, name.Position, type));

			// If there are tokens left, the next token must be a comma and it must be removed before starting over
			if (tokens.Any() && !tokens.Pop()!.Is(Operators.COMMA)) throw Errors.Get(Position, "Invalid parameters");
		}

		// Declare the parameters
		foreach (var parameter in parameters)
		{
			context.Declare(parameter.Type, VariableCategory.PARAMETER, parameter.Name);
		}

		return parameters;
	}

	public override bool Equals(object? other)
	{
		return other is FunctionToken token &&
			   base.Equals(other) &&
			   EqualityComparer<IdentifierToken>.Default.Equals(Identifier, token.Identifier) &&
			   EqualityComparer<ContentToken>.Default.Equals(Parameters, token.Parameters) &&
			   Name == token.Name;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Identifier, Parameters, Name);
	}

	public override object Clone()
	{
		var clone = (FunctionToken)MemberwiseClone();
		clone.Parameters = (ContentToken)Parameters.Clone();
		clone.Identifier = (IdentifierToken)Identifier.Clone();

		return clone;
	}

	public override string ToString()
	{
		return Identifier.ToString() + Parameters.ToString();
	}
}
