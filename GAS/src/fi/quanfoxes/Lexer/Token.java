package fi.quanfoxes.Lexer;

import java.util.List;
import java.util.Objects;

public class Token {
    private String text;
    private TokenType type;

    public Token(String text, TokenType type) {
        this.text = text;
        this.type = type;
    }

    public String getText() {
        return text;
    }

    public TokenType getType() {
        return type;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof Token)) return false;
        Token token = (Token) o;
        return Objects.equals(text, token.text) &&
                type == token.type;
    }

    @Override
    public int hashCode() {
        return Objects.hash(text, type);
    }
}
