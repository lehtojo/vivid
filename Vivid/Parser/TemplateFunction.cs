using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateFunction : Function
{
	public List<string> TemplateParameters { get; private set; }
	private FunctionToken Header { get; set; }
	private Dictionary<string, Function> Variants { get; set; } = new Dictionary<string, Function>();

	public TemplateFunction(Context context, int modifiers, string name, List<string> template_parameters, List<Token> parameter_tokens, Position? start, Position? end) : base(context, modifiers | Modifier.TEMPLATE_FUNCTION, name, start, end)
	{
		TemplateParameters = template_parameters;
		Header = new FunctionToken(new IdentifierToken(name), new ContentToken(parameter_tokens));
	}

	public TemplateFunction(Context context, int modifiers, string name, int parameters) : base(context, modifiers | Modifier.TEMPLATE_FUNCTION, name, (Position?)null, (Position?)null)
	{
		TemplateParameters = new List<string>();

		// Generate parameter names based on the specified parameter count
		var parameter_tokens = new List<Token>();

		for (var i = 0; i < parameters; i++)
		{
			parameter_tokens.Add(new IdentifierToken($"P{i}"));
			parameter_tokens.Add(new OperatorToken(Operators.COMMA));
		}

		// Remove the unnecessary comma from the end
		if (parameters > 0) parameter_tokens.RemoveAt(parameter_tokens.Count - 1);

		Header = new FunctionToken(new IdentifierToken(name), new ContentToken(parameter_tokens));
	}

	/// <summary>
	/// Creates the parameters of this function in a way that they do not have types
	/// </summary>
	public void Initialize()
	{
		Parameters.AddRange(Header.GetParameters(new Context(string.Empty)));
	}

	private Function? TryGetVariant(Type[] template_arguments)
	{
		var identifier = string.Join(", ", template_arguments.Take(TemplateParameters.Count).Select(i => i.ToString()));

		if (Variants.TryGetValue(identifier, out Function? variant)) return variant;

		return null;
	}

	private void InsertArguments(List<Token> tokens, Type[] arguments)
	{
		for (var i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].Type == TokenType.IDENTIFIER)
			{
				var j = TemplateParameters.IndexOf(tokens[i].To<IdentifierToken>().Value);
				if (j == -1) continue;

				var position = tokens[i].To<IdentifierToken>().Position;

				tokens.RemoveAt(i);
				tokens.InsertRange(i, Common.GetTokens(arguments[j], position));
			}
			else if (tokens[i].Type == TokenType.FUNCTION)
			{
				InsertArguments(tokens[i].To<FunctionToken>().Parameters.Tokens, arguments);
			}
			else if (tokens[i].Type == TokenType.CONTENT)
			{
				InsertArguments(tokens[i].To<ContentToken>().Tokens, arguments);
			}
		}
	}

	private Function? CreateVariant(Type[] template_arguments)
	{
		var identifier = string.Join(", ", template_arguments.Select(i => i.ToString()));

		// Copy the blueprint and insert the specified arguments to their places
		var blueprint = Blueprint.Select(i => (Token)i.Clone()).ToList();
		blueprint.First().To<FunctionToken>().Identifier.Value = Name + $"<{identifier}>";

		InsertArguments(blueprint, template_arguments);

		// Parse the new variant
		var result = Parser.Parse(Parent ?? throw new ApplicationException("Template function did not have parent context"), blueprint).First;

		if (result == null || !result.Is(NodeType.FUNCTION_DEFINITION))
		{
			throw new ApplicationException("Tried to parse a new variant from template function but the result was not a new function");
		}

		// Register the new variant
		var variant = result.To<FunctionDefinitionNode>().Function;
		Variants.Add(identifier, variant);

		return variant;
	}

	public override bool Passes(List<Type> arguments)
	{
		throw new InvalidOperationException("Tried to execute pass function without template parameters");
	}

	public new bool Passes(List<Type> actual_types, Type[] template_arguments)
	{
		if (template_arguments.Length != TemplateParameters.Count) return false;

		// None of the types can be unresolved
		if (actual_types.Any(i => i.IsUnresolved) || template_arguments.Any(i => i.IsUnresolved)) return false;

		// Clone the header, insert the template arguments and determine the expected parameters
		var header = (FunctionToken)Header.Clone();
		InsertArguments(header.Parameters.Tokens, template_arguments);

		var container = new Context(this);
		var expected_types = header.GetParameters(container).Select(i => i.Type).ToList();
		if (expected_types.Count != actual_types.Count) return false;

		for (var i = 0; i < actual_types.Count; i++)
		{
			var expected = expected_types[i];
			if (expected == null) continue;

			var actual = actual_types[i];
			if (Equals(expected, actual)) continue;
			
			if (!expected.IsPrimitive || !actual.IsPrimitive)
			{
				if (!expected.IsTypeInherited(actual) && !actual.IsTypeInherited(expected)) return false;
			}
			else if (Resolver.GetSharedType(expected, actual) == null)
			{
				return false;
			}
		}

		return true;
	}

	public override FunctionImplementation? Get(IEnumerable<Type> arguments)
	{
		throw new InvalidOperationException("Tried to get overload of template function without template parameters");
	}

	public FunctionImplementation? Get(List<Type> parameters, Type[] template_arguments)
	{
		if (template_arguments.Length != TemplateParameters.Count)
		{
			throw new ApplicationException("Missing template arguments");
		}

		var variant = TryGetVariant(template_arguments) ?? CreateVariant(template_arguments);

		if (variant == null)
		{
			return null;
		}

		var implementation = variant.Get(parameters)!;
		implementation.Identifier = Name;
		implementation.Metadata.Modifiers = Modifiers;
		implementation.TemplateArguments = template_arguments;

		return implementation;
	}

	public override string ToString()
	{
		return Name + '<' + string.Join(", ", TemplateParameters) + $">({string.Join(", ", Parameters)})";
	}
}