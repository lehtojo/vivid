package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.*;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.nodes.TypeNode;

import java.util.List;

public class TypePattern extends Pattern {
    public static final int PRIORITY = 21;

    private static final int MODIFIER = 0;
    private static final int TYPE = 1;
    private static final int NAME = 2;
    private static final int BODY = 4;

    public TypePattern() {
        // [private / protected / public] type ... [\n] {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
              TokenType.KEYWORD, /* type */
              TokenType.IDENTIFIER, /* ... */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.CONTENT); /* {...} */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken modifier = (KeywordToken)tokens.get(MODIFIER);

        if (modifier != null && modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
            return false;
        }

        KeywordToken type = (KeywordToken)tokens.get(TYPE);
        return type.getKeyword() == Keywords.TYPE;
    }

    private String getName(List<Token> tokens) {
        return ((IdentifierToken)tokens.get(NAME)).getValue();
    }

    private List<Token> getBody(List<Token> tokens) {
        return ((ContentToken)tokens.get(BODY)).getTokens();
    }

    private int getModifiers(List<Token> tokens) {
        int modifiers = AccessModifier.PUBLIC;

        KeywordToken token = (KeywordToken)tokens.get(MODIFIER);

        if (token != null) {
            AccessModifierKeyword modifier = (AccessModifierKeyword)token.getKeyword();
            modifiers = modifier.getModifier();
        }

        return modifiers;
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        int modifiers = getModifiers(tokens);

        // Get type name and its body
        String name = getName(tokens);
        List<Token> body = getBody(tokens);

        // Create this type and parse its possible subtypes
        Type type = new Type(context, name, modifiers);
        Parser.parse(type, body, PRIORITY);

        return new TypeNode(type, body);
    }
}
