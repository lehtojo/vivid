public static class Flag
{
	public static bool Has(long mask, long flag)
	{
		return (mask & flag) == flag;
	}

	public static bool Has(int mask, int flag)
	{
		return (mask & flag) == flag;
	}

	public static int Combine(int[] flags)
	{
		var result = 0;

		foreach (var flag in flags)
		{
			result |= flag;
		}

		return result;
	}
}
