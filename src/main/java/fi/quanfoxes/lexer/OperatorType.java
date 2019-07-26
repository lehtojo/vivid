package fi.quanfoxes.lexer;

import java.util.HashMap;

public enum OperatorType{
    // Priority -1: Dot operator is processed independently
    DOT(".", -1, OperatorCategory.OTHER),

    // Priority -1: Increment and decrement operators are processed independently
    INCREMENT("++", -1, OperatorCategory.OTHER),
    DECREMENT("--", -1, OperatorCategory.OTHER),

    POWER("^", 15, OperatorCategory.ARITHMETIC),

    MULTIPLY("*", 12, OperatorCategory.ARITHMETIC),
    DIVIDE("/", 12, OperatorCategory.ARITHMETIC),
    MODULUS("%", 12, OperatorCategory.ARITHMETIC),

    ADD("+", 11, OperatorCategory.ARITHMETIC),
    SUBTRACT("-", 11, OperatorCategory.ARITHMETIC),

    SHIFT_LEFT("<<", 10, OperatorCategory.ARITHMETIC),
    SHIFT_RIGHT(">>", 10, OperatorCategory.ARITHMETIC),

    GREATER_THAN(">", 9, OperatorCategory.COMPARISON),
    GREATER_OR_EQUAL(">=", 9, OperatorCategory.COMPARISON),
    LESS_THAN("<", 9, OperatorCategory.COMPARISON),
    LESS_OR_EQUAL("<=", 9, OperatorCategory.COMPARISON),

    EQUALS("==", 8, OperatorCategory.COMPARISON),
    NOT_EQUALS("!=", 8, OperatorCategory.COMPARISON),

    BITWISE_AND("and", 7, OperatorCategory.ARITHMETIC),
    BITWISE_XOR("xor", 6, OperatorCategory.ARITHMETIC),
    BITWISE_OR("or", 5, OperatorCategory.ARITHMETIC),
    AND("&", 4, OperatorCategory.COMPARISON),
    OR("|", 3, OperatorCategory.COMPARISON),

    ASSIGN("=", 1, OperatorCategory.ACTION),
    ASSIGN_POWER("^=", 1, OperatorCategory.ACTION),
    ASSIGN_ADD("+=", 1, OperatorCategory.ACTION),
    ASSIGN_SUBTRACT("-=", 1, OperatorCategory.ACTION),
    ASSIGN_MULTIPLY("*=", 1, OperatorCategory.ACTION),
    ASSIGN_DIVIDE("/=", 1, OperatorCategory.ACTION),
    ASSIGN_OR("|=", 1, OperatorCategory.ACTION),

    // Priority -1: Comma operator is processed independently
    COMMA(",", -1, OperatorCategory.OTHER);

    private String identifier;
    private int priority;
    private OperatorCategory category;

    private static HashMap<String, OperatorType> map = new HashMap<>();

    private OperatorType(String text, int priority, OperatorCategory category) {
        this.identifier = text;
        this.priority = priority;
        this.category = category;
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

    public OperatorCategory getCategory() {
        return category;
    }
}
