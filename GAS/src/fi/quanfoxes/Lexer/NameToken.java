package fi.quanfoxes.Lexer;

import java.util.Objects;

public class NameToken extends Token {
    private String name;

    public NameToken(Lexer.TokenArea area) {
        this(area.text);
    }

    public NameToken(String name) {
        super(TokenType.NAME);
        this.name = name;
    }

    public String getName () {
        return name;
    }

    @Override
    public String getText() {
        return name;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof NameToken)) return false;
        if (!super.equals(o)) return false;
        NameToken nameToken = (NameToken) o;
        return Objects.equals(name, nameToken.name);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), name);
    }
}
