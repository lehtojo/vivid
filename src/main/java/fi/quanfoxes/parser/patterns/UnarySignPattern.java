package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.nodes.NegateNode;
import fi.quanfoxes.parser.nodes.NodeType;
import fi.quanfoxes.parser.nodes.NumberNode;

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

        return operator.getOperator() != Operators.INCREMENT && operator.getOperator() != Operators.DECREMENT &&
                (sign.getOperator() == Operators.ADD || sign.getOperator() == Operators.SUBTRACT);
    }

    private boolean isNegative(OperatorToken sign) {
        return sign.getOperator() == Operators.SUBTRACT;
    }

    private Node getNegativeNode(Node node) {
        if (node.getNodeType() == NodeType.NUMBER_NODE) {
            NumberNode number = (NumberNode)node;
            number.setValue(-number.getValue().longValue());

            return number;
        }

        return new NegateNode(node);
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        OperatorToken sign = (OperatorToken)tokens.get(SIGN);
        Node node = Singleton.parse(context, tokens.get(OBJECT));

        if (isNegative(sign)) {
            return getNegativeNode(node);
        }

        return node;
    }

    @Override
    public int start() {
        return SIGN;
    }
}
