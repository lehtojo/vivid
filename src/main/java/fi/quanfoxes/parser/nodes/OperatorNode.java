package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.Types;
import fi.quanfoxes.lexer.*;
import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Contextable;
import fi.quanfoxes.parser.Node;
import fi.quanfoxes.parser.Resolver;
import fi.quanfoxes.parser.Type;

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
        Type left;

        if (getLeft() instanceof Contextable) {
            Contextable contextable = (Contextable)getLeft();
            Context context = contextable.getContext();

            if (!context.isType()) {
                return null;
            }

            if (!((ClassicOperator)operator).isSharedContext()) {
                return (Type)context;
            }

            left = (Type)context;
        }
        else {
            return null;
        }

        Type right;

        if (getRight() instanceof Contextable) {
            Contextable contextable = (Contextable)getRight();
            Context context = contextable.getContext();

            if (!context.isType()) {
                return null;
            }

            right = (Type)context;
        }
        else {
            return null;
        }

        return Resolver.getSharedType(left, right);
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

    @Override
    public NodeType getNodeType() {
        return NodeType.OPERATOR_NODE;
    }
}
