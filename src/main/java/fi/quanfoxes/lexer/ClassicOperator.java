package fi.quanfoxes.lexer;

public class ClassicOperator extends Operator {
    private boolean shared;

    public ClassicOperator(String identifier, int priority) {
        this(identifier, priority, true);
    }

    public ClassicOperator(String identifier, int priority, boolean shared) {
        super(identifier, OperatorType.CLASSIC, priority);
        this.shared = shared;
    }

    public boolean isSharedContext() {
        return shared;
    }
}