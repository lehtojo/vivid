package fi.quanfoxes.Lexer;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Objects;

public class ContentToken extends Token {
    private List<Token> tokens = new ArrayList<>();

    public ContentToken(Lexer.TokenArea area) throws Exception {
        super(area.text, TokenType.CONTENT);

        String text = getText();

        // Make sure there is content
        if (text.length() > 2) {
            String content = text.substring(1, text.length() - 1);
            tokens = Lexer.getTokens(content);
        }
    }

    public ContentToken(String text) throws Exception {
        super(text, TokenType.CONTENT);

        // Make sure there is content
        if (text.length() > 2) {
            String content = text.substring(1, text.length() - 1);
            tokens = Lexer.getTokens(content);
        }
    }

    public ContentToken(String full, Token... tokens) throws Exception {
        super(full, TokenType.CONTENT);
        this.tokens = Arrays.asList(tokens);
    }

    public List<Token> getTokens() {
        return tokens;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof ContentToken)) return false;
        if (!super.equals(o)) return false;
        ContentToken that = (ContentToken) o;
        return Objects.equals(tokens, that.tokens);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), tokens);
    }
}
