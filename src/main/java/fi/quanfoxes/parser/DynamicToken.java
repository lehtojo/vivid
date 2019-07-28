package fi.quanfoxes.parser;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;

import java.util.Objects;

public class DynamicToken extends Token {
    private Node node;

    public DynamicToken(Node node) {
        super(TokenType.DYNAMIC);
        this.node = node;
    }

    public Node getNode() {
        return node;
    }

    @Override
    public String getText() {
        return node.toString();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        if (!super.equals(o)) return false;
        DynamicToken token = (DynamicToken) o;
        return Objects.equals(node, token.node);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), node);
    }
}
