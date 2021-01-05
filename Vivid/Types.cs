public static class Types
{
	public const Type UNKNOWN = null;

	public static readonly Type UNIT = new Type("_", Modifier.PUBLIC);

	public static readonly Bool BOOL = new Bool();
	public static readonly Link LINK = new Link();

	public static readonly Tiny TINY = new Tiny();
	public static readonly Small SMALL = new Small();
	public static readonly Normal NORMAL = new Normal();
	public static readonly Large LARGE = new Large();

	public static readonly U8 U8 = new U8();
	public static readonly U16 U16 = new U16();
	public static readonly U32 U32 = new U32();
	public static readonly U64 U64 = new U64();

	public static readonly Decimal DECIMAL = new Decimal();

	public static readonly L16 L16 = new L16();
	public static readonly L32 L32 = new L32();
	public static readonly L64 L64 = new L64();

	public static bool IsPrimitive(Type type)
	{
		return (type is Number || type is Bool) && type != Types.LINK;
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
