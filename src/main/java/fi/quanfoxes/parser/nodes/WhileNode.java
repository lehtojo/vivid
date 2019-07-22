package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class WhileNode extends Node {
    private Context context;

    public WhileNode(Context context, Node condition, Node body) {
        this.context = context;
        super.add(condition);
        super.add(body);
    }

    public Context getContext() {
        return context;
    }

    public Node getCondition() {
        return getFirst();
    }

    public Node getBody() {
        return getLast();
    }
}
