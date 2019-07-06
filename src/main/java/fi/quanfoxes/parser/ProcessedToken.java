package fi.quanfoxes.parser;

import fi.quanfoxes.lexer.Token;
import fi.quanfoxes.lexer.TokenType;

import java.util.Objects;

public class ProcessedToken extends Token {
    private Node node;

    public ProcessedToken(Node node) {
        super(TokenType.PROCESSED);
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
        ProcessedToken token = (ProcessedToken) o;
        return Objects.equals(node, token.node);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), node);
    }
}
