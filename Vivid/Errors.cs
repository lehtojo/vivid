using System;
using System.Diagnostics.CodeAnalysis;

[SuppressMessage("Microsoft.Maintainability", "CA1032")]
public class SourceException : Exception
{
	public Position? Position { get; }

	public SourceException(Position? position, string message) : base(message) 
	{
		Position = position;
	}
}

public static class Errors
{
	public const string UNKNOWN_FILE = "<Source>";
	public const string UNKNOWN_LOCATION = "<Source>:<Line>:<Character>";
	public const string ERROR_BEGIN = "\x1B[1;31m";
	public const string ERROR_END = "\x1B[0m";

	public static Exception Get(Position? position, string description)
	{
		if (position == null)
		{
			return new SourceException(null, $"{ERROR_BEGIN}Error{ERROR_END}: {description}");
		}

		var fullname = UNKNOWN_FILE;

		if (position.File != null)
		{
			fullname = position.File.Fullname;
		}

		return new SourceException(position, $"{fullname}:{position.FriendlyLine}:{position.FriendlyCharacter}: {ERROR_BEGIN}Error{ERROR_END}: {description}");
	}

	public static string Format(Position? position, string description)
	{
		if (position == null)
		{
			return $"{ERROR_BEGIN}Error{ERROR_END}: {description}";
		}

		var fullname = UNKNOWN_FILE;

		if (position.File != null)
		{
			fullname = position.File.Fullname;
		}

		return $"{fullname}:{position.FriendlyLine}:{position.FriendlyCharacter}: {ERROR_BEGIN}Error{ERROR_END}: {description}";
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