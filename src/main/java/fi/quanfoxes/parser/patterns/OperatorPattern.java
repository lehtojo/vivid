package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.Errors;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.NumberToken;
import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Variable;
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

    private Node getNode(Context context, Token token) throws Exception {
        switch (token.getType()) {
            case TokenType.IDENTIFIER:
                IdentifierToken identifier = (IdentifierToken)token;
                Variable variable = context.getVariable(identifier.getValue());
                return new VariableNode(variable);
            case TokenType.NUMBER:
                NumberToken number = (NumberToken)token;
                return new NumberNode(number.getNumberType(), number.getNumber());
            case TokenType.PROCESSED:
                ProcessedToken processed = (ProcessedToken)token;
                return processed.getNode();
            default:
                throw new Exception("INTERNAL_ERROR");
        }
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        OperatorToken type = (OperatorToken)tokens.get(OPERATOR);
        OperatorNode operator = new OperatorNode(type.getOperator());

        Token left = tokens.get(LEFT);

        try {
            Node node = getNode(context, left);
            operator.add(node);
        }
        catch (Exception exception) {
            throw Errors.get(left.getPosition(), exception);
        }

        Token right = tokens.get(RIGHT);

        try {
            Node node = getNode(context, right);
            operator.add(node);
        }
        catch (Exception exception) {
            throw Errors.get(right.getPosition(), exception);
        }

        return operator;
    }
}
