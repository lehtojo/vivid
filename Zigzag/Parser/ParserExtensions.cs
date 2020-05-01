using System.Collections.Generic;

public static class ParserExtensions
{
    public static IList<T> Sublist<T>(this List<T> list, int start, int end)
	{
		return new Sublist<T>(list, start, end);
	}
}