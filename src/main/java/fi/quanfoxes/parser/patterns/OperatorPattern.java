package fi.quanfoxes.parser.patterns;

import fi.quanfoxes.Errors;
import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.Singleton;
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
        super(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.PROCESSED, 
              TokenType.OPERATOR,
              TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.PROCESSED);
    }

    @Override
    public int priority(List<Token> tokens) {
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        return operator.getOperator().getPriority();
    }

    @Override
    public boolean passes(List<Token> tokens) {
        return true;
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        OperatorToken type = (OperatorToken)tokens.get(OPERATOR);
        OperatorNode operator = new OperatorNode(type.getOperator());

        Token left = tokens.get(LEFT);

        try {
            
            Node node = Singleton.parse(context, left);
            operator.add(node);
        }
        catch (Exception exception) {
            throw Errors.get(left.getPosition(), exception);
        }

        Token right = tokens.get(RIGHT);

        try {
            Node node = Singleton.parse(context, right);
            operator.add(node);
        }
        catch (Exception exception) {
            throw Errors.get(right.getPosition(), exception);
        }

        return operator;
    }
}
