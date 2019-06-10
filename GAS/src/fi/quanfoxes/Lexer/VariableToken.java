package fi.quanfoxes.Lexer;

public class VariableToken extends Token {

    public VariableToken (Lexer.TokenArea area) {
        super(area.text, TokenType.VARIABLE);
    }
    public VariableToken (String name)
    {
        super(name, TokenType.VARIABLE);
    }

    public String getName () {
        return getText();
    }
}
