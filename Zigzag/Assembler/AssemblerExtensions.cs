using System;
using System.Diagnostics.CodeAnalysis;

public static class AssemblerExtensions
{
	public static Size GetSize(this Type type)
	{
		return Size.FromBytes(type.ReferenceSize);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1308", Justification = "Assembly style required lower case")]
	public static string ToString(this Size size)
	{
		return (Enum.GetName(typeof(Size), size) ?? throw new ApplicationException("Couldn't get identifier for instruction parameter size")).ToLowerInvariant();
	}

	public static bool IsVisible(this Size size)
	{
		return size != Size.NONE;
	}
}