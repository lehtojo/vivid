package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Label;
import fi.quanfoxes.parser.Node;

public class LabelNode extends Node {
    private Label label;

    public LabelNode(Label label) {
        this.label = label;
    }

    public Label getLabel() {
        return label;
    }

    @Override
    public NodeType getNodeType() {
        return NodeType.LABEL_NODE;
    }
}