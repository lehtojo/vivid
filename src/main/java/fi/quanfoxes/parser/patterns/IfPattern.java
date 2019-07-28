package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.Keywords;
import fi.quanfoxes.lexer.ContentToken;
import fi.quanfoxes.lexer.KeywordToken;
import fi.quanfoxes.lexer.ParenthesisType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Parser;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.DynamicToken;
import fi.quanfoxes.parser.nodes.ContentNode;
import fi.quanfoxes.parser.nodes.IfNode;

public class IfPattern extends Pattern {
    public static final int PRIORITY = 15;

    private static final int IF = 0;
    private static final int CONDITION = 1;
    private static final int BODY = 2;

    public IfPattern() {
        // Pattern:
        // if (...) {...}
        super(TokenType.KEYWORD, TokenType.DYNAMIC, TokenType.CONTENT);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(IF);

        // Keyword at the start must be 'if' since this pattern represents if statement
        if (keyword.getKeyword() != Keywords.IF) {
            return false;
        }

        DynamicToken condition = (DynamicToken)tokens.get(CONDITION);

        if (!(condition.getNode() instanceof ContentNode)) {
            return false;
        }

        ContentToken body = (ContentToken)tokens.get(BODY);

        return body.getParenthesisType() == ParenthesisType.CURLY_BRACKETS;
    }

    /**
     * Returns the if statement's condition in node tree form
     * @param tokens If statement pattern represented in tokens
     * @return If statement's condition in node tree form
     */
    private Node getCondition(List<Token> tokens) {
        DynamicToken token = (DynamicToken)tokens.get(CONDITION);
        ContentNode condition = (ContentNode)token.getNode();
        return condition.first();
    }

    /**
     * Tries to parse the body of the if statement
     * @param context Context to use while parsing
     * @param tokens If statement pattern represented in tokens
     * @return Parsed body in node tree form
     */
    private Node getBody(Context context, List<Token> tokens) throws Exception {
        ContentToken content = (ContentToken)tokens.get(BODY);
        return Parser.parse(context, content.getTokens()); 
    }

	@Override
	public Node build(Context environment, List<Token> tokens) throws Exception {
        // Create new context for if statement's body, which is linked to its environment
        Context context = new Context();
        context.link(environment);

        // Collect the components of this if statement
        Node condition = getCondition(tokens);
        Node body = getBody(context, tokens);
        
        // Build the components into a node
        return new IfNode(condition, body);
	}
}