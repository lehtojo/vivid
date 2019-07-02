package fi.quanfoxes.Lexer;

import java.util.HashMap;

public enum OperatorType{
    DOT(".", 20),

    // Priority -1: Increment and decrement operators are processed independently
    INCREMENT("++", -1),
    DECREMENT("--", -1),

    POWER("^", 15),

    MULTIPLY("*", 12),
    DIVIDE("/", 12),
    MODULUS("%", 12),

    ADD("+", 11),
    SUBTRACT("-", 11),

    SHIFT_LEFT("<<", 10),
    SHIFT_RIGHT(">>", 10),

    GREATER_THAN(">", 9),
    GREATER_OR_EQUAL(">=", 9),
    LESS_THAN("<", 9),
    LESS_OR_EQUAL("<=", 9),

    EQUALS("==", 8),
    NOT_EQUALS("!=", 8),

    BITWISE_AND("and", 7),
    BITWISE_XOR("xor", 6),
    BITWISE_OR("or", 5),
    AND("&&", 4),
    OR("||", 3),

    ASSIGN("=", 1),
    ASSIGN_POWER("^=", 1),
    ASSIGN_ADD("+=", 1),
    ASSIGN_SUBTRACT("-=", 1),
    ASSIGN_MULTIPLY("*=", 1),
    ASSIGN_DIVIDE("/=", 1),
    ASSIGN_OR("|=", 1),

    // Priority -1: Comma operator is processed independently
    COMMA(",", -1);

    private String identifier;
    private int priority;

    private static HashMap<String, OperatorType> map = new HashMap<>();

    OperatorType(String text, int priority) {
        this.identifier = text;
        this.priority = priority;
    }

    static {
        for (OperatorType operator : OperatorType.values()) {
            map.put(operator.identifier, operator);
        }
    }

    public static OperatorType get(String text) {
        return map.get(text);
    }

    public static boolean has(String text) {
        return map.containsKey(text);
    }

    public int getPriority() {
        return priority;
    }

    public String getIdentifier() {
        return identifier;
    }
}
