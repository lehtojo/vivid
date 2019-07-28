package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifier;
import fi.quanfoxes.AccessModifierKeyword;
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
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;

import java.util.List;

public class MemberFunctionPattern extends Pattern {
    public static final int PRIORITY = 20;

    private static final int RETURN_TYPE = 0;
    private static final int HEAD = 1;
    private static final int BODY = 2;

    private static final int UNMODIFIED_LENGTH = 3;

    public MemberFunctionPattern() {
        // Pattern:
        // [private / protected / public] [static] Type / Type.Subtype / func ... (...) {...}
        super(TokenType.KEYWORD | TokenType.OPTIONAL, TokenType.KEYWORD | TokenType.OPTIONAL, TokenType.KEYWORD | TokenType.IDENTIFIER | TokenType.DYNAMIC, TokenType.FUNCTION, TokenType.CONTENT);
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
        
        Token token = tokens.get(count + RETURN_TYPE);

        switch (token.getType()) {
            case TokenType.KEYWORD:
                return ((KeywordToken)token).getKeyword() == Keywords.FUNC;
            case TokenType.IDENTIFIER:
                return true;             
            case TokenType.DYNAMIC:
                Node node = ((DynamicToken)token).getNode();
                return (node instanceof LinkNode) || (node instanceof TypeNode);
        }

        return false;
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

    private List<Token> getBody(List<Token> tokens, int start) {
        return ((ContentToken)tokens.get(start + BODY)).getTokens();
    }

    private Type getReturnType(Context context, List<Token> tokens, int start) throws Exception {
        Token token = tokens.get(start + RETURN_TYPE);

        switch (token.getType()) {

            case TokenType.KEYWORD:
                return Types.UNKNOWN;

            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;
                
                if (context.isTypeDeclared(id.getValue())) {
                    return context.getType(id.getValue());
                }
                else {
                    return new UnresolvedType(context, id.getValue());
                }

             case TokenType.DYNAMIC:
                DynamicToken dynamic = (DynamicToken)token;
                Node node = dynamic.getNode();

                if (node instanceof TypeNode) {
                    TypeNode type = (TypeNode)node;
                    return type.getType();
                }
                else if (node instanceof Resolvable) {
                    return new UnresolvedType(context, (Resolvable)node);
                }
        }

        throw new Exception("INTERNAL_ERROR");
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {    
        int count = tokens.size() - UNMODIFIED_LENGTH;

        int modifiers = getModifiers(tokens, count);
        Type result = getReturnType(context, tokens, count);
        FunctionToken head = (FunctionToken)tokens.get(count + HEAD);
        List<Token> body = getBody(tokens, count);

        Function function = new Function(context, head.getName(), modifiers, result);
        function.setParameters(head.getParameters(function));

        return new FunctionNode(function, body);
    }
}
