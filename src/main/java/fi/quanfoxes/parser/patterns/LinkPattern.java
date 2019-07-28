package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.DynamicToken;
import fi.quanfoxes.parser.Resolvable;
import fi.quanfoxes.parser.Singleton;
import fi.quanfoxes.parser.nodes.LinkNode;
import fi.quanfoxes.lexer.Operators;

public class LinkPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int LEFT = 0;
    private static final int OPERATOR = 1;
    private static final int RIGHT = 2;

    public LinkPattern() {
        // Pattern:
        // (Variable / Type / Processed) (.) (Variable / Type)
        super(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.DYNAMIC, 
              TokenType.OPERATOR, 
              TokenType.FUNCTION | TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        
        // The operator between left and right token must be dot
        if (operator.getOperator() != Operators.DOT) {
            return false;
        }

        // When left token is a processed, it must be contextable
        if (tokens.get(LEFT).getType() == TokenType.DYNAMIC) {
            DynamicToken token = (DynamicToken)tokens.get(LEFT);
            return (token.getNode() instanceof Contextable);
        }

        return true;
    }

    @Override
    public Node build(Context environment, List<Token> tokens) throws Exception {
        Node left = Singleton.parse(environment, tokens.get(LEFT));
        Node right;

        if (left instanceof Contextable) {
            Contextable contextable = (Contextable)left;
            Context primary = contextable.getContext();

            // Creates an unresolved node from right token if the primary context is unresolved
            if (primary == null || primary instanceof Resolvable) {
                right = Singleton.getUnresolved(environment, tokens.get(RIGHT));
            }
            else {
                right = Singleton.parse(environment, primary, tokens.get(RIGHT));
            }
        }
        else {
            right = Singleton.getUnresolved(environment, tokens.get(RIGHT));
        }

        return new LinkNode().setOperands(left, right);
    }
}