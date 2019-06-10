package fi.quanfoxes.Lexer;

import fi.quanfoxes.Keyword;
import fi.quanfoxes.KeywordDatabase;

import java.util.Objects;

public class KeywordToken extends Token {
    private Keyword keyword;

    public KeywordToken(Lexer.TokenArea area) {
        super(area.text, TokenType.KEYWORD);
        keyword = KeywordDatabase.get(area.text);
    }

    public Keyword getKeyword() {
        return keyword;
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
