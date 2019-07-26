package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class IfNode extends Node {
    private Context context;

    public IfNode(Node condition, Node body) {
        super.add(condition);
        super.add(body);
    }

    public Node getCondition() {
        return first();
    }

    public Node getBody() {
        return last();
    }

    public Context getContext() {
        return context;
    }
}