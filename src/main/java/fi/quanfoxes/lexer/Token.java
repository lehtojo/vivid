package fi.quanfoxes.lexer;

import java.util.Objects;

import fi.quanfoxes.lexer.Lexer.Position;

public abstract class Token {
    private int type;
    private Position position;

    public Token(final int type) {
        this.type = type;
    }

    public abstract String getText();

    public void setPosition(Position position) {
        this.position = position;
    }

    public Position getPosition() {
        return position.clone();
    }

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
