package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Node;

public class WhileNode extends Node {
    public WhileNode(Node condition, Node body) {
        super.add(condition);
        super.add(body);
    }

    public Node getCondition() {
        return getFirst();
    }

    public Node getBody() {
        return getLast();
    }
}
