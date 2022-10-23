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
	public List<string> TemplateParameters { get; private set; }
	public List<Token> Inherited { get; private set; } = new List<Token>();

	public List<Token> Blueprint { get; private set; }
	private Dictionary<string, TemplateTypeVariant> Variants { get; set; } = new Dictionary<string, TemplateTypeVariant>();

	public TemplateType(Context context, string name, int modifiers, List<Token> blueprint, List<string> template_parameters, Position position) : base(context, name, modifiers | Modifier.TEMPLATE_TYPE, position)
	{
		Blueprint = blueprint;
		TemplateParameters = template_parameters;
	}

	private Type? TryGetVariant(Type[] arguments)
	{
		var identifier = string.Join(", ", arguments.Take(TemplateParameters.Count).Select(i => i.ToString()));

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
			else if (tokens[i].Type == TokenType.PARENTHESIS)
			{
				InsertArguments(tokens[i].To<ParenthesisToken>().Tokens, arguments);
			}
		}
	}

	private Type CreateVariant(Type[] arguments)
	{
		var identifier = string.Join(", ", arguments.Select(i => i.ToString()));

		// Copy the blueprint and insert the specified arguments to their places
		var tokens = Inherited.Select(i => (Token)i.Clone()).ToList();

		var blueprint = Blueprint.Select(i => (Token)i.Clone()).ToList();
		blueprint.First().To<IdentifierToken>().Value = Name + $"<{identifier}>";

		tokens.AddRange(blueprint);

		InsertArguments(tokens, arguments);

		// Parse the new variant
		var result = Parser.Parse(Parent!, tokens, 0, Parser.MAX_PRIORITY).First;

		if (result == null || !result.Is(NodeType.TYPE_DEFINITION))
		{
			throw new ApplicationException("Tried to parse a new variant from template type but the result was not a new type");
		}

		// Register the new variant
		var variant = result.To<TypeDefinitionNode>().Type;
		variant.Identifier = Name;
		variant.Modifiers = Modifiers & (~Modifier.IMPORTED); // Remove the imported modifier, because new variants are not imported
		variant.TemplateArguments = arguments;

		Variants.Add(identifier, new TemplateTypeVariant(variant, arguments));

		// Parse the body of the type
		result.To<TypeDefinitionNode>().Parse();

		// Finally, add the inherited supertypes to the variant
		variant.Supertypes.AddRange(Supertypes);

		return variant;
	}

	/// <summary>
	/// Returns a variant with the specified template arguments, creating it if necessary
	/// </summary>
	public Type GetVariant(Type[] arguments)
	{
		if (arguments.Length < TemplateParameters.Count)
		{
			throw new ApplicationException("Missing template arguments");
		}

		return TryGetVariant(arguments) ?? CreateVariant(arguments);
	}
}