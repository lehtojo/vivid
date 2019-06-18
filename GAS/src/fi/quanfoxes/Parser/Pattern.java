package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.TokenType;

import java.util.Arrays;
import java.util.List;

public class Pattern {
    private List<TokenType> pattern;

    public Pattern(TokenType... pattern) {
        this.pattern = Arrays.asList(pattern);
    }

    public List<TokenType> getPattern() {
        return pattern;
    }
}
