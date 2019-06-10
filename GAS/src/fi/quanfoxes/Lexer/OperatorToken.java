package fi.quanfoxes.Lexer;

public class OperatorToken extends Token {

    private final String[] OPERATORS =
    {
        "+", "-", "*", "/", "%", ">", ">=", "<", "<=", "==", "!=", "&", "|", "^", "!",
        "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^="
    };


    private OperatorType operator;

    public OperatorToken(Lexer.TokenArea area) {
        super(area.text, TokenType.OPERATOR);

        for (int i = 0; i < OPERATORS.length; i++) {
            if (OPERATORS[i].equals(area.text)) {
                operator = OperatorType.values()[i];
                break;
            }
        }
    }

    public OperatorType getOperator ()  {
        return operator;
    }
}
