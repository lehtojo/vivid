public class Status
{
	public static readonly Status OK = new Status("OK", false);

	public string Description { get; private set; }

	public bool IsProblematic { get; private set; }

	public Status(string description, bool problematic)
	{
		Description = description;
		IsProblematic = problematic;
	}

	public static Status Error(string format, params object[] args)
	{
		return new Status(string.Format(format, args), true);
	}

	public static Status Error(string description)
	{
		return new Status(description, true);
	}
}