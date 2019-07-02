package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.OperatorToken;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.nodes.*;

import java.util.List;

public class OperatorPattern extends Pattern {
    private static final int LEFT = 0;
    private static final int OPERATOR = 1;
    private static final int RIGHT = 2;

    public OperatorPattern() {
        super(TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.PROCESSED, TokenType.OPERATOR,
                    TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.PROCESSED);
    }

    @Override
    public int priority(List<Token> tokens) {
        final OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        return operator.getOperator().getPriority();
    }

    @Override
    public boolean passes(List<Token> tokens) {
        return true;
    }

    @Override
    public Node build(Node parent, List<Token> tokens) throws Exception {
        Token left = tokens.get(LEFT);
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        Token right = tokens.get(RIGHT);

        return new OperatorNode((ContextNode) parent, left, operator.getOperator(), right);
    }
}
