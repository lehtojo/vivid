using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionToken : Token
{
	public IdentifierToken Identifier { get; private set; }
	public ParenthesisToken Parameters { get; private set; }
	public Node Node { get; private set; } = new Node();

	public string Name => Identifier.Value;

	public FunctionToken(IdentifierToken name, ParenthesisToken parameters) : base(TokenType.FUNCTION)
	{
		Identifier = name;
		Parameters = parameters;
	}

	public FunctionToken(IdentifierToken name, ParenthesisToken parameters, Position position) : base(TokenType.FUNCTION)
	{
		Identifier = name;
		Parameters = parameters;
		Position = position;
	}

	/// <summary>
	/// Returns the parameters of this token
	/// </summary>
	public List<Parameter> GetParameters(Context context)
	{
		var tokens = Parameters.Tokens.Where(i => i.Type != TokenType.END).ToList();
		var result = new List<Parameter>();

		while (tokens.Any())
		{
			// Ensure the name is valid
			var name = tokens.Pop();
			if (name == null || !name.Is(TokenType.IDENTIFIER)) throw Errors.Get(name?.Position, "Can not understand the parameters");

			var next = tokens.Pop();

			if (next == null || next.Is(Operators.COMMA))
			{
				result.Add(new Parameter(name.To<IdentifierToken>().Value, name.Position, null));
				continue;
			}

			// If there are tokens left and the next token is not a comma, it must represent a parameter type
			if (!next.Is(Operators.COLON)) throw Errors.Get(name?.Position, "Can not understand the parameters");

			var parameter_type = Common.ReadType(context, tokens);
			if (parameter_type == null) throw Errors.Get(next.Position, "Can not resolve the parameter type");

			result.Add(new Parameter(name.To<IdentifierToken>().Value, name.Position, parameter_type));

			// If there are tokens left, the next token must be a comma and it must be removed before starting over
			if (tokens.Any() && !tokens.Pop()!.Is(Operators.COMMA)) throw Errors.Get(Position, "Can not understand the parameters");
		}

		// Declare the parameters
		foreach (var parameter in result)
		{
			context.Declare(parameter.Type, VariableCategory.PARAMETER, parameter.Name).Position = parameter.Position;
		}

		return result;
	}

	public Node Parse(Context context)
	{
		if (Node.First != null) return Node;

		var result = Parser.Parse(context, new List<Token>(Parameters.Tokens), Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);

		if (result.First != null && result.First.Instance == NodeType.LIST) { Node = result.First; }
		else { Node = result; }

		return Node;
	}

	public override bool Equals(object? other)
	{
		return other is FunctionToken token &&
			   base.Equals(other) &&
			   EqualityComparer<IdentifierToken>.Default.Equals(Identifier, token.Identifier) &&
			   EqualityComparer<ParenthesisToken>.Default.Equals(Parameters, token.Parameters) &&
			   Name == token.Name;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Identifier, Parameters, Name);
	}

	public override object Clone()
	{
		var clone = (FunctionToken)MemberwiseClone();
		clone.Parameters = (ParenthesisToken)Parameters.Clone();
		clone.Identifier = (IdentifierToken)Identifier.Clone();

		return clone;
	}

	public override string ToString()
	{
		return Name + Parameters.ToString();
	}
}
