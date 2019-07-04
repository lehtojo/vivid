package fi.quanfoxes.Parser.nodes;

import fi.quanfoxes.Lexer.Token;

import java.util.ArrayList;

public class TypeNode extends ContextNode {
    private String identifier;
    private int access;

    public TypeNode(String identifier, int access) {
        this.identifier = identifier;
        this.access = access;
    }

    public String getIdentifier() {
        return identifier;
    }
}
