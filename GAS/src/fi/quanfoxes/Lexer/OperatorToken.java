package fi.quanfoxes.Lexer;

import java.util.Objects;

public class OperatorToken extends Token {

    private OperatorType operator;

    public OperatorToken(String text) {
        super(TokenType.OPERATOR);
        operator = OperatorType.get(text);
    }

    public OperatorToken(OperatorType type) {
        super(TokenType.OPERATOR);
        operator = type;
    }

    public OperatorType getOperator ()  {
        return operator;
    }

    @Override
    public String getText() {
        return operator.getIdentifier();
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
