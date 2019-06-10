package fi.quanfoxes.Lexer;

import java.util.ArrayList;
import java.util.List;

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

    public List<Token> getTokens() {
        return tokens;
    }
}
