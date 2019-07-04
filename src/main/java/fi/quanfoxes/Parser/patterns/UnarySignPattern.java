package fi.quanfoxes.Parser.patterns;

import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.Pattern;
import fi.quanfoxes.Parser.ProcessedToken;
import fi.quanfoxes.Parser.nodes.ContextNode;
import fi.quanfoxes.Parser.nodes.NegateNode;
import fi.quanfoxes.Parser.nodes.NumberNode;

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

    private Node getNode(Node parent, Token token) throws Exception {
        switch (token.getType()) {
            case TokenType.IDENTIFIER:
                ContextNode context = (ContextNode)parent;
                IdentifierToken identifier = (IdentifierToken)token;
                return context.getVariable(identifier.getIdentifier());
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
    public Node build(Node parent, List<Token> tokens) throws Exception {
        OperatorToken sign = (OperatorToken)tokens.get(SIGN);
        Node object = getNode(parent, tokens.get(OBJECT));

        if (sign.getOperator() == OperatorType.ADD) {
            return object;
        }

        return new NegateNode(object);
    }

    @Override
    public int getStart(List<Token> tokens) {
        return super.getStart(tokens);
    }
}
