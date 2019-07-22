package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.FunctionToken;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.NumberToken;
import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.OperatorType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.nodes.DotOperatorNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
import fi.quanfoxes.parser.nodes.NumberNode;
import fi.quanfoxes.parser.nodes.TypeNode;
import fi.quanfoxes.parser.nodes.UnresolvedFunction;
import fi.quanfoxes.parser.nodes.UnresolvedIdentifier;
import fi.quanfoxes.parser.nodes.VariableNode;

public class DotPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int LEFT = 0;
    private static final int OPERATOR = 1;
    private static final int RIGHT = 2;

    public DotPattern() {
        // Pattern:
        // (Variable / Type / Processed) (.) (Variable / Type)
        // Examples:
        // thread_pool.thread_count     => DotOperator { VariableNode, VariableNode } ?
        // thread_pool.start()          => DotOperator { VariableNode, FunctionNode }
        // get_configuration().save()   => DotOperator { FunctionNode, FunctionNode }
        // ThreadPool.create()          => FunctionNode
        // ThreadPool.Worker            => TypeNode
        super(TokenType.FUNCTION | TokenType.IDENTIFIER | TokenType.PROCESSED, 
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
        if (operator.getOperator() != OperatorType.DOT) {
            return false;
        }

        // When left token is a processed, it must be contextable
        if (tokens.get(LEFT).getType() == TokenType.PROCESSED) {
            ProcessedToken token = (ProcessedToken)tokens.get(LEFT);
            return (token.getNode() instanceof Contextable);
        }

        return true;
    }

    private Node getUnresolved(Context environment, Token token) throws Exception {
        switch(token.getType()) {
            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;
                return new UnresolvedIdentifier(id.getValue());
            case TokenType.FUNCTION:
                FunctionToken function = (FunctionToken)token;
                return new UnresolvedFunction(function.getName())
                                .setParameters(function.getParameters(environment));
        }
        
        throw new Exception("Couldn't resolve token");
    }

    private Node getNode(Context environment, Context primary, Token token) throws Exception {
        switch(token.getType()) {
            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;

                if (primary.isVariableDeclared(id.getValue())) {
                    return new VariableNode(primary.getVariable(id.getValue()));
                }
                else if (primary.isTypeDeclared(id.getValue())) {
                    return new TypeNode(primary.getType(id.getValue()));
                }
                else {
                    return getUnresolved(environment, token);
                }

            case TokenType.FUNCTION:
                FunctionToken function = (FunctionToken)token;

                if (primary.isFunctionDeclared(function.getName())) {
                    return new FunctionNode(primary.getFunction(function.getName()));
                }
                else {
                    return getUnresolved(environment, token);
                }

            case TokenType.NUMBER:
                NumberToken number = (NumberToken)token;
                return new NumberNode(number.getNumberType(), number.getNumber());

            case TokenType.PROCESSED:
                ProcessedToken processed = (ProcessedToken)token;
                return processed.getNode();
        }

        throw new Exception("Couldn't resolve token");
    }

    @Override
    public Node build(Context environment, List<Token> tokens) throws Exception {
        Node left = getNode(environment, environment, tokens.get(LEFT));
        Node right;

        if (left instanceof Contextable) {
            Contextable contextable = (Contextable)left;
            Context primary = contextable.getContext();
            right = getNode(environment, primary, tokens.get(RIGHT));
        }
        else {
            right = getUnresolved(environment, tokens.get(RIGHT));
        }

        DotOperatorNode dot = new DotOperatorNode();
        dot.setOperands(left, right);

        return dot;
    }
}