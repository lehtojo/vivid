public class Flag
{
	public static bool Has(int mask, int flag)
	{
		return (mask & flag) == flag;
	}
}
