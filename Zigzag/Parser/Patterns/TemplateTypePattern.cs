using System.Collections.Generic;
using System.Linq;

public class TemplateTypePattern : Pattern
{
	public const int PRIORITY = 22;

	public const int NAME = 0;
	public const int TEMPLATE_ARGUMENTS = 1;
	public const int BODY = 3;

	// a-z { Type 1, Type 2 ... Type n } [\n] {...}
	public TemplateTypePattern() : base
	(
		TokenType.IDENTIFIER, TokenType.CONTENT, TokenType.END | TokenType.OPTIONAL, TokenType.CONTENT
	) { }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		return tokens[TEMPLATE_ARGUMENTS].To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS && 
               tokens[BODY].To<ContentToken>().Type == ParenthesisType.CURLY_BRACKETS;
	}

	private static List<string> GetTemplateArgumentNames(List<Token> tokens)
	{
		var template_arguments = tokens[TEMPLATE_ARGUMENTS].To<ContentToken>();
		var template_argument_names = new List<string>();

		for (var section = 0; section < template_arguments.SectionCount; section++)
		{
			var section_tokens = template_arguments.GetTokens(section);

			if (section_tokens.Count != 1 || section_tokens.First().Type != TokenType.IDENTIFIER)
			{
				throw Errors.Get(tokens[TEMPLATE_ARGUMENTS].Position, "Template type's argument list is invalid");
			}

			template_argument_names.Add(section_tokens.First().To<IdentifierToken>().Value);
		}

		if (template_argument_names.Count == 0)
		{
			throw Errors.Get(tokens[TEMPLATE_ARGUMENTS].Position, "Template type's argument list cannot be empty");
		}

		return template_argument_names;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens[BODY].To<ContentToken>();
		var template_argument_names = GetTemplateArgumentNames(tokens);

		var blueprint = new List<Token>() { (Token)name.Clone(), (Token)body.Clone() };

		var template_type = new TemplateType(context, name.Value, AccessModifier.PUBLIC | AccessModifier.TEMPLATE_TYPE, blueprint, template_argument_names);

		return new TypeNode(template_type);
	}
}
