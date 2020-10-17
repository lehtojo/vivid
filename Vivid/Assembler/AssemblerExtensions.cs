using System;
using System.Diagnostics.CodeAnalysis;
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

	public static Size GetSize(this Type type)
	{
		return Size.FromBytes(type.ReferenceSize);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1308", Justification = "Assembly style required lower case")]
	public static string ToString(this Size size)
	{
		return (Enum.GetName(typeof(Size), size) ?? throw new ApplicationException("Could not get identifier for instruction parameter size")).ToLowerInvariant();
	}

	public static bool IsVisible(this Size size)
	{
		return size != Size.NONE;
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
}