package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.Errors;
import fi.quanfoxes.Keyword;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.*;
import fi.quanfoxes.parser.nodes.*;

import java.util.List;

public class VariablePattern extends Pattern {
    public static final int PRIORITY = 17;

    private static final int TYPE = 0;
    private static final int NAME = 1;

    public VariablePattern() {
        // Pattern:
        // Type / Type.Subtype ...
        super(TokenType.IDENTIFIER | TokenType.KEYWORD | TokenType.DYNAMIC, TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.DYNAMIC) {
            DynamicToken dynamic = (DynamicToken)token;
            return dynamic.getNode().getNodeType() == NodeType.LINK_NODE;
        }
        else if (token.getType() == TokenType.KEYWORD) {
            Keyword keyword = ((KeywordToken)token).getKeyword();
            return keyword == Keywords.VAR;
        }

        return true;
    }

    private Type getType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(TYPE);

        if (token.getType() == TokenType.DYNAMIC) {
            DynamicToken dynamic = (DynamicToken)token;
            Node node = dynamic.getNode();

            if (node.getNodeType() == NodeType.LINK_NODE) {
                return new UnresolvedType(context, (Resolvable)node);
            }

            throw Errors.get(tokens.get(NAME).getPosition(), "Couldn't resolve type of the variable '%s'", getName(tokens));
        }
        else if (token.getType() == TokenType.KEYWORD) {
            return Types.UNKNOWN;
        }
        else {
            IdentifierToken id = (IdentifierToken)token;

            if (context.isTypeDeclared(id.getValue())) {
                return context.getType(id.getValue());
            }
            else {
                return new UnresolvedType(context, id.getValue());
            }
        }
    }

    private String getName(List<Token> tokens) {
        return ((IdentifierToken)tokens.get(NAME)).getValue();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        Type type = getType(context, tokens);
        String name = getName(tokens);
        VariableType category = context.isGlobalContext() ? VariableType.GLOBAL : VariableType.LOCAL;

        if (context.isLocalVariableDeclared(name)) {
            throw Errors.get(tokens.get(0).getPosition(), String.format("Variable '%s' already exists in this context", name));
        }

        Variable variable = new Variable(context, type, category, name, AccessModifier.PUBLIC);
       
        return new VariableNode(variable);
    }
}
