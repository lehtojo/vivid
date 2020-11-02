using System;
using System.Collections.Generic;

public class FunctionToken : Token
{
	public IdentifierToken Identifier { get; private set; }
	public ContentToken Parameters { get; private set; }
	public Node ParameterTree { get; private set; } = new Node();

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
		if (!ParameterTree.IsEmpty)
		{
			return ParameterTree;
		}

		var parameters = Parser.Parse(context, Parameters.Tokens);

		if (parameters.First is ListNode list)
		{
			ParameterTree = list;
		}
		else
		{
			ParameterTree = parameters;
		}

		return ParameterTree;
	}

	/// <summary>
	/// Returns the parameter names
	/// </summary>
	/// <returns>List of parameter names</returns>
	public List<string> GetParameterNames(Context function_context)
	{
		var names = new List<string>();

		if (Parameters.IsEmpty)
		{
			return names;
		}

		ParameterTree = GetParsedParameters(function_context);

		foreach (var parameter in ParameterTree)
		{
			if (parameter is VariableNode variable_node)
			{
				names.Add(variable_node.Variable.Name);
			}
			else if (parameter is OperatorNode assign && assign.Operator == Operators.ASSIGN)
			{
				throw new NotImplementedException("Parameter default values aren't supported yet");
			}
			else if (parameter is UnresolvedIdentifier parameter_identifier)
			{
				names.Add(parameter_identifier.Value);
			}
			else
			{
				throw new NotImplementedException("Unknown parameter syntax");
			}
		}

		return names;
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

		ParameterTree = GetParsedParameters(function_context);

		foreach (var parameter in ParameterTree)
		{
			if (parameter is VariableNode node)
			{
				parameters.Add(new Parameter(node.Variable.Name, node.Variable.Type));
			}
			else if (parameter is OperatorNode assign && assign.Operator == Operators.ASSIGN)
			{
				throw new NotImplementedException("Parameter default values aren't supported yet");
			}
			else if (parameter is UnresolvedIdentifier name)
			{
				parameters.Add(new Parameter(name.Value));
			}
			else
			{
				throw new NotImplementedException("Unknown parameter syntax");
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
}
