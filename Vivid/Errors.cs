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

	/// <summary>
	/// Prints to the console the specified description of the error and also the current stack trace.
	/// After the printing the application exits with the specified exit code
	/// </summary>
	public static void Abort(string description, int exit_code = 1)
	{
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine($"Terminated: {description}\n{Environment.StackTrace}");
		Console.ForegroundColor = ConsoleColor.White;
		Environment.Exit(exit_code);
	}
}