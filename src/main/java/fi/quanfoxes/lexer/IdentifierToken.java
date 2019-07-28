package fi.quanfoxes.lexer;

import java.util.Objects;

public class IdentifierToken extends Token {
    private String value;

    /**
     * Creates an identifier token that holds the given text value
     * @param value Text value to hold
     */
    public IdentifierToken(String value) {
        super(TokenType.IDENTIFIER);
        this.value = value;
    }

    /**
     * Returns the value that the identifier holds
     * @return Identifier's value
     */
    public String getValue() {
        return value;
    }

    @Override
    public String getText() {
        return value;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof IdentifierToken)) return false;
        if (!super.equals(o)) return false;
        IdentifierToken id = (IdentifierToken) o;
        return Objects.equals(value, id.value);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), value);
    }
}
