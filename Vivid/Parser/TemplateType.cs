using System;
using System.Collections.Generic;
using System.Linq;

public class TemplateTypeVariant
{
	public Type Type { get; private set; }
	public Type[] Arguments { get; private set; }

	public TemplateTypeVariant(Type type, Type[] arguments)
	{
		Type = type;
		Arguments = arguments;
	}
}

public class TemplateType : Type
{
	private const int NAME = 0;

	public List<string> TemplateArgumentNames { get; private set; }
	public List<Token> Inherited { get; private set; } = new List<Token>();

	public List<Token> Blueprint { get; private set; }
	private Dictionary<string, TemplateTypeVariant> Variants { get; set; } = new Dictionary<string, TemplateTypeVariant>();

	public TemplateType(Context context, string name, int modifiers, List<Token> blueprint, List<string> template_argument_names, Position position) : base(context, name, modifiers | Modifier.TEMPLATE_TYPE, position)
	{
		Blueprint = blueprint;
		TemplateArgumentNames = template_argument_names;
	}

	public TemplateType(Context context, string name, int modifiers, int argument_count) : base(context, name, modifiers | Modifier.TEMPLATE_TYPE)
	{
		// Create an empty type with the specified name using tokens
		Blueprint = new List<Token> { new IdentifierToken(name), new ContentToken() { Type = ParenthesisType.CURLY_BRACKETS } };
		TemplateArgumentNames = new List<string>();

		// Generate the template arguments
		for (var i = 0; i < argument_count; i++)
		{
			TemplateArgumentNames.Add($"T{i}");
		}
	}

	private Type? TryGetVariant(Type[] arguments)
	{
		var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(i => i.ToString()));

		if (Variants.TryGetValue(identifier, out TemplateTypeVariant? variant))
		{
			return variant.Type;
		}

		return null;
	}

	private void InsertArguments(List<Token> tokens, Type[] arguments)
	{
		for (var i = 0; i < tokens.Count; i++)
		{
			if (tokens[i].Type == TokenType.IDENTIFIER)
			{
				var j = TemplateArgumentNames.IndexOf(tokens[i].To<IdentifierToken>().Value);

				if (j == -1)
				{
					continue;
				}

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

	private Type CreateVariant(Type[] arguments)
	{
		var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(i => i.ToString()));

		// Copy the blueprint and insert the specified arguments to their places
		var tokens = Inherited.Select(t => (Token)t.Clone()).ToList();

		var blueprint = Blueprint.Select(t => (Token)t.Clone()).ToList();
		blueprint[NAME].To<IdentifierToken>().Value = Name + '<' + string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name)) + '>';

		tokens.AddRange(blueprint);

		InsertArguments(tokens, arguments);

		// Parse the new variant
		var result = Parser.Parse(Parent ?? throw new ApplicationException("Template type did not have parent context"), tokens).First;

		if (result == null || !result.Is(NodeType.TYPE))
		{
			throw new ApplicationException("Tried to parse a new variant from template type but the result was not a new type");
		}

		// Register the new variant
		var variant = result.To<TypeNode>().Type;
		variant.Identifier = Name;
		variant.Modifiers = Modifiers;
		variant.TemplateArguments = arguments;

		Variants.Add(identifier, new TemplateTypeVariant(variant, arguments));

		// Parse the body of the type
		result.To<TypeNode>().Parse();

		// Finally, add the inherited supertypes to the variant
		variant.Supertypes.AddRange(Supertypes);

		return variant;
	}

	/// <summary>
	/// Returns a variant with the specified template arguments, creating it if necessary
	/// </summary>
	public Type GetVariant(Type[] arguments)
	{
		if (arguments.Length < TemplateArgumentNames.Count)
		{
			throw new ApplicationException("Missing template arguments");
		}

		return TryGetVariant(arguments) ?? CreateVariant(arguments);
	}
}