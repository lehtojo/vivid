using System.Globalization;

public class Status
{
	public static readonly Status OK = new("OK", false);

	public string Description { get; }

	public bool IsProblematic { get; private set; }

	private Status(string description, bool problematic)
	{
		Description = description;
		IsProblematic = problematic;
	}

	public static Status Error(string format, params object[] arguments)
	{
		return new Status(string.Format(format, arguments), true);
	}

	public static Status Error(string description)
	{
		return new Status(description, true);
	}

	public static Status Error(Position? position, string description)
	{
		var location = string.Empty;

		if (position != null)
		{
			var fullname = Errors.UNKNOWN_FILE;

			if (position.File != null)
			{
				fullname = position.File.Fullname;
			}

			location = $"{fullname}:{position.FriendlyLine}:{position.FriendlyCharacter}";
		}
		else
		{
			location = Errors.UNKNOWN_LOCATION;
		}

		return Status.Error($"{location}: {Errors.ERROR_BEGIN}Error{Errors.ERROR_END}: {description}");
	}

	public static Status Warning(Position? position, string description)
	{
		var location = string.Empty;

		if (position != null)
		{
			var fullname = Errors.UNKNOWN_FILE;

			if (position.File != null)
			{
				fullname = position.File.Fullname;
			}

			location = $"{fullname}:{position.FriendlyLine}:{position.FriendlyCharacter}";
		}
		else
		{
			location = Errors.UNKNOWN_LOCATION;
		}

		return Status.Error($"{location}: {Errors.WARNING_BEGIN}Warning{Errors.WARNING_END}: {description}");
	}
}