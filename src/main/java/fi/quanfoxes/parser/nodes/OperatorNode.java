package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.ProcessedToken;
import fi.quanfoxes.parser.Variable;

public class OperatorNode extends Node {
    private OperatorType operator;

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
                ProcessedToken processed = (ProcessedToken)token;
                return processed.getNode();
            default:
                throw new Exception("INTERNAL_ERROR: Unhandled token");
        }
    }

    public OperatorNode(Context context, Token left, OperatorType operator, Token right) throws Exception {
        this.operator = operator;

        super.add(getNode(context, left));
        super.add(getNode(context, right));
    }

    public OperatorNode(OperatorType operator) {
        this.operator = operator;
    }

    public OperatorType getOperator() {
        return operator;
    }
}
