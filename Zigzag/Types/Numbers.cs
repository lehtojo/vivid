using System.Collections.Generic;

public class Numbers
{
	private static Dictionary<NumberType, Number> numbers = new Dictionary<NumberType, Number>();

	public static Number Get(NumberType type)
	{
		return numbers[type];
	}

	private static void Add(Number number)
	{
		numbers.Add(number.Type, number);
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