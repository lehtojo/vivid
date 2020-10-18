using System;
using System.Collections.Generic;
using System.Linq;

public class VariableDeclarationPattern : Pattern
{
	public const int PRIORITY = 19;

	private const int LAMBDA_MIN_TOKEN_COUNT = 5;
	private const int TEMPLATE_TYPE_MIN_TOKEN_COUNT = 6;

	public const int NAME = 0;
	public const int OPERATOR = 1;
	public const int TYPE = 2;

	public const int LAMBDA_TYPE_PARAMETERS = 2;
	public const int LAMBDA_TYPE_IMPLICATION = 3;
	public const int LAMBDA_TYPE_RETURN_TYPE = 4;

	public const int TEMPLATE_TYPE_PARAMETERS = 3;

	// Examples:
	// views: num
	// apples: num[]
	// players: List<Player>
	// predicate: (num) => bool
	public VariableDeclarationPattern() : base
	(
		TokenType.IDENTIFIER, TokenType.OPERATOR
	)
	{ }

	public override int GetPriority(List<Token> tokens)
	{
		return PRIORITY;
	}

	public override bool Passes(Context context, PatternState state, List<Token> tokens)
	{
		if (!tokens[OPERATOR].Is(Operators.COLON))
		{
			return false;
		}

		// Examples:
		// (num) => bool
		if (Consume(state, out List<Token> consumed, TokenType.CONTENT, TokenType.OPERATOR, TokenType.IDENTIFIER))
		{
			if (!consumed.First().Is(ParenthesisType.PARENTHESIS))
			{
				return false;
			}

			// Ensure the consumed operator is the implication operator
			return consumed[1].Is(Operators.IMPLICATION);
		}

		// Examples:
		// players: List<Player>
		// views: num
		return Try(Common.ConsumeType, state) || Consume(state, out Token? _, TokenType.IDENTIFIER | TokenType.FUNCTION);
	}

	private static bool IsTypeLambda(List<Token> tokens)
	{
		return tokens.Count >= LAMBDA_MIN_TOKEN_COUNT && tokens[LAMBDA_TYPE_IMPLICATION].Is(Operators.IMPLICATION);
	}

	private static bool IsTemplateType(List<Token> tokens)
	{
		return tokens.Count >= TEMPLATE_TYPE_MIN_TOKEN_COUNT && tokens[TEMPLATE_TYPE_PARAMETERS].Is(Operators.LESS_THAN);
	}

	private static Type ResolveType(Context context, List<Token> tokens)
	{
		Type? type;

		if (IsTypeLambda(tokens))
		{
			var parameters = tokens[LAMBDA_TYPE_PARAMETERS].Type == TokenType.CONTENT
				? tokens[LAMBDA_TYPE_PARAMETERS].To<ContentToken>()
				: new ContentToken(tokens[LAMBDA_TYPE_PARAMETERS]);

			var parameter_types = new List<Type>();
			var sections = parameters.GetSections();

			for (var i = 0; i < sections.Count; i++)
			{
				var section = sections[i];

				if (section.Count != 1)
				{
					throw Errors.Get(tokens[LAMBDA_TYPE_PARAMETERS].Position, "Parameters of the short function could not be resolved");
				}

				var parameter_type = section.First();

				if (!parameter_type.Is(TokenType.IDENTIFIER))
				{
					throw Errors.Get(tokens[LAMBDA_TYPE_PARAMETERS].Position, "Parameters of the short function could not be resolved");
				}

				parameter_types.Add(
					context.GetType(parameter_type.To<IdentifierToken>().Value) ??
					new UnresolvedType(context, parameter_type.To<IdentifierToken>().Value)
				);
			}

			var return_type_name = tokens[LAMBDA_TYPE_RETURN_TYPE].To<IdentifierToken>().Value;
			var return_type = Types.UNIT;

			// Underscore here indicates no return type
			if (return_type_name != "_")
			{
				return_type = context.GetType(return_type_name) ?? new UnresolvedType(context, return_type_name);
			}

			return new LambdaType(parameter_types!, return_type);
		}
		else if (IsTemplateType(tokens))
		{
			var template_arguments = Common.ReadTemplateArguments(context, new Queue<Token>(tokens.Skip(TEMPLATE_TYPE_PARAMETERS)));

			var name = tokens[TYPE].To<IdentifierToken>().Value;

			if (template_arguments.Any(i => i.IsUnresolved))
			{
				return new UnresolvedType(context, name, template_arguments);
			}

			type = context.GetType(name);

			if (type != null && type is TemplateType template_type)
			{
				return template_type.GetVariant(template_arguments);
			}

			return new UnresolvedType(context, name, template_arguments);
		}

		switch (tokens[TYPE].Type)
		{
			case TokenType.IDENTIFIER:
			{
				var type_name = tokens[TYPE].To<IdentifierToken>().Value;
				type = context.GetType(type_name) ?? new UnresolvedType(context, type_name);
				break;
			}

			default: throw new ApplicationException("Unsupported variable declaration syntax");
		}

		return type;
	}

	public override Node Build(Context context, List<Token> tokens)
	{
		var name = tokens[NAME].To<IdentifierToken>();

		if (context.IsLocalVariableDeclared(name.Value))
		{
			throw Errors.Get(name.Position, $"Variable '{name.Value}' already exists in this context");
		}

		if (name.Value == Function.SELF_POINTER_IDENTIFIER || name.Value == Lambda.SELF_POINTER_IDENTIFIER)
		{
			throw Errors.Get(name.Position, $"Cannot declare variable called '{name.Value}' since the name is reserved");
		}

		var type = ResolveType(context, tokens);

		var category = context.IsType ? VariableCategory.MEMBER : VariableCategory.LOCAL;
		var is_constant = !context.IsInsideFunction && !context.IsInsideType;

		var variable = new Variable
		(
			context,
			type,
			category,
			name.Value,
			AccessModifier.PUBLIC | (is_constant ? AccessModifier.CONSTANT : 0)
		);

		return new VariableNode(variable);
	}
}