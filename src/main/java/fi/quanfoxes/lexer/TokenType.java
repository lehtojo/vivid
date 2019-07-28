package fi.quanfoxes.lexer;

public class TokenType
{
    public static final int CAST = 1;
    public static final int CONTENT = 2;
    public static final int FUNCTION = 4;
    public static final int KEYWORD = 8;
    public static final int IDENTIFIER = 16;
    public static final int NUMBER = 32;
    public static final int OPERATOR = 64;
    public static final int OPTIONAL = 128;
    public static final int DYNAMIC = 256;

    public static final int COUNT = 9;
}
