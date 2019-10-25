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
        Add(Types.BYTE);
        Add(Types.LONG);
        Add(Types.NORMAL);
        Add(Types.SHORT);
        Add(Types.TINY);
        Add(Types.UINT);
        Add(Types.ULONG);
        Add(Types.USHORT);
    }
}