package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.nodes.NegateNode;

import java.util.List;

public class UnarySignPattern extends Pattern {
    private static final int PRIORITY = 14;

    private static final int OPERATOR = 0;
    private static final int SIGN = 1;
    private static final int OBJECT = 2;

    public UnarySignPattern() {
        super(TokenType.OPERATOR, TokenType.OPERATOR,
                TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.DYNAMIC);
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

    private boolean isPositive(OperatorToken sign) {
        return sign.getOperator() == OperatorType.ADD;
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        OperatorToken sign = (OperatorToken)tokens.get(SIGN);
        Node node = Singleton.parse(context, tokens.get(OBJECT));

        return isPositive(sign) ? node : new NegateNode(node);
    }

    @Override
    public int start() {
        return SIGN;
    }
}
