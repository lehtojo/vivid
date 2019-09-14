package fi.quanfoxes.lexer;

public class StringToken extends Token {
    private String text;

    public StringToken(String text) {
        super(TokenType.STRING);
        this.text = text.substring(1, text.length() - 1);
    }

    public String getText() {
        return text;
    }
}