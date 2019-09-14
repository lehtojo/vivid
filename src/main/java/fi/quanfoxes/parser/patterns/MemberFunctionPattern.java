package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.Errors;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.DynamicToken;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.UnresolvedType;
import fi.quanfoxes.parser.nodes.NodeType;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;

import java.util.List;

public class MemberFunctionPattern extends Pattern {
    public static final int PRIORITY = 20;

    private static final int MODIFIER = 0;
    private static final int RETURN_TYPE = 2;
    private static final int HEAD = 3;
    private static final int BODY = 5;

    public MemberFunctionPattern() {
        // Pattern:
        // [private / protected / public] [static] Type / Type.Subtype / func ... (...) [\n] {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, /* [private / protected / public] */
              TokenType.KEYWORD | TokenType.OPTIONAL, /* [static] */
              TokenType.KEYWORD | TokenType.IDENTIFIER | TokenType.DYNAMIC, /* Type / Type.Subtype / func */
              TokenType.FUNCTION | TokenType.IDENTIFIER, /* ... [(...)] */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.CONTENT); /* {...} */
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
        
        Token token = tokens.get(RETURN_TYPE);

        switch (token.getType()) {

            case TokenType.KEYWORD: {
                return ((KeywordToken)token).getKeyword() == Keywords.FUNC;
            }
                
            case TokenType.IDENTIFIER: {
                return true;
            }
                             
            case TokenType.DYNAMIC: {
                Node node = ((DynamicToken)token).getNode();
                return node.getNodeType() == NodeType.TYPE_NODE || node.getNodeType() == NodeType.LINK_NODE;
            }            
        }

        ContentToken body = (ContentToken)tokens.get(BODY);
        return body.getParenthesisType() == ParenthesisType.CURLY_BRACKETS;
    }

    private int getModifiers(List<Token> tokens) {
        KeywordToken first = (KeywordToken)tokens.get(MODIFIER);
        KeywordToken second = (KeywordToken)tokens.get(MODIFIER + 1);

        int modifiers = AccessModifier.PUBLIC;

        if (first != null) {
            KeywordToken keyword = (KeywordToken)first;
            AccessModifierKeyword modifier = (AccessModifierKeyword)keyword.getKeyword();
            modifiers |= modifier.getModifier();

            if (second != null) {
                keyword = (KeywordToken)second;
                modifier = (AccessModifierKeyword)keyword.getKeyword();
                modifiers |= modifier.getModifier();
            }
        }

        return modifiers;
    }

    private List<Token> getBody(List<Token> tokens) {
        return ((ContentToken)tokens.get(BODY)).getTokens();
    }

    private Type getReturnType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(RETURN_TYPE);

        switch (token.getType()) {

            case TokenType.KEYWORD: {
                return Types.UNKNOWN;
            }
                
            case TokenType.IDENTIFIER: {
                IdentifierToken id = (IdentifierToken)token;
                
                if (context.isTypeDeclared(id.getValue())) {
                    return context.getType(id.getValue());
                }
                else {
                    return new UnresolvedType(context, id.getValue());
                }
            }
                
            case TokenType.DYNAMIC: {
                DynamicToken dynamic = (DynamicToken)token;
                Node node = dynamic.getNode();

                if (node.getNodeType() == NodeType.TYPE_NODE) {
                    TypeNode type = (TypeNode)node;
                    return type.getType();
                }
                else if (node instanceof Resolvable) {
                    return new UnresolvedType(context, (Resolvable)node);
                }
            }            
        }

        throw Errors.get(tokens.get(HEAD).getPosition(), "Couldn't resolve return type");
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {  
        Function function;

        int modifiers = getModifiers(tokens);
        Type result = getReturnType(context, tokens);
        List<Token> body = getBody(tokens);
      
        Token token = tokens.get(HEAD);

        if (token.getType() == TokenType.FUNCTION) {
            FunctionToken head = (FunctionToken)token;
            function = new Function(context, head.getName(), modifiers, result);
            function.setParameters(head.getParameters(function));
        }
        else {
            IdentifierToken name = (IdentifierToken)token;
            function = new Function(context, name.getValue(), modifiers, result);
        }

        return new FunctionNode(function, body);
    }
}
