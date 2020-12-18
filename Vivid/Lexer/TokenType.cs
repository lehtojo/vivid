public static class TokenType
{
	public const int CONTENT = 1;
	public const int FUNCTION = 2;
	public const int KEYWORD = 4;
	public const int IDENTIFIER = 8;
	public const int NUMBER = 16;
	public const int OPERATOR = 32;
	public const int OPTIONAL = 64;
	public const int DYNAMIC = 128;
	public const int END = 256;
	public const int STRING = 512;

	public const int OBJECT = CONTENT | FUNCTION | IDENTIFIER | NUMBER | DYNAMIC | STRING;
	public const int NONE = 0;
	public const int ANY = -1;

	public const int COUNT = 10;
}
