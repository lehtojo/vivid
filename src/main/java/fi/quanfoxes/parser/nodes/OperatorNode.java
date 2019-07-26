package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolver;

public class OperatorNode extends Node implements Contextable {
    private OperatorType operator;

    public OperatorNode(OperatorType operator) {
        this.operator = operator;
    }

    public OperatorType getOperator() {
        return operator;
    }

    public OperatorNode setOperands(Node left, Node right) {
        super.add(left);
        super.add(right);
        return this;
    }

    public Node getLeft() {
        return first();
    }

    public Node getRight() {
        return last();
    }

    @Override
    public Context getContext() throws Exception {
        Context left;

        if (getLeft() instanceof Contextable) {
            Contextable contextable = (Contextable)getLeft();
            left = contextable.getContext();
        }
        else {
            return null;
        }

        Context right;

        if (getRight() instanceof Contextable) {
            Contextable contextable = (Contextable)getRight();
            right = contextable.getContext();
        }
        else {
            return null;
        }

        return Resolver.getSharedContext(left, right);
    }
}
