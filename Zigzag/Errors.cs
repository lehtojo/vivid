using System;
using System.Globalization;

public static class Errors
{
	public static Exception Get(Position position, Exception exception)
	{
		return new Exception(message:
			$"Line: {position.FriendlyLine}, Character: {position.FriendlyCharacter} | {exception.Message}");
	}

	public static Exception Get(Position position, string exception)
	{
		return new Exception($"Line: {position.FriendlyLine}, Character: {position.FriendlyCharacter} | {exception}");
	}

	public static Exception Get(Position position, string format, params object[] args)
	{
		return new Exception(
			$"Line: {position.FriendlyLine}, Character: {position.FriendlyCharacter} | {string.Format(CultureInfo.InvariantCulture, format, args)}");
	}
}