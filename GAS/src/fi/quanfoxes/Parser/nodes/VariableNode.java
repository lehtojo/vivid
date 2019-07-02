package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Parser.Node;

public class VariableNode extends Node {
    private String identifier;
    private TypeNode type;

    public VariableNode(String identifier, TypeNode type) {
        this.identifier = identifier;
        this.type = type;
    }

    public String getIdentifier() {
        return identifier;
    }

    public TypeNode getType() {
        return type;
    }
}
