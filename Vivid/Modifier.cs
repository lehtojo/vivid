public static class Modifier
{
	/// NOTE: Do not change the order, since the importer, for example, is dependent on it
	public const int PUBLIC = 1;
	public const int PRIVATE = 2;
	public const int PROTECTED = 4;
	public const int STATIC = 8;
	public const int EXTERNAL = 16;
	public const int READONLY = 32;
	public const int GLOBAL = 64;
	public const int CONSTANT = 128;
	public const int TEMPLATE_TYPE = 256;
	public const int TEMPLATE_FUNCTION = 512;
	public const int OUTLINE = 1024;
	public const int INLINE = 2048;

	public const int DEFAULT = PUBLIC;
	
	private static int GetExcluder(int modifier)
	{
		return modifier switch
		{
			PUBLIC => PRIVATE | PROTECTED,
			PRIVATE => PUBLIC | PROTECTED,
			PROTECTED => PUBLIC | PRIVATE,
			_ => 0
		};
	}

	public static int Combine(int modifiers, int modifier)
	{
		return (modifiers | modifier) & ~GetExcluder(modifier);
	}
}
