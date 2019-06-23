package fi.quanfoxes.Parser;

import fi.quanfoxes.Lexer.Token;
import fi.quanfoxes.Lexer.TokenType;

import java.util.Arrays;
import java.util.List;

public abstract class Pattern {
    private List<TokenType> path;

    public Pattern(TokenType... path) {
        this.path = Arrays.asList(path);
    }

    public abstract boolean passes(final List<Token> tokens);
    public abstract List<Instruction> build(final List<Token> tokens);

    public List<TokenType> getPath() {
        return path;
    }
}
