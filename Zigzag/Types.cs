public class Types
{
	public const Type UNKNOWN = null;

	public static readonly Bool BOOL = new Bool();
	public static readonly Byte BYTE = new Byte();
	public static readonly Link LINK = new Link();
	public static readonly Long LONG = new Long();
	public static readonly Normal NORMAL = new Normal();
	public static readonly Decimal DECIMAL = new Decimal();
	public static readonly Short SHORT = new Short();
	public static readonly Tiny TINY = new Tiny();
	public static readonly Uint UINT = new Uint();
	public static readonly Ulong ULONG = new Ulong();
	public static readonly Ushort USHORT = new Ushort();

	public static void Inject(Context context)
	{
		context.Declare(BOOL);
		context.Declare(BYTE);
		context.Declare(LINK);
		context.Declare(LONG);
		context.Declare(NORMAL);
		context.Declare(DECIMAL);
		context.Declare(SHORT);
		context.Declare(TINY);
		context.Declare(UINT);
		context.Declare(ULONG);
		context.Declare(USHORT);
	}
}
