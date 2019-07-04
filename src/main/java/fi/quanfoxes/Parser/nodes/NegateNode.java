package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Parser.Node;

public class NegateNode extends Node {
    public NegateNode(Node object) {
        add(object);
    }
}
