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
	private const string TEMPLATE_ARGUMENT_SIZE_ACCESSOR = "size";
	private const string TEMPLATE_ARGUMENT_NAME_ACCESSOR = "name";

	private const int NAME = 0;

	public List<string> TemplateArgumentNames { get; private set; }
	public int TemplateArgumentCount => TemplateArgumentNames.Count;

	private List<Token> Blueprint { get; set; }
	private Dictionary<string, TemplateTypeVariant> Variants { get; set; } = new Dictionary<string, TemplateTypeVariant>();

	public static TemplateType? TryGetTemplateType(Context environment, string name, Node parameters)
	{
		if (!environment.IsTemplateTypeDeclared(name))
		{
			return null;
		}

		var template_type = (TemplateType)environment.GetType(name)!;

		// Check if the template type has the same amount of arguments as this function has parameters
		return template_type.TemplateArgumentCount == parameters.Count() ? template_type : null;
	}

	public TemplateType(Context context, string name, int modifiers, List<Token> blueprint, List<string> template_argument_names) : base(context, name, modifiers)
	{
		Blueprint = blueprint;
		TemplateArgumentNames = template_argument_names;
	}

	private Type? TryGetVariant(Type[] arguments)
	{
		var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

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

				var type = arguments[j];

				// Check if accessor pattern is possible (e.g T.size or T.name)
				if (i + 2 < tokens.Count && tokens[i + 1].Type == TokenType.OPERATOR && tokens[i + 1].To<OperatorToken>().Operator == Operators.DOT && tokens[i + 2].Type == TokenType.IDENTIFIER)
				{
					switch (tokens[i + 2].To<IdentifierToken>().Value)
					{
						case TEMPLATE_ARGUMENT_SIZE_ACCESSOR:
						{
							tokens.RemoveRange(i, 3);
							tokens.Insert(i, new NumberToken(type.ReferenceSize));
							continue;
						}

						case TEMPLATE_ARGUMENT_NAME_ACCESSOR:
						{
							tokens.RemoveRange(i, 3);
							tokens.Insert(i, new StringToken(type.Name));
							continue;
						}

						default: break;
					}
				}

				tokens[i].To<IdentifierToken>().Value = type.Name;
			}
			else if (tokens[i].Type == TokenType.CONTENT)
			{
				InsertArguments(tokens[i].To<ContentToken>().Tokens, arguments);
			}
		}
	}

	private Type CreateVariant(Type[] arguments)
	{
		var identifier = string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name));

		// Copy the blueprint and insert the specified arguments to their places
		var blueprint = Blueprint.Select(t => (Token)t.Clone()).ToList();
		blueprint[NAME].To<IdentifierToken>().Value = Name + '<' + string.Join(", ", arguments.Take(TemplateArgumentNames.Count).Select(a => a.Name)) + '>';

		InsertArguments(blueprint, arguments);

		// Parse the new variant
		var result = Parser.Parse(Parent ?? throw new ApplicationException("Template type did not have parent context"), blueprint).First;

		if (result == null || !result.Is(NodeType.TYPE))
		{
			throw new ApplicationException("Tried to parse a new variant from template type but the result was not a new type");
		}

		// Register the new variant
		var variant = result.To<TypeNode>().Type;
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

	/// <summary>
	/// Returns the template arguments of the specified variant
	/// </summary>
	public Type[]? GetVariantArguments(Type variant)
	{
		foreach (var iterator in Variants.Values)
		{
			if (iterator.Type == variant)
			{
				return iterator.Arguments;
			}
		}

		return null;
	}
}