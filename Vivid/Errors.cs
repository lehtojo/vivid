using System;

public static class Errors
{
	public const string UNKNOWN_FILE = "[Unknown file]";
	public const string UNKNOWN_LOCATION = "[Unknown location]";
	public const string ERROR_BEGIN = "\x1B[1;31m";
	public const string ERROR_END = "\x1B[0m";

	public static Exception Get(Position position, string description)
	{
		var fullname = UNKNOWN_FILE;

		if (position.File != null)
		{
			fullname = position.File.Fullname;
		}

		return new Exception($"{fullname}:{position.FriendlyLine}:{position.FriendlyCharacter}: {ERROR_BEGIN}error{ERROR_END}: {description}");
	}

	public static string FormatPosition(Position position)
	{
		var fullname = UNKNOWN_FILE;

		if (position.File != null)
		{
			fullname = position.File.Fullname;
		}

		return $"{fullname}:{position.FriendlyLine}:{position.FriendlyCharacter}";
	}
}