using System;
using System.Collections.Generic;
using System.Linq;

public static class AssemblerExtensions
{
	public static IDictionary<TKey, List<TValue>> Merge<TKey, TValue>(this IDictionary<TKey, List<TValue>> a, IDictionary<TKey, List<TValue>> b)
	{
		foreach (var i in b)
		{
			if (a.ContainsKey(i.Key))
			{
				a[i.Key].AddRange(i.Value);
				continue;
			}

			a.Add(i);
		}

		return a;
	}

	public static IEnumerable<T> Except<T>(this IEnumerable<T> a, T b)
	{
		return a.Where(i => !ReferenceEquals(i, b));
	}

	public static Format GetRegisterFormat(this Variable variable)
	{
		var type = variable.Type!;
		if (type.Format.IsDecimal()) return Format.DECIMAL;
		if (type.Format.IsUnsigned()) return Format.UINT64;
		return Format.INT64;
	}

	public static Format GetRegisterFormat(this Type type)
	{
		if (type.Format.IsDecimal()) return Format.DECIMAL;
		if (type.Format.IsUnsigned()) return Format.UINT64;
		return Format.INT64;
	}

	public static T? Pop<T>(this List<T> source) where T : class
	{
		if (source.Count == 0) return null;

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