public static class Extensions
{
	public static bool IsUnsigned(this Format type)
	{
		return ((short)type & 1) == 1;
	}
}