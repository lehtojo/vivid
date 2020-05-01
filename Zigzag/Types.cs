using System;

public class Types
{
	public const Type UNKNOWN = null;

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

	public static void Inject(Context context)
	{
		context.Declare(BOOL);
		context.Declare(LINK);
		context.Declare(TINY);
		context.Declare(SMALL);
		context.Declare(NORMAL);
		context.Declare(LARGE);
		context.Declare(U32);
		context.Declare(U64);
		context.Declare(U16);
		context.Declare(U8);
		context.Declare(DECIMAL);

		switch (Parser.Size.Bytes)
		{
			case 1: { context.DeclareTypeAlias("num", TINY); break; }
			case 2: { context.DeclareTypeAlias("num", SMALL); break; }
			case 4: { context.DeclareTypeAlias("num", NORMAL); break; }
			case 8: { context.DeclareTypeAlias("num", LARGE); break; }

			default: throw new ApplicationException("Couldn't declare alias type 'num'");
		}

		context.DeclareTypeAlias("byte", U8);

		context.DeclareTypeAlias("i8", TINY);
		context.DeclareTypeAlias("i16", SMALL);
		context.DeclareTypeAlias("i32", NORMAL);
		context.DeclareTypeAlias("i64", LARGE);
	}
}
