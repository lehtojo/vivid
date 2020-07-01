using System;
using System.Globalization;

public static class Errors
{
	public static Exception Get(Position position, Exception exception)
	{
		return new Exception(string.Format(CultureInfo.InvariantCulture, "Line: {0}, Character: {1} | {2}", position.FriendlyLine, position.FriendlyCharacter, exception.Message));
	}

	public static Exception Get(Position position, string exception)
	{
		return new Exception(string.Format(CultureInfo.InvariantCulture, "Line: {0}, Character: {1} | {2}", position.FriendlyLine, position.FriendlyCharacter, exception));
	}

	public static Exception Get(Position position, string format, params object[] args)
	{
		return new Exception(string.Format(CultureInfo.InvariantCulture, "Line: {0}, Character: {1} | {2}", position.FriendlyLine, position.FriendlyCharacter, string.Format(CultureInfo.InvariantCulture, format, args)));
	}
}