package fi.quanfoxes.lexer;

import java.util.Objects;

public class IdentifierToken extends Token {
    private String name;

    public IdentifierToken(String name) {
        super(TokenType.IDENTIFIER);
        this.name = name;
    }

    public String getIdentifier() {
        return name;
    }

    @Override
    public String getText() {
        return name;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof IdentifierToken)) return false;
        if (!super.equals(o)) return false;
        IdentifierToken nameToken = (IdentifierToken) o;
        return Objects.equals(name, nameToken.name);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), name);
    }
}
