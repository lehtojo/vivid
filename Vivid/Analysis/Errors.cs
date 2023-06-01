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
	public const string UNKNOWN_FILE = "<unknown>";
	public const string ERROR_BEGIN = "\x1B[1;31m";
	public const string ERROR_END = "\x1B[0m";
	public const string WARNING_BEGIN = "\x1B[1;33m";
	public const string WARNING_END = "\x1B[0m";

	public static string FormatPosition(Position? position)
	{
		if (position == null) return UNKNOWN_FILE;

		var file = position.File != null ? position.File.Fullname : UNKNOWN_FILE;

		return $"{file}:{position.FriendlyLine}:{position.FriendlyCharacter}";
	}

	public static string Format(Position? position, string message)
	{
		if (position == null) return $"{ERROR_BEGIN}Error{ERROR_END}: {message}";

		return $"{FormatPosition(position)}: {ERROR_BEGIN}Error{ERROR_END}: {message}";
	}

	public static string Format(Status status)
	{
		return Format(status.Position, status.Message);
	}

	public static Exception Get(Position? position, string message)
	{
		return new SourceException(position, Format(position, message));
	}
}