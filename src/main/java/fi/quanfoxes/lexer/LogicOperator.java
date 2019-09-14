package fi.quanfoxes.lexer;

public class LogicOperator extends Operator {
    public LogicOperator(String identifier, int priority) {
        super(identifier, OperatorType.LOGIC, priority);
    }
}