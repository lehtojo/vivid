package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.FunctionToken;
import fi.quanfoxes.lexer.IdentifierToken;
import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.OperatorType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Function;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Type;
import fi.quanfoxes.parser.Variable;
import fi.quanfoxes.parser.nodes.DotOperatorNode;
import fi.quanfoxes.parser.nodes.FunctionNode;
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

    private Node getUnresolved(Context context, Token token) throws Exception {
        switch(token.getType()) {
            case TokenType.FUNCTION:
                FunctionToken function = (FunctionToken)token;
                return new UnresolvedFunction(function.getName()).setParameters(function.getParameters(context));

            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;
                return new UnresolvedIdentifier(id.getValue());

            default:
                throw new Exception("INTERNAL_ERROR");
        }
    }

    private Node getNode(Context base, Node left, Token token) throws Exception {
        Context context = base;

        // Check if the right side is being resolved
        if (left != null) {
            // Try to get the return context of the left side
            if (left instanceof Contextable) {
                Contextable contextable = (Contextable)left;
                context = contextable.getContext();
            }
            else {
                // Since the left side wasn't contextable, it means it couldn't be resolved
                // When left side isn't resolved, the right side cannot be resolved
                return getUnresolved(base, token);
            }
        }

        switch(token.getType()) {

            case TokenType.FUNCTION:
                FunctionToken properties = (FunctionToken)token;
                Node parameters = properties.getParameters(base);

                if (context.isFunctionDeclared(properties.getName())) {
                    Function function = context.getFunction(properties.getName());
                    return new FunctionNode(function).setParameters(parameters);
                }
                else {
                    return new UnresolvedFunction(properties.getName()).setParameters(parameters);
                }

            case TokenType.IDENTIFIER:
                IdentifierToken id = (IdentifierToken)token;

                if (context.isVariableDeclared(id.getValue())) {
                    Variable variable = context.getVariable(id.getValue());
                    return new VariableNode(variable);
                }
                else if (context.isTypeDeclared(id.getValue())) {
                    Type type = context.getType(id.getValue());
                    return new TypeNode(type);
                }
                else {
                    return new UnresolvedIdentifier(id.getValue());
                }

            case TokenType.PROCESSED:
                ProcessedToken processed = (ProcessedToken)token;
                return processed.getNode();

            default:
                throw new Exception("INTERNAL_ERROR");
        }
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        Node left = getNode(context, null, tokens.get(LEFT));
        Node right = getNode(context, left, tokens.get(RIGHT));

        DotOperatorNode dot = new DotOperatorNode();
        dot.setOperands(left, right);

        return dot;
    }
}