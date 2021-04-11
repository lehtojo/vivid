public static class Types
{
	public const Type UNKNOWN = null;

	public static readonly Type UNIT = new("_", Modifier.PUBLIC);

	public static readonly Bool BOOL = new();
	public static readonly Link LINK = new();

	public static readonly Tiny TINY = new();
	public static readonly Small SMALL = new();
	public static readonly Normal NORMAL = new();
	public static readonly Large LARGE = new();

	public static readonly U8 U8 = new();
	public static readonly U16 U16 = new();
	public static readonly U32 U32 = new();
	public static readonly U64 U64 = new();

	public static readonly Decimal DECIMAL = new();

	public static readonly L16 L16 = new();
	public static readonly L32 L32 = new();
	public static readonly L64 L64 = new();

	public static bool IsPrimitive(Type type)
	{
		return (type is Number || type is Bool) && type is not Link;
	}

	public static void Inject(Context context)
	{
		context.Declare(UNIT);
		context.Declare(BOOL);
		context.Declare(LINK);
		context.Declare(TINY);
		context.Declare(SMALL);
		context.Declare(NORMAL);
		context.Declare(LARGE);
		context.Declare(U8);
		context.Declare(U16);
		context.Declare(U32);
		context.Declare(U64);
		context.Declare(DECIMAL);
		context.Declare(L16);
		context.Declare(L32);
		context.Declare(L64);

		context.DeclareTypeAlias("byte", U8);
		context.DeclareTypeAlias("l8", LINK);
		context.DeclareTypeAlias("i8", TINY);
		context.DeclareTypeAlias("i16", SMALL);
		context.DeclareTypeAlias("i32", NORMAL);
		context.DeclareTypeAlias("i64", LARGE);
	}
}
