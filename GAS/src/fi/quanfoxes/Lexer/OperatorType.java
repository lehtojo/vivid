package fi.quanfoxes.Lexer;

import java.util.HashMap;
import java.util.Map;

public enum OperatorType{
    ADD("+"),
    SUBTRACT("-"),
    MULTIPLY("*"),
    DIVIDE("/"),
    MODULUS("%"),
    GREATER_THAN(">"),
    GREATER_OR_EQUAL(">="),
    LESS_THAN("<"),
    LESS_OR_EQUAL("<="),
    EQUALS("=="),
    NOT_EQUALS("!="),
    AND("&&"),
    OR("||"),
    BITWISE_AND("&"),
    BITWISE_OR("|"),
    BITWISE_XOR("^"),
    NOT("!"),
    ASSIGN_ADD("+="),
    ASSIGN_SUBTRACT("-="),
    ASSIGN_MULTIPLY("*="),
    ASSIGN_DIVIDE("/="),
    ASSIGN_MODULUS("%="),
    ASSIGN_AND("&="),
    ASSIGN_OR("|="),
    ASSIGN_XOR("^="),
    ASSIGN("="),
    COMMA(",");

    private String text;
    private static Map map = new HashMap<>();

    OperatorType(String text) {
        this.text = text;
    }

    static {
        for (OperatorType operator : OperatorType.values()) {
            map.put(operator.text, operator);
        }
    }

    public static OperatorType get(String text) {
        return (OperatorType) map.get(text);
    }

    public String getText() {
        return text;
    }
}
