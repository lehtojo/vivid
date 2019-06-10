package fi.quanfoxes.Lexer;

import java.util.Objects;

public class OperatorToken extends Token {

    private static final String[] OPERATORS =
    {
        "+", "-", "*", "/", "%", ">", ">=", "<", "<=", "==", "!=", "&", "|", "^", "!",
        "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "="
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

    public OperatorToken(OperatorType type) {
        super(OPERATORS[ type.ordinal()], TokenType.OPERATOR);
        operator = type;
    }

    public OperatorType getOperator ()  {
        return operator;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof OperatorToken)) return false;
        if (!super.equals(o)) return false;
        OperatorToken that = (OperatorToken) o;
        return operator == that.operator;
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), operator);
    }
}
