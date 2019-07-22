package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.UnresolvedType;
import fi.quanfoxes.parser.nodes.DotOperatorNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.TypeNode;

import java.util.ArrayList;
import java.util.List;

public class MemberFunctionPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int MODIFIER = 0;
    private static final int RETURN_TYPE = 1;
    private static final int HEAD = 2;
    private static final int BODY = 3;

    public MemberFunctionPattern() {
        // Pattern:
        // [private / protected / public] (func) (Name) ( (...) ) ( {...} )
        // Examples:
        // public func getThreadCount () {...}
        // protected func getSum (num a, num b) {...}
        super(TokenType.KEYWORD, TokenType.KEYWORD | TokenType.IDENTIFIER | TokenType.PROCESSED, TokenType.FUNCTION, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken modifier = (KeywordToken)tokens.get(MODIFIER);

        if (modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
            return false;
        }

        Token token = tokens.get(RETURN_TYPE);

        switch (token.getType()) {

            case TokenType.KEYWORD:
                return ((KeywordToken)token).getKeyword() == Keywords.FUNC;

            case TokenType.IDENTIFIER:
                return true;
                
            case TokenType.PROCESSED:
                Node node = ((ProcessedToken)token).getNode();
                return (node instanceof DotOperatorNode) || (node instanceof TypeNode);
        }

        return false;
    }

    private AccessModifierKeyword getAccessModifier(List<Token> tokens) {
        return (AccessModifierKeyword)((KeywordToken)tokens.get(MODIFIER)).getKeyword();
    }

    private ArrayList<Token> getBody(List<Token> tokens) {
        return ((ContentToken)tokens.get(BODY)).getTokens();
    }

    private Type getReturnType(Context context, List<Token> tokens) throws Exception {
        Token token = tokens.get(RETURN_TYPE);

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

             case TokenType.PROCESSED:
                ProcessedToken processed = (ProcessedToken)token;
                Node node = processed.getNode();

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
        AccessModifierKeyword modifier = getAccessModifier(tokens);
        Type returnType = getReturnType(context, tokens);
        FunctionToken head = (FunctionToken)tokens.get(HEAD);
        ArrayList<Token> body = getBody(tokens);

        Function function = new Function(context, head.getName(), modifier.getModifier(), returnType);
        function.setParameters(head.getParameters(function));

        return new FunctionNode(function, body);
    }
}
