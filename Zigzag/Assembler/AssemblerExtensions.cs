using System;

public static class AssemblerExtensions
{
    public static Size GetInstructionParameterSize(this Variable variable)
    {
        return Size.FromBytes(variable.Type?.ReferenceSize ?? throw new ApplicationException("Couldn't get variable type"));
    }

    public static Size GetInstructionParameterSize(this Type type)
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