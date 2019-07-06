package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.nodes.FunctionNode;

import java.util.ArrayList;
import java.util.List;

public class MemberFunctionPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int MODIFIER = 0;
    private static final int HEAD = 2;
    private static final int BODY = 3;

    public MemberFunctionPattern() {
        // Pattern:
        // [private / protected / public] (func) (Name) ( (...) ) ( {...} )
        // Examples:
        // public func getThreadCount () {...}
        // protected func getSum (num a, num b) {...}
        super(TokenType.KEYWORD, TokenType.KEYWORD, TokenType.FUNCTION, TokenType.CONTENT);
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

        KeywordToken function = (KeywordToken)tokens.get(1);
        return function.getKeyword() == Keywords.FUNC;
    }

    private AccessModifierKeyword getAccessModifier(List<Token> tokens) {
        return (AccessModifierKeyword)((KeywordToken)tokens.get(MODIFIER)).getKeyword();
    }

    private Node getParameters(Function function, ContentToken content) throws Exception {
        Node parameters = new Node();

        for (int i = 0; i < content.getSectionCount(); i++) {
            ArrayList<Token> tokens = content.getTokens(i);
            Parser.parse(parameters, function, tokens, VariablePattern.PRIORITY);
        }

        return parameters;
    }

    private ArrayList<Token> getBody(List<Token> tokens) {
        return ((ContentToken)tokens.get(BODY)).getTokens();
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        AccessModifierKeyword modifier = getAccessModifier(tokens);
        FunctionToken head = (FunctionToken)tokens.get(HEAD);
        ArrayList<Token> body = getBody(tokens);

        Function function = new Function(context, head.getName(), modifier.getModifier());
        function.setParameters(getParameters(function, head.getParameters()));

        return new FunctionNode(function, body);
    }
}
