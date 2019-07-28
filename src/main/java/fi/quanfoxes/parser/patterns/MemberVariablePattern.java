package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.DynamicToken;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.UnresolvedType;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.VariableNode;

import java.util.List;

public class MemberVariablePattern extends Pattern {
    public static final int PRIORITY = 20;

    private static final int TYPE = 0;
    private static final int NAME = 1;

    private static final int UNMODIFIED_LENGTH = 2;

    public MemberVariablePattern() {
        // Pattern:
        // [private / protected / public] [static] Type / Type.Subtype ...
        super(TokenType.KEYWORD | TokenType.OPTIONAL, TokenType.IDENTIFIER | TokenType.DYNAMIC, TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        int count = tokens.size() - UNMODIFIED_LENGTH;

        for (int i = 0; i < count; i++) {
            KeywordToken modifier = (KeywordToken)tokens.get(i);

            if (modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
                return false;
            }
        }

        Token token = tokens.get(count + TYPE);

        if (token.getType() == TokenType.DYNAMIC) {
            Node node = ((DynamicToken)token).getNode();
            return (node instanceof LinkNode) || (node instanceof TypeNode);
        }

        return true;
    }

    private int getModifiers(List<Token> tokens, int count) {
        if (count == 0) {
            return AccessModifier.PUBLIC;
        }

        int modifiers = 0;

        for (int i = 0; i < count; i++) {
            KeywordToken token = (KeywordToken)tokens.get(i);
            AccessModifierKeyword modifier = (AccessModifierKeyword)token.getKeyword();

            modifiers |= modifier.getModifier();
        }

        return modifiers;
    }

    private Type getType(Context context, List<Token> tokens, int start) throws Exception {
        Token token = tokens.get(start + TYPE);

        if (token.getType() == TokenType.DYNAMIC) {
            Node node = ((DynamicToken)token).getNode();
            
            if (node instanceof TypeNode) {
                TypeNode type = (TypeNode)node;
                return type.getType();
            }
            else if (node instanceof Resolvable) {
                return new UnresolvedType(context, (Resolvable)node);
            }
    
            throw new Exception("Node must be resolvable");
        }
        else {
            IdentifierToken id = (IdentifierToken)token;

            if (context.isTypeDeclared(id.getValue())) {
                return context.getType(id.getValue());
            }

            return new UnresolvedType(context, id.getValue());
        }
    }

    private String getName(List<Token> tokens, int start) {
        return ((IdentifierToken)tokens.get(start + NAME)).getValue();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        int count = tokens.size() - UNMODIFIED_LENGTH;

        int modifiers = getModifiers(tokens, count);
        Type type = getType(context, tokens, count);
        String name = getName(tokens, count);

        Variable variable = new Variable(context, type, name, modifiers);

        return new VariableNode(variable);
    }
}
