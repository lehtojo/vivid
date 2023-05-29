using System.Collections.Generic;
using System.Linq;

public class TemplateTypePattern : Pattern
{
	public const int NAME = 0;
	public const int TEMPLATE_PARAMETERS = 1;
	public const int BODY = 3;

	public const int TEMPLATE_PARAMETERS_START = 2;
	public const int TEMPLATE_PARAMETERS_END = 3;

	// Pattern: $name <$1, $2, ... $n> [\n] {...}
	public TemplateTypePattern() : base
	(
		TokenType.IDENTIFIER
	)
	{ Priority = 22; IsConsumable = false; }

	public override bool Passes(Context context, ParserState state, List<Token> tokens, int priority)
	{
		// Pattern: $name <$1, $2, ... $n> [\n] {}
		if (!Common.ConsumeTemplateParameters(state)) return false;

		// Optionally consume a line-ending
		state.ConsumeOptional(TokenType.END);

		// Consume the body of the template type
		return state.ConsumeParenthesis(ParenthesisType.CURLY_BRACKETS);
	}

	public override Node Build(Context context, ParserState state, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();
		var body = tokens.Last().To<ParenthesisToken>();

		var template_parameter_tokens = tokens.GetRange(TEMPLATE_PARAMETERS_START, tokens.Count - TEMPLATE_PARAMETERS_END - TEMPLATE_PARAMETERS_START);
		var template_parameters = Common.GetTemplateParameters(template_parameter_tokens, tokens[TEMPLATE_PARAMETERS].Position);

		var blueprint = new List<Token>() { (Token)name.Clone(), (Token)body.Clone() };

		var template_type = new TemplateType(context, name.Value, Modifier.DEFAULT, blueprint, template_parameters, name.Position);
		return new TypeDefinitionNode(template_type, new List<Token>(), name.Position);
	}
}
