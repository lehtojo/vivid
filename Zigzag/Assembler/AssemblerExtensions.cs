using System;

public static class AssemblerExtensions
{
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
}