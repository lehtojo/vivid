package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.NegateNode;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.VariableNode;

import java.util.List;

public class UnarySignPattern extends Pattern {
    private static final int PRIORITY = 14;

    private static final int OPERATOR = 0;
    private static final int SIGN = 1;
    private static final int OBJECT = 2;

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

    private Node getNode(Context context, Token token) throws Exception {
        switch (token.getType()) {
            case TokenType.IDENTIFIER:
                IdentifierToken identifier = (IdentifierToken)token;
                Variable variable = context.getVariable(identifier.getIdentifier());
                return new VariableNode(variable);
            case TokenType.NUMBER:
                NumberToken number = (NumberToken)token;
                return new NumberNode(number.getNumberType(), number.getNumber());
            case TokenType.PROCESSED:
                ProcessedToken process = (ProcessedToken)token;
                return process.getNode();
        }

        throw new Exception("INTERNAL_ERROR: Unhandled token");
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        OperatorToken sign = (OperatorToken)tokens.get(SIGN);
        Node object = getNode(context, tokens.get(OBJECT));

        if (sign.getOperator() == OperatorType.ADD) {
            return object;
        }

        return new NegateNode(object);
    }

    @Override
    public int start() {
        return SIGN;
    }
}
