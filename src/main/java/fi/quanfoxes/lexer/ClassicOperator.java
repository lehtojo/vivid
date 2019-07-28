package fi.quanfoxes.lexer;

public class ClassicOperator extends Operator {
    public ClassicOperator(String identifier, int priority) {
        super(identifier, OperatorType.CLASSIC, priority);
    }
}