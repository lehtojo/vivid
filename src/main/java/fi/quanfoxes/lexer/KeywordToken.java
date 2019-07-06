package fi.quanfoxes.lexer;

import fi.quanfoxes.Keyword;
import fi.quanfoxes.Keywords;

import java.util.Objects;

public class KeywordToken extends Token {
    private Keyword keyword;

    public KeywordToken(final String text) {
        super(TokenType.KEYWORD);
        keyword = Keywords.get(text);
    }

    public KeywordToken(final Keyword keyword) {
        super(TokenType.KEYWORD);
        this.keyword = keyword;
    }

    public Keyword getKeyword() {
        return keyword;
    }

    @Override
    public String getText() {
        return keyword.getIdentifier();
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
