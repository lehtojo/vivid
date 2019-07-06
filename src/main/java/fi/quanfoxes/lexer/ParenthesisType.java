package fi.quanfoxes.lexer;

import java.util.HashMap;

// ([{
public enum ParenthesisType {
    PARENTHESIS('('),
    BRACKETS('['),
    CURLY_BRACKETS('{');

    private static HashMap<Character, ParenthesisType> map = new HashMap<>();

    static {
        for (ParenthesisType operator : ParenthesisType.values()) {
            map.put(operator.opening, operator);
        }
    }

    private char opening;

    private ParenthesisType(char opening) {
        this.opening = opening;
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
}