package fi.quanfoxes.lexer;

public class ComparisonOperator extends Operator {
    private ComparisonOperator counterpart;

    public ComparisonOperator(String identifier, int priority) {
        super(identifier, OperatorType.COMPARISON, priority);
    }

    public ComparisonOperator setCounterpart(ComparisonOperator counterpart) {
        this.counterpart = counterpart;
        return this;
    }

    public ComparisonOperator getCounterpart() {
        return counterpart;
    }
}