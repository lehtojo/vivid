using System.Globalization;

public class Status
{
	public static readonly Status OK = new Status("OK", false);

	public string Description { get; }

	public bool IsProblematic { get; private set; }

	private Status(string description, bool problematic)
	{
		Description = description;
		IsProblematic = problematic;
	}

	public static Status Error(string format, params object[] arguments)
	{
		return new Status(string.Format(CultureInfo.InvariantCulture, format, arguments), true);
	}

	public static Status Error(string description)
	{
		return new Status(description, true);
	}
}