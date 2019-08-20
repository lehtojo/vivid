package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.Errors;
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
import fi.quanfoxes.parser.VariableType;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.VariableNode;

import java.util.List;

public class MemberVariablePattern extends Pattern {
    public static final int PRIORITY = 20;

    private static final int MODIFIER = 0;
    private static final int TYPE = 2;
    private static final int NAME = 3;

    public MemberVariablePattern() {
        // Pattern:
        // [private / protected / public] [static] Type / Type.Subtype ...
        super(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
              TokenType.KEYWORD | TokenType.OPTIONAL, /* [static] */
              TokenType.IDENTIFIER | TokenType.DYNAMIC, /* Type / Type.Subtype */
              TokenType.IDENTIFIER); /* ... */
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken first = (KeywordToken)tokens.get(MODIFIER);
        KeywordToken second = (KeywordToken)tokens.get(MODIFIER + 1);

        if ((first != null && first.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) ||
            (second != null && second.getKeyword().getType() != KeywordType.ACCESS_MODIFIER)) {
            return false;
        }

        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.DYNAMIC) {
            Node node = ((DynamicToken)token).getNode();
            return (node instanceof LinkNode) || (node instanceof TypeNode);
        }

        return true;
    }

    private int getModifiers(List<Token> tokens) {
        KeywordToken first = (KeywordToken)tokens.get(MODIFIER);
        KeywordToken second = (KeywordToken)tokens.get(MODIFIER + 1);

        int modifiers = AccessModifier.PUBLIC;

        if (first != null) {
            AccessModifierKeyword modifier = (AccessModifierKeyword)first.getKeyword();
            modifiers |= modifier.getModifier();

            if (second != null) {
                modifier = (AccessModifierKeyword)second.getKeyword();
                modifiers |= modifier.getModifier();
            }
        }

        return modifiers;
    }

    private Type getType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(TYPE);

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

    private String getName(List<Token> tokens) {
        return ((IdentifierToken)tokens.get(NAME)).getValue();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        int modifiers = getModifiers(tokens);
        Type type = getType(context, tokens);
        String name = getName(tokens);

        if (context.isLocalVariableDeclared(name)) {
            throw Errors.get(tokens.get(NAME).getPosition(), String.format("Variable '%s' already exists in this context", name));
        }

        VariableType category = context.isGlobalContext() ? VariableType.GLOBAL : VariableType.MEMBER;
        Variable variable = new Variable(context, type, category, name, modifiers);

        return new VariableNode(variable);
    }
}
