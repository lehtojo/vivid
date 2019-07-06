package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.OperatorType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.nodes.*;

import java.util.List;

public class OperatorPattern extends Pattern {
    private static final int LEFT = 0;
    private static final int OPERATOR = 1;
    private static final int RIGHT = 2;

    public OperatorPattern() {
        // Pattern:
        // (Variable / Number / (...)) (Operator) (Variable / Number / (...))
        // Examples:
        // a * 777
        // 5 * b
        // -1 + (a + b)
        // (a * b) ^ 2
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
    public Node build(Context context, List<Token> tokens) throws Exception {
        Token left = tokens.get(LEFT);
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        Token right = tokens.get(RIGHT);

        return new OperatorNode(context, left, operator.getOperator(), right);
    }
}
