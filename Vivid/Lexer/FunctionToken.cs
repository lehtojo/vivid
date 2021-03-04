using System;
using System.Collections.Generic;

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

	/// <summary>
	/// Returns function parameters as node tree
	/// </summary>
	/// <param name="context">Context used to parse</param>
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
	/// Returns the parameters
	/// </summary>
	/// <returns>List of parameter names</returns>
	public List<Parameter> GetParameters(Context function_context)
	{
		var parameters = new List<Parameter>();

		if (Parameters.IsEmpty)
		{
			return parameters;
		}

		Tree = GetParsedParameters(function_context);

		foreach (var parameter in Tree)
		{
			if (parameter is VariableNode node)
			{
				parameters.Add(new Parameter(node.Variable.Name, node.Position, node.Variable.Type));
			}
			else if (parameter.Is(Operators.ASSIGN))
			{
				throw Errors.Get(parameter.Position, "Default parameter values are not supported");
			}
			else if (parameter is UnresolvedIdentifier name)
			{
				parameters.Add(new Parameter(name.Value, name.Position, Types.UNKNOWN));
			}
			else
			{
				throw Errors.Get(parameter.Position, "Can not resolve the parameter declaration");
			}
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
