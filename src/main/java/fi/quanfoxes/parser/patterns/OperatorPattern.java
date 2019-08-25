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
    private static final int OPERATOR = 2;
    private static final int RIGHT = 4;

    public OperatorPattern() {
        // Pattern:
        // Function / Variable / Number / (...) [\n] Operator [\n] Function / Variable / Number / (...)
        super(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC, /* Function / Variable / Number / (...) */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.OPERATOR, /* Operator */
              TokenType.END | TokenType.OPTIONAL, /* [\n] */
              TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.NUMBER | TokenType.STRING | TokenType.DYNAMIC); /* Function / Variable / Number / (...) */
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
