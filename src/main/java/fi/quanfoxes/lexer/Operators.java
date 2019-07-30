package fi.quanfoxes.lexer;

import java.util.HashMap;
import java.util.Map;

public class Operators {
    
    public static final Operator EXTENDER = new ClassicOperator(":", 19, false);

    public static final Operator POWER = new ClassicOperator("^", 15);

    public static final Operator MULTIPLY = new ClassicOperator("*", 12);
    public static final Operator DIVIDE = new ClassicOperator("/", 12);
    public static final Operator MODULUS = new ClassicOperator("%", 12);

    public static final Operator ADD = new ClassicOperator("+", 11);
    public static final Operator SUBTRACT = new ClassicOperator("-", 11);

    public static final Operator SHIFT_LEFT = new ClassicOperator("<<", 10);
    public static final Operator SHIFT_RIGHT = new ClassicOperator(">>", 10);

    public static final Operator GREATER_THAN = new ComparisonOperator(">", 9);
    public static final Operator GREATER_OR_EQUAL = new ComparisonOperator(">=", 9);
    public static final Operator LESS_THAN = new ComparisonOperator("<", 9);
    public static final Operator LESS_OR_EQUAL = new ComparisonOperator("<=", 9);

    public static final Operator EQUALS = new ComparisonOperator("==", 8);
    public static final Operator NOT_EQUALS = new ComparisonOperator("!=", 8);

    public static final Operator BITWISE_AND = new ClassicOperator("and", 7);
    public static final Operator BITWISE_XOR = new ClassicOperator("xor", 6);
    public static final Operator BITWISE_OR = new ClassicOperator("or", 5);
    public static final Operator AND = new ComparisonOperator("&", 4);
    public static final Operator OR = new ComparisonOperator("|", 3);

    public static final Operator ASSIGN = new ActionOperator("=", null, 1);
    public static final Operator ASSIGN_POWER = new ActionOperator("^=", Operators.POWER, 1);
    public static final Operator ASSIGN_ADD = new ActionOperator("+=", Operators.ADD, 1);
    public static final Operator ASSIGN_SUBTRACT = new ActionOperator("-=", Operators.SUBTRACT, 1);
    public static final Operator ASSIGN_MULTIPLY = new ActionOperator("*=", Operators.MULTIPLY, 1);
    public static final Operator ASSIGN_DIVIDE = new ActionOperator("/=", Operators.DIVIDE, 1);

    public static final Operator COMMA = new IndependentOperator(",");
    public static final Operator DOT = new IndependentOperator(".");

    public static final Operator INCREMENT = new IndependentOperator("++");
    public static final Operator DECREMENT = new IndependentOperator("--");

    public static final Operator CAST = new IndependentOperator("->");

    public static final Operator END = new IndependentOperator("\n");

    private static Map<String, Operator> map = new HashMap<>();

    private static void add(Operator operator) {
        map.put(operator.getIdentifier(), operator);
    }

    static {
        add(POWER);
        add(MULTIPLY);
        add(DIVIDE);
        add(MODULUS);
        add(ADD);
        add(SUBTRACT);
        add(SHIFT_LEFT);
        add(SHIFT_RIGHT);
        add(GREATER_THAN);
        add(GREATER_OR_EQUAL);
        add(LESS_THAN);
        add(LESS_OR_EQUAL);
        add(EQUALS);
        add(NOT_EQUALS);
        add(BITWISE_AND);
        add(BITWISE_XOR);
        add(BITWISE_OR);
        add(AND);
        add(OR);
        add(ASSIGN);
        add(ASSIGN_POWER);
        add(ASSIGN_ADD);
        add(ASSIGN_SUBTRACT);
        add(ASSIGN_MULTIPLY);
        add(ASSIGN_DIVIDE);
        add(COMMA);
        add(DOT);
        add(INCREMENT);
        add(DECREMENT);
        add(CAST);
        add(EXTENDER);
        add(END);
    }

    public static Operator get(String text) {
        return map.get(text);
    }

    public static boolean exists(String identifier) {
        return map.containsKey(identifier);
    }
}