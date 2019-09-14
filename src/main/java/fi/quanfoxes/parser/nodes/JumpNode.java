package fi.quanfoxes.parser.nodes;

import fi.quanfoxes.parser.Label;
import fi.quanfoxes.parser.Node;

public class JumpNode extends Node {
    private Label label;

    public JumpNode(Label label) {
        this.label = label;
    }

    public Label getLabel() {
        return label;
    }

    @Override
    public NodeType getNodeType() {
        return NodeType.JUMP_NODE;
    }
}