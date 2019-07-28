package fi.quanfoxes.lexer;

import java.util.Objects;

public class OperatorToken extends Token {

    private Operator operator;

    public OperatorToken(String identifier) {
        super(TokenType.OPERATOR);
        this.operator = Operators.get(identifier);
    }

    public OperatorToken(Operator operator) {
        super(TokenType.OPERATOR);
        this.operator = operator;
    }

    public Operator getOperator ()  {
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
