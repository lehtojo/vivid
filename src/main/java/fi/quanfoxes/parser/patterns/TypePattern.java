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

    private static final int MODIFIED_LENGTH = 4;

    private static final int NAME = 1;
    private static final int BODY = 2;

    public TypePattern() {
        // [private / protected / public] type ... {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, TokenType.KEYWORD, TokenType.IDENTIFIER, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    private Keyword getModifierKeyword(List<Token> tokens) {
        return ((KeywordToken)tokens.get(0)).getKeyword();
    }

    @Override
    public boolean passes(List<Token> tokens) {
        int start = 0;

        if (tokens.size() == MODIFIED_LENGTH) {

            Keyword modifier = getModifierKeyword(tokens);

            // Access modifier keywords are only accepted
            if (modifier.getType() != KeywordType.ACCESS_MODIFIER)  {
                return false;
            }

            start++;
        }

        KeywordToken type = (KeywordToken)tokens.get(start);
        return type.getKeyword() == Keywords.TYPE;
    }

    private String getName(List<Token> tokens, int start) {
        return ((IdentifierToken)tokens.get(start + NAME)).getValue();
    }

    private List<Token> getBody(List<Token> tokens, int start) {
        return ((ContentToken)tokens.get(start + BODY)).getTokens();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        int modifiers = AccessModifier.PUBLIC;
        int start = 0;

        // Check if this type has access modifiers
        if (tokens.size() == MODIFIED_LENGTH) {
            AccessModifierKeyword modifier = (AccessModifierKeyword)getModifierKeyword(tokens);
            modifiers = modifier.getModifier();

            start++;
        }

        // Get type name and its body
        String name = getName(tokens, start);
        List<Token> body = getBody(tokens, start);

        // Create this type and parse its possible subtypes
        Type type = new Type(context, name, modifiers);
        Parser.parse(type, body, PRIORITY);

        return new TypeNode(type, body);
    }
}
