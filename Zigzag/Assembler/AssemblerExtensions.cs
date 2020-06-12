using System;
using System.Collections.Generic;
public static class ListPopExtensionStructs
{
	public static T? Pop<T>(this List<T> source) where T : struct
	{
		if (source.Count == 0)
		{
			return null;
		}

		var element = source[0];
		source.RemoveAt(0);

		return element;
	}
}

public static class AssemblerExtensions
{
	public static T? Pop<T>(this List<T> source) where T : class
	{
		if (source.Count == 0)
		{
			return null;
		}

		var element = source[0];
		source.RemoveAt(0);

		return element;
	}

	public static Size GetSize(this Type type)
	{
		return Size.FromBytes(type.ReferenceSize);
	}

	public static string ToString(this Size size)
	{
		return (Enum.GetName(typeof(Size), size) ?? throw new ApplicationException("Couldn't get identifier for instruction parameter size")).ToLower();
	}

	public static bool IsVisible(this Size size)
	{
		return size != Size.NONE;
	}

	public static bool IsDecimal(this Format format)
	{
		return format == Format.DECIMAL;
	}

	public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
	{
		foreach (var item in collection)
		{
			action(item);
		}
	}
}