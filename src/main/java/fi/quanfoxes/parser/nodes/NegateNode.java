package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Node;

public class NegateNode extends Node {
    public NegateNode(Node object) {
        super.add(object);
    }
}
