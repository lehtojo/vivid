public static class Primitives
{
	public const string UNIT = "_";
	public const string LINK = "link";
	public const string BOOL = "bool";
	public const string DECIMAL = "decimal";
	public const string LARGE = "large";
	public const string NORMAL = "normal";
	public const string SMALL = "small";
	public const string TINY = "tiny";
	public const string I64 = "i64";
	public const string I32 = "i32";
	public const string I16 = "i16";
	public const string I8 = "i8";
	public const string U64 = "u64";
	public const string U32 = "u32";
	public const string U16 = "u16";
	public const string U8 = "u8";
	public const string L64 = "l64";
	public const string L32 = "l32";
	public const string L16 = "l16";
	public const string L8 = "l8";
	public const string BYTE = "byte";

	public const string LINK_IDENTIFIER = "Ph";
	public const string BOOL_IDENTIFIER = "b";
	public const string DECIMAL_IDENTIFIER = "d";
	public const string LARGE_IDENTIFIER = "x";
	public const string NORMAL_IDENTIFIER = "i";
	public const string SMALL_IDENTIFIER = "s";
	public const string TINY_IDENTIFIER = "c";
	public const string I64_IDENTIFIER = LARGE_IDENTIFIER;
	public const string I32_IDENTIFIER = NORMAL_IDENTIFIER;
	public const string I16_IDENTIFIER = SMALL_IDENTIFIER;
	public const string I8_IDENTIFIER = TINY_IDENTIFIER;
	public const string U64_IDENTIFIER = "y";
	public const string U32_IDENTIFIER = "j";
	public const string U16_IDENTIFIER = "t";
	public const string U8_IDENTIFIER = "h";
	public const string BYTE_IDENTIFIER = U8_IDENTIFIER;

	/// <summary>
	/// Creates a primitive number type with the specified name and format
	/// </summary>
	public static Number CreateNumber(string primitive, Format format)
	{
		var number = new Number(format, Size.FromFormat(format).Bits, format.IsUnsigned(), primitive);

		number.Identifier = primitive switch
		{
			LINK => LINK_IDENTIFIER,
			BOOL => BOOL_IDENTIFIER,
			BYTE => BYTE_IDENTIFIER,
			DECIMAL => DECIMAL_IDENTIFIER,
			LARGE => LARGE_IDENTIFIER,
			NORMAL => NORMAL_IDENTIFIER,
			SMALL => SMALL_IDENTIFIER,
			TINY => TINY_IDENTIFIER,
			I64 => I64_IDENTIFIER,
			I32 => I32_IDENTIFIER,
			I16 => I16_IDENTIFIER,
			I8 => I8_IDENTIFIER,
			U64 => U64_IDENTIFIER,
			U32 => U32_IDENTIFIER,
			U16 => U16_IDENTIFIER,
			U8 => U8_IDENTIFIER,
			_ => number.Name
		};

		return number;
	}

	/// <summary>
	/// Creates a primitive unit type
	/// </summary>
	public static Type CreateUnit()
	{
		return new Type(UNIT, Modifier.PRIMITIVE);
	}

	/// <summary>
	/// Creates a primitive boolean type
	/// </summary>
	public static Type CreateBool()
	{
		return CreateNumber(BOOL, Format.UINT8);
	}

	/// <summary>
	/// Returns whether the specified type is primitive type and whether its name matches the specified name
	/// </summary>
	public static bool IsPrimitive(Type? type, string expected)
	{
		return type != null && type.IsPrimitive && type.Name == expected;
	}

	/// <summary>
	/// Returns whether the specified type is primitive type
	/// </summary>
	public static bool IsPrimitive(Type type)
	{
		return type.IsPrimitive && type.Name != LINK;
	}

	public static void Inject(Context context)
	{
		context.Declare(CreateUnit());
		context.Declare(CreateBool());
		context.Declare(new Link());
		context.Declare(CreateNumber(TINY, Format.INT8));
		context.Declare(CreateNumber(SMALL, Format.INT16));
		context.Declare(CreateNumber(NORMAL, Format.INT32));
		context.Declare(CreateNumber(LARGE, Format.INT64));
		context.Declare(CreateNumber(I8, Format.INT8));
		context.Declare(CreateNumber(I16, Format.INT16));
		context.Declare(CreateNumber(I32, Format.INT32));
		context.Declare(CreateNumber(I64, Format.INT64));
		context.Declare(CreateNumber(U8, Format.UINT8));
		context.Declare(CreateNumber(U16, Format.UINT16));
		context.Declare(CreateNumber(U32, Format.UINT32));
		context.Declare(CreateNumber(U64, Format.UINT64));
		context.Declare(CreateNumber(DECIMAL, Format.DECIMAL));
		context.Declare(CreateNumber(BYTE, Format.UINT8));
		context.Declare(Link.GetVariant(CreateNumber(U8, Format.UINT8), L8));
		context.Declare(Link.GetVariant(CreateNumber(U16, Format.UINT16), L16));
		context.Declare(Link.GetVariant(CreateNumber(U32, Format.UINT32), L32));
		context.Declare(Link.GetVariant(CreateNumber(U64, Format.UINT64), L64));
	}
}
