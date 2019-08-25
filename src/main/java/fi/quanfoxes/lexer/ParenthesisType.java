package fi.quanfoxes.lexer;

import java.util.HashMap;
import java.util.Map;

public enum ParenthesisType {
    PARENTHESIS('(', ')'),
    BRACKETS('[', ']'),
    CURLY_BRACKETS('{', '}');

    private static Map<Character, ParenthesisType> map = new HashMap<>();

    static {
        for (ParenthesisType operator : ParenthesisType.values()) {
            map.put(operator.opening, operator);
        }
    }

    private char opening;
    private char closing;

    private ParenthesisType(char opening, char closing) {
        this.opening = opening;
        this.closing = closing;
    }

    public static ParenthesisType get(Character opening) {
        return map.get(opening);
    }

    public static boolean has(Character opening) {
        return map.containsKey(opening);
    }

    public Character getOpening() {
        return opening;
    }

    public Character getClosing() {
        return closing;
    }
}