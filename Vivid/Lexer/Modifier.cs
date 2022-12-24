public static class Modifier
{
	// NOTE: Access levels must be sorted
	public const int PUBLIC = 1;
	public const int PROTECTED = 2;
	public const int PRIVATE = 4;
	public const int STATIC = 8;
	public const int IMPORTED = 16;
	public const int READABLE = 32;
	public const int EXPORTED = 64;
	public const int CONSTANT = 128;
	public const int TEMPLATE_TYPE = 256;
	public const int TEMPLATE_FUNCTION = 512;
	public const int OUTLINE = 1024;
	public const int INLINE = 2048;
	public const int PRIMITIVE = 4096;
	public const int PLAIN = 8192;
	public const int PACK = 16384 | PLAIN;
	public const int SELF = 524288;

	public const int DEFAULT = PUBLIC;
	public const int ACCESS_LEVEL_MASK = 0b111;

	private static int GetExcluder(int modifiers)
	{
		if (Flag.Has(modifiers, PUBLIC)) return PRIVATE | PROTECTED;
		if (Flag.Has(modifiers, PRIVATE)) return PUBLIC | PROTECTED;
		if (Flag.Has(modifiers, PROTECTED)) return PUBLIC | PRIVATE;

		return 0;
	}

	public static int Combine(int modifiers, int modifier)
	{
		return (modifiers | modifier) & ~GetExcluder(modifier);
	}
}
