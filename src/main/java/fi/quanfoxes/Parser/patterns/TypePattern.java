package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.*;
import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.nodes.ContextNode;
import fi.quanfoxes.Parser.nodes.TypeNode;

import java.util.ArrayList;
import java.util.List;

public class TypePattern extends Pattern {
    public static final int PRIORITY = 20;
    private static final int ACCESS_MODIFIED_TYPE_LENGTH = 4;

    public TypePattern() {
        // [private / protected / public] type (name) {}
        super(TokenType.KEYWORD, TokenType.KEYWORD, TokenType.IDENTIFIER, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    private Keyword getAccessModifierKeyword(List<Token> tokens) {
        return ((KeywordToken)tokens.get(0)).getKeyword();
    }

    @Override
    public boolean passes(List<Token> tokens) {
        int typePosition = 0;

        if (tokens.size() == ACCESS_MODIFIED_TYPE_LENGTH) {

            Keyword modifier = getAccessModifierKeyword(tokens);

            if (modifier.getType() != KeywordType.ACCESS_MODIFIER)  {
                return false;
            }

            typePosition++;
        }

        KeywordToken type = (KeywordToken)tokens.get(typePosition);
        return type.getKeyword() == Keywords.TYPE;
    }

    @Override
    public Node build(Node parent, List<Token> tokens) throws Exception {
        int accessModifiers = AccessModifier.PUBLIC;
        int identifierPosition = 1;
        int bodyPosition = 2;

        if (tokens.size() == ACCESS_MODIFIED_TYPE_LENGTH) {
            AccessModifierKeyword modifier = (AccessModifierKeyword)getAccessModifierKeyword(tokens);
            accessModifiers = modifier.getModifier();

            identifierPosition++;
            bodyPosition++;
        }

        IdentifierToken identifier = (IdentifierToken)tokens.get(identifierPosition);
        ContentToken body = (ContentToken)tokens.get(bodyPosition);

        ContextNode context = (ContextNode) parent;

        ArrayList<Token> bodyTokens = body.getTokens();
        TypeNode type = new TypeNode(identifier.getIdentifier(), accessModifiers);
        Parser.parse(type, bodyTokens, Parser.STRUCTURAL_MIN_PRIORITY, Parser.STRUCTURAL_MAX_PRIORITY);

        context.declare(type);

        return type;
    }
}
