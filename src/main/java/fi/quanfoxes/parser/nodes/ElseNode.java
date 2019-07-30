package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Context;
import fi.quanfoxes.parser.Node;

public class ElseNode extends Node {
    private Context context;

    public ElseNode(Context context, Node body) {
        this.context = context;
        super.add(body);
    }

    public Node getBody() {
        return first();
    }

    public Context getContext() {
        return context;
    }
}