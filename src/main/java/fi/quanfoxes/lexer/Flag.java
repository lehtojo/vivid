package fi.quanfoxes.lexer;

public class Flag {
    public static boolean has(final int mask, final int flag) {
        return (mask & flag) == flag;
    }
}
