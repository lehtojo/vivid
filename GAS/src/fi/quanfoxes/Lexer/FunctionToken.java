package fi.quanfoxes.Lexer;

public class FunctionToken extends Token {
    private String name;
    private ContentToken parameters;

    public FunctionToken(Lexer.TokenArea area) {
        super(area.text, TokenType.FUNCTION);
    }

}
