package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolver;

public class OperatorNode extends Node implements Contextable {
    private Operator operator;

    public OperatorNode(Operator operator) {
        this.operator = operator;
    }

    public Operator getOperator() {
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

    private Context getClassicContext() throws Exception {
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

    private Context getComparisonContext() {
        return Types.BOOL;
    }

    private Context getActionContext() throws Exception {
        if (getLeft() instanceof Contextable) {
            Contextable contextable = (Contextable)getLeft();
            return contextable.getContext();
        }
        
        return null;
    }

    @Override
    public Context getContext() throws Exception {
        switch (operator.getType()) {
            case CLASSIC:
                return getClassicContext();
            case COMPARISON:
                return getComparisonContext();
            case ACTION:
                return getActionContext();
            default:
                throw new Exception("Independent operator doesn't belong here");
        }
    }
}
