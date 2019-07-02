package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.OperatorToken;
import fi.quanfoxes.Lexer.OperatorType;
import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Pattern;

import java.util.List;

public class UnarySignPattern extends Pattern {
    private static final int PRIORITY = 14;

    private static final int OPERATOR = 0;
    private static final int SIGN = 0;

    public UnarySignPattern() {
        super(TokenType.OPERATOR, TokenType.OPERATOR,
                TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.PROCESSED);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        OperatorToken sign = (OperatorToken)tokens.get(SIGN);

        return operator.getOperator() != OperatorType.INCREMENT && operator.getOperator() != OperatorType.DECREMENT &&
                (sign.getOperator() == OperatorType.ADD || sign.getOperator() == OperatorType.SUBTRACT);
    }

    @Override
    public Node build(Node parent, List<Token> tokens) {
        return null;
    }
}
