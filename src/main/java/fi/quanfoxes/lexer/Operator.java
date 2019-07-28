package fi.quanfoxes.lexer;

public class Operator {
    private String identifier;
    private OperatorType type;
    private int priority;

    /**
     * Creates an operator with identifier, category and priority
     * @param identifier Text to identify the operator
     * @param type Type of the operator
     * @param priority Priority of the operator when parsing
     */
    public Operator(String identifier, OperatorType type, int priority) {
        this.identifier = identifier;
        this.type = type;
        this.priority = priority;
    }

    /**
     * Returns the text used to identify the operator
     * @return Text used to identify the operator
     */
    public String getIdentifier() {
        return identifier;
    }

    /**
     * Returns the type of the operator
     * @return Type of the operator
     */
    public OperatorType getType() {
        return type;
    }

    /**
     * Returns the priority of the operator, which is used in parsing process
     * @return Priority of the operator
     */
    public int getPriority() {
        return priority;
    }
}