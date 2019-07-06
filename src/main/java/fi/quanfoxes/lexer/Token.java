package fi.quanfoxes.lexer;

import java.util.Objects;

public abstract class Token {
    private int type;

    public Token(final int type) {
        this.type = type;
    }

    public abstract String getText();

    public int getType() {
        return type;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof Token)) return false;
        Token token = (Token) o;
        return type == token.type;
    }

    @Override
    public int hashCode() {
        return Objects.hash(type);
    }
}
