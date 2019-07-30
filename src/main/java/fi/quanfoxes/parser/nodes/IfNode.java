package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class IfNode extends Node {
    private Context context;

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

    public Context getContext() {
        return context;
    }
}