package fi.quanfoxes.Parser.nodes;

public class TypeNode extends ContextNode {
    private String identifier;
    private int access;

    public TypeNode(String identifier, int access) {
        this.identifier = identifier;
    }

    public String getIdentifier() {
        return identifier;
    }
}
