using System.Runtime.InteropServices;
using System.IO;
using System;

public static class ServiceUtility
{
	public const string FILE_SCHEME = "file://";
	public const string UNTITLED_FILE_SCHEME = "untitled:";

	public static string Normalize(string path)
	{
		// Ensure the path is expressed as absolute path
		path = Path.GetFullPath(path);

		return Path.GetFullPath(new Uri(path).LocalPath)
			.Replace('\\', '/')
			.TrimEnd('/')
			.ToLowerInvariant();
	}

	public static string Normalize(string path, string base_path)
	{
		// Ensure the path is expressed as absolute path
		path = Path.GetFullPath(path, base_path);

		return Path.GetFullPath(new Uri(path).LocalPath)
			.Replace('\\', '/')
			.TrimEnd('/')
			.ToLowerInvariant();
	}

	/// <summary>
	/// Converts the specified uri to string
	/// </summary>
	public static string ToPath(Uri uri)
	{
		var path = uri.ToString();
		if (string.IsNullOrEmpty(path)) return string.Empty;

		// Untitled file paths should not be edited
		if (path.StartsWith(UNTITLED_FILE_SCHEME)) return path;

		// Cut out the file scheme if it is present
		if (path.StartsWith(FILE_SCHEME)) { path = path[FILE_SCHEME.Length..]; }

		// Cut out the root slash on Windows
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) { path = path.TrimStart('/', '\\'); }

		return Normalize(path);
	}

	/// <summary>
	/// Converts the specified path to uri
	/// </summary>
	public static Uri ToUri(string path)
	{
		if (path.StartsWith(UNTITLED_FILE_SCHEME)) return new Uri(path);

		return new Uri(FILE_SCHEME + '/' + Normalize(path));
	}

	/// <summary>
	/// Converts the specified line number and character number into an absolute offset from the start of the specified text
	/// </summary>
	public static int? ToAbsolutePosition(string document, int line, int character)
	{
		if (line < 0 || character < 0) return null;

		var position = 0;

		for (var i = 0; i < line; i++)
		{
			var j = document.IndexOf('\n', position);
			if (j == -1) return null;

			position = j + 1;
		}

		position += character;

		return position > document.Length ? null : position;
	}
}