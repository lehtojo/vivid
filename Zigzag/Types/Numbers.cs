using System.Collections.Generic;

public static class Numbers
{
	private static Dictionary<Format, Number> Values { get; } = new Dictionary<Format, Number>();

	public static Number Get(Format type)
	{
		return Values[type];
	}

	private static void Add(Number number)
	{
		Values.Add(number.Type, number);
	}

	static Numbers()
	{
		Add(Types.TINY);
		Add(Types.SMALL);
		Add(Types.NORMAL);
		Add(Types.LARGE);
		
		Add(Types.U8);
		Add(Types.U16);
		Add(Types.U32);
		Add(Types.U64);

		Add(Types.DECIMAL);
	}
}