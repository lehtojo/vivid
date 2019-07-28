package fi.quanfoxes.lexer;

public class ActionOperator extends Operator {
    private Operator operator;

    public ActionOperator(String identifier, Operator operator, int priority) {
        super(identifier, OperatorType.ACTION, priority);
        this.operator = operator;
    }

    /**
     * Returns the operation associated with the action
     * @return Operation associated with the action
     */
    public Operator getOperator() {
        return operator;
    }
}