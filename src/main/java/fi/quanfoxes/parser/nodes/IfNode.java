package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class IfNode extends Node {
    private Context context;
    private Node successor;

    public IfNode(Context context, Node condition, Node body) {
        this.context = context;
        super.add(condition);
        super.add(body);
    }

    public Node getCondition() {
        return first();
    }

    public Node getBody() {
        return last();
    }

    public void setSuccessor(Node successor) {
        this.successor = successor;
        this.insert(last(), successor);
    }

    public Node getSuccessor() {
        return successor;
    }

    public Context getContext() {
        return context;
    }

    @Override
    public NodeType getNodeType() {
        return NodeType.IF_NODE;
    }
}