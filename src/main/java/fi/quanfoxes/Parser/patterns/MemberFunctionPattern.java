package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.AccessModifierKeyword;
import fi.quanfoxes.KeywordType;
import fi.quanfoxes.Keywords;
import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Parser;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.nodes.ContextNode;
import fi.quanfoxes.Parser.nodes.FunctionNode;

import java.util.List;

public class MemberFunctionPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int MODIFIER = 0;
    private static final int IDENTIFIER = 2;
    private static final int PARAMETERS = 3;
    private static final int BODY = 4;

    public MemberFunctionPattern() {
        // Pattern:
        // [private / protected / public] (func) (Name) ( (...) ) ( {...} )
        // Examples:
        // public func getThreadCount () {...}
        // protected func getSum (num a, num b) {...}
        super(TokenType.KEYWORD, TokenType.KEYWORD, TokenType.IDENTIFIER, TokenType.CONTENT, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken modifier = (KeywordToken)tokens.get(0);

        if (modifier.getKeyword().getType() != KeywordType.ACCESS_MODIFIER) {
            return false;
        }

        KeywordToken function = (KeywordToken)tokens.get(1);
        return function.getKeyword() == Keywords.FUNC;
    }

    private AccessModifierKeyword getAccessModifier(List<Token> tokens) {
        return (AccessModifierKeyword)((KeywordToken)tokens.get(MODIFIER)).getKeyword();
    }

    private IdentifierToken getIdentifier(List<Token> tokens) {
        return (IdentifierToken)tokens.get(IDENTIFIER);
    }

    private Node getParameters(List<Token> tokens) throws Exception {
        ContentToken content = (ContentToken)tokens.get(PARAMETERS);
        Node parameters = new Node();

        for (int i = 0; i < content.getSectionCount(); i++) {
            ContentToken section = content.getSection(i);
            Parser.parse(parameters, section.getTokens());
        }

        return parameters;
    }

    @Override
    public Node build(Node parent, List<Token> tokens) throws Exception {
        AccessModifierKeyword modifier = getAccessModifier(tokens);
        IdentifierToken identifier = getIdentifier(tokens);
        Node parameters = getParameters(tokens);
        ContentToken body = (ContentToken)tokens.get(BODY);

        ContextNode context = (ContextNode)parent;
        FunctionNode function =  new FunctionNode(identifier.getIdentifier(), modifier.getModifier(), parameters, body.getTokens());
        context.declare(function);

        return function;
    }
}
