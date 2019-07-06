package fi.quanfoxes.parser.patterns;

import java.util.List;

import fi.quanfoxes.lexer.OperatorToken;
import fi.quanfoxes.lexer.OperatorType;
import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Pattern;

public class DotOperatorPattern extends Pattern {
    public static final int PRIORITY = 19;

    private static final int LEFT = 0;
    private static final int OPERATOR = 1;
    private static final int RIGHT = 2;

    public DotOperatorPattern() {
        // Pattern:
        // (Variable / Type / Processed) (.) (Variable / Type)
        // Examples:
        // thread_pool.thread_count     => DotOperator { VariableNode, VariableNode } ?
        // thread_pool.start()          => DotOperator { VariableNode, FunctionNode }
        // get_configuration().save()   => DotOperator { FunctionNode, FunctionNode }
        // ThreadPool.create()          => FunctionNode
        // ThreadPool.Worker            => TypeNode
        super(TokenType.IDENTIFIER | TokenType.PROCESSED, TokenType.OPERATOR, TokenType.IDENTIFIER);
    }

    @Override
    public int priority(List<Token> tokens) {
        return PRIORITY;
    }

    @Override
    public boolean passes(List<Token> tokens) {
        OperatorToken operator = (OperatorToken)tokens.get(OPERATOR);
        return operator.getOperator() == OperatorType.DOT;
    }

    private Node getNode(Token token) {
        return null;
    }

    @Override
    public Node build(Context context, List<Token> tokens) throws Exception {
        return null;
    }
}