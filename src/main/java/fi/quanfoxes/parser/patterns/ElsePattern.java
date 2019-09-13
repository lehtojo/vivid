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
import fi.quanfoxes.parser.nodes.ElseNode;

public class ElsePattern extends Pattern {
    public static final int PRIORITY = 15;

    private static final int ELSE = 0;
    private static final int BODY = 2;

    public ElsePattern() {
        // Pattern:
        // else [\n] {...}
        super(
                TokenType.KEYWORD, /* else */
                TokenType.END | TokenType.OPTIONAL, /* [\n] */
                TokenType.CONTENT /* {...} */
        );
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        KeywordToken keyword = (KeywordToken)tokens.get(ELSE);

        if (keyword.getKeyword() != Keywords.ELSE) {
            return false;
        }

        ContentToken body = (ContentToken)tokens.get(BODY);
        return body.getParenthesisType() == ParenthesisType.CURLY_BRACKETS;
    }

    private List<Token> getBody(List<Token> tokens) {
        ContentToken body = (ContentToken)tokens.get(BODY);
        return body.getTokens();
    }

	@Override
	public Node build(Context environment, List<Token> tokens) throws Exception {
        Context context = new Context();
        context.link(environment);

        List<Token> body = getBody(tokens);
        Node node = Parser.parse(context, body);

        return new ElseNode(context, node);
	}
}