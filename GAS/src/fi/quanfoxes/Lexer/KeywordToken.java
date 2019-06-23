package fi.quanfoxes.Lexer;

import fi.quanfoxes.Keyword;
import fi.quanfoxes.KeywordDatabase;

import java.util.Objects;

public class KeywordToken extends Token {
    private Keyword keyword;

    public KeywordToken(final String text) {
        super(TokenType.KEYWORD);
        keyword = KeywordDatabase.get(text);
    }

    public KeywordToken(Keyword keyword) {
        super(TokenType.KEYWORD);
        this.keyword = keyword;
    }

    public Keyword getKeyword() {
        return keyword;
    }

    @Override
    public String getText() {
        return keyword.getName();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof KeywordToken)) return false;
        if (!super.equals(o)) return false;
        KeywordToken that = (KeywordToken) o;
        return Objects.equals(keyword, that.keyword);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), keyword);
    }
}
