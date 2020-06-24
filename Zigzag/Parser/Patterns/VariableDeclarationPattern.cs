using System;
using System.Collections.Generic;

public class VariableDeclarationPattern : Pattern
{
    public const int PRIORITY = 19;

    public const int NAME = 0;
    public const int OPERATOR = 1;
    public const int TYPE = 2;

    // Examples:
    // views: num
    // apples: num[]
    // players: List(Player)
    public VariableDeclarationPattern() : base
    (
        TokenType.IDENTIFIER, TokenType.OPERATOR, TokenType.IDENTIFIER | TokenType.DYNAMIC | TokenType.FUNCTION
    )
    {
    }

    public override int GetPriority(List<Token> tokens)
    {
        return PRIORITY;
    }

    public override bool Passes(Context context, List<Token> tokens)
    {
        return tokens[OPERATOR].To<OperatorToken>().Operator == Operators.COLON;
    }

    private static TemplateType? TryGetTemplateType(Context environment, string name, Node parameters)
    {
        if (!environment.IsTemplateTypeDeclared(name)) return null;

        var template_type = (TemplateType) environment.GetType(name)!;

        // Check if the template type has the same amount of arguments as this function has parameters
        return template_type.TemplateArgumentCount == parameters.Count() ? template_type : null;
    }

    private Type SolveTemplateTypeVariant(Context environment, TemplateType template_type, Node parameters)
    {
        var types = Resolver.GetTypes(parameters);

        // Check if the type could be resolved
        var variant = types == null
            ? new UnresolvedType(environment, new UnresolvedTemplateType(template_type, parameters))
            : template_type[types.ToArray()];

        return variant;
    }

    public override Node Build(Context context, List<Token> tokens)
    {
        var name = tokens[NAME].To<IdentifierToken>();

        if (context.IsVariableDeclared(name.Value))
        {
            throw Errors.Get(name.Position, $"Variable '{name.Value}' already exists in this context");
        }

        if (name.Value == Function.THIS_POINTER_IDENTIFIER)
        {
            throw Errors.Get(name.Position,
                $"Cannot declare variable called '{Function.THIS_POINTER_IDENTIFIER}' since the name is reserved");
        }

        Type? type;

        switch (tokens[TYPE].Type)
        {
            case TokenType.IDENTIFIER:
            {
                var type_name = tokens[TYPE].To<IdentifierToken>().Value;
                type = context.GetType(type_name) ?? throw Errors.Get(tokens[TYPE].Position,
                    $"Couldn't resolve variable type '{type_name}'");
                break;
            }
            case TokenType.FUNCTION:
            {
                var function = tokens[TYPE].To<FunctionToken>();
                var parameters = function.GetParsedParameters(context);
                var template_type = TryGetTemplateType(context, function.Name, parameters);

                if (template_type != null)
                {
                    type = SolveTemplateTypeVariant(context, template_type, parameters);
                }
                else
                {
                    throw Errors.Get(tokens[TYPE].Position, "Expected template type");
                }

                break;
            }

            default:
                throw new ApplicationException("Unsupported variable declaration syntax");
        }

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