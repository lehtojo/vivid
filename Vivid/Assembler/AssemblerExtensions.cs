using System;
using System.Collections.Generic;

public static class AssemblerExtensions
{
	public static Format GetRegisterFormat(this Variable variable)
	{
		return variable.Type! == Types.DECIMAL ? Format.DECIMAL : Assembler.Format;
	}

	public static Format GetRegisterFormat(this Type type)
	{
		return type == Types.DECIMAL ? Format.DECIMAL : Assembler.Format;
	}

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

	public static string ToString(this Size size)
	{
		return (Enum.GetName(typeof(Size), size) ?? throw new ApplicationException("Could not get identifier for instruction parameter size")).ToLowerInvariant();
	}

	public static bool IsDecimal(this Format format)
	{
		return format == Format.DECIMAL;
	}

	public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
	{
		foreach (var item in collection)
		{
			action(item);
		}

		return collection;
	}
	
	public static bool IsUnsigned(this Format type)
	{
		return ((short)type & 1) == 1;
	}
}