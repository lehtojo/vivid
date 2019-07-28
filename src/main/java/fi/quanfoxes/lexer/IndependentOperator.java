package fi.quanfoxes.lexer;

public class IndependentOperator extends Operator {
    public IndependentOperator(String identifier) {
        super(identifier, OperatorType.INDEPENDENT, -1);
    }
}