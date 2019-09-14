package fi.quanfoxes.lexer;

import java.util.HashMap;
import java.util.Map;

public class Operators {
    
    public static final ClassicOperator EXTENDER = new ClassicOperator(":", 19, false);

    public static final ClassicOperator POWER = new ClassicOperator("^", 15);

    public static final ClassicOperator MULTIPLY = new ClassicOperator("*", 12);
    public static final ClassicOperator DIVIDE = new ClassicOperator("/", 12);
    public static final ClassicOperator MODULUS = new ClassicOperator("%", 12);

    public static final ClassicOperator ADD = new ClassicOperator("+", 11);
    public static final ClassicOperator SUBTRACT = new ClassicOperator("-", 11);

    public static final ClassicOperator SHIFT_LEFT = new ClassicOperator("<<", 10);
    public static final ClassicOperator SHIFT_RIGHT = new ClassicOperator(">>", 10);

    public static final ComparisonOperator GREATER_THAN = new ComparisonOperator(">", 9);
    public static final ComparisonOperator GREATER_OR_EQUAL = new ComparisonOperator(">=", 9);
    public static final ComparisonOperator LESS_THAN = new ComparisonOperator("<", 9);
    public static final ComparisonOperator LESS_OR_EQUAL = new ComparisonOperator("<=", 9);

    public static final ComparisonOperator EQUALS = new ComparisonOperator("==", 8);
    public static final ComparisonOperator NOT_EQUALS = new ComparisonOperator("!=", 8);

    public static final ClassicOperator BITWISE_AND = new ClassicOperator("and", 7);
    public static final ClassicOperator BITWISE_XOR = new ClassicOperator("xor", 6);
    public static final ClassicOperator BITWISE_OR = new ClassicOperator("or", 5);
    public static final LogicOperator AND = new LogicOperator("&", 4);
    public static final LogicOperator OR = new LogicOperator("|", 3);

    public static final ActionOperator ASSIGN = new ActionOperator("=", null, 1);
    public static final ActionOperator ASSIGN_POWER = new ActionOperator("^=", Operators.POWER, 1);
    public static final ActionOperator ASSIGN_ADD = new ActionOperator("+=", Operators.ADD, 1);
    public static final ActionOperator ASSIGN_SUBTRACT = new ActionOperator("-=", Operators.SUBTRACT, 1);
    public static final ActionOperator ASSIGN_MULTIPLY = new ActionOperator("*=", Operators.MULTIPLY, 1);
    public static final ActionOperator ASSIGN_DIVIDE = new ActionOperator("/=", Operators.DIVIDE, 1);

    public static final IndependentOperator COMMA = new IndependentOperator(",");
    public static final IndependentOperator DOT = new IndependentOperator(".");

    public static final IndependentOperator INCREMENT = new IndependentOperator("++");
    public static final IndependentOperator DECREMENT = new IndependentOperator("--");

    public static final IndependentOperator CAST = new IndependentOperator("->");

    public static final IndependentOperator END = new IndependentOperator("\n");

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
        add(GREATER_THAN.setCounterpart(LESS_OR_EQUAL));
        add(GREATER_OR_EQUAL.setCounterpart(LESS_THAN));
        add(LESS_THAN.setCounterpart(GREATER_OR_EQUAL));
        add(LESS_OR_EQUAL.setCounterpart(GREATER_THAN));
        add(EQUALS.setCounterpart(NOT_EQUALS));
        add(NOT_EQUALS.setCounterpart(EQUALS));
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