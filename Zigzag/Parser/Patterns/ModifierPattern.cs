using System.Collections.Generic;

public class ModifierPattern : Pattern
{
    public const int MODIFIER = 0;
    public const int VARIABLE = 1;

    public const int PRIORITY = 1;
    
    // Example: (public static) name
    public ModifierPattern() : base
    (
        TokenType.KEYWORD,
        TokenType.DYNAMIC
    ) {}

    public override bool Passes(Context context, List<Token> tokens)
    {
        var modifier = (KeywordToken)tokens[MODIFIER];

        if (modifier.Keyword.Type != KeywordType.ACCESS_MODIFIER)
        {
            return false;
        }

        var variable = (DynamicToken)tokens[VARIABLE];

        return variable.Node.Is(NodeType.VARIABLE_NODE);
    }

    public override Node? Build(Context context, List<Token> tokens)
    {
        var modifiers = ((AccessModifierKeyword)((KeywordToken)tokens[MODIFIER]).Keyword).Modifier;
        var variable = ((VariableNode)((DynamicToken)tokens[VARIABLE]).Node).Variable;

        variable.Modifiers = modifiers;

        return null;
    }

    public override int GetPriority(List<Token> tokens)
    {
        return PRIORITY;
    }
}