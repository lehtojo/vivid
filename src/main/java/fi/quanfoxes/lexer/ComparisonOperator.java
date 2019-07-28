package fi.quanfoxes.lexer;

public class ComparisonOperator extends Operator {
    public ComparisonOperator(String identifier, int priority) {
        super(identifier, OperatorType.COMPARISON, priority);
    }
}