package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Lexer.*;
import fi.quanfoxes.Parser.Node;
import fi.quanfoxes.Parser.ProcessedToken;

public class OperatorNode extends Node {
    private OperatorType operation;

    private Node getNode(ContextNode parent, Token token) throws Exception {
        switch (token.getType()) {
            case TokenType.IDENTIFIER:
                IdentifierToken identifier = (IdentifierToken)token;
                return parent.getVariable(identifier.getIdentifier());
            case TokenType.NUMBER:
                NumberToken number = (NumberToken)token;
                return new NumberNode(number.getNumberType(), number.getNumber());
            case TokenType.PROCESSED:
                ProcessedToken program = (ProcessedToken)token;
                return program.getNode();
            default:
                throw new Exception("INTERNAL_ERROR: Unhandled token");
        }
    }

    public OperatorNode(ContextNode parent, Token left, OperatorType operation, Token right) throws Exception {
        this.operation = operation;
        super.add(getNode(parent, left));
        super.add(getNode(parent, right));
    }

    public OperatorNode(OperatorType operation) {
        this.operation = operation;
    }

    public OperatorType getOperation() {
        return operation;
    }
}
