using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SourceFile
{
	public string Fullname { get; }
	public string Filename => Path.GetFileName(Fullname);
	public string? Folder => Path.GetDirectoryName(Fullname);
	public string Content { get; }
	public int Index { get; }
	public List<Token> Tokens { get; }
	public Node? Root { get; set; }
	public Context? Context { get; set; }

	public SourceFile(string filename, string content, int index)
	{
		Fullname = filename.Replace('\\', '/');
		Content = content;
		Index = index;
		Tokens = new List<Token>();
	}

	public string GetFilenameWithoutExtension()
	{
		return Path.GetFileNameWithoutExtension(Fullname);
	}

	public override bool Equals(object? other)
	{
		return other is SourceFile file && Fullname.Equals(file.Fullname);
	}

	public override int GetHashCode()
	{
		return Fullname.GetHashCode();
	}
}

public class FilePhase : Phase
{
	public override Status Execute()
	{
		var filenames = Settings.Filenames;
		if (!filenames.Any()) return new Status("Please enter input files");

		var files = new List<SourceFile>();

		for (var i = 0; i < filenames.Count; i++)
		{
			var filename = filenames[i];

			try
			{
				var content = File.ReadAllText(filename).Replace('\r', ' ').Replace('\t', ' ');
				files.Add(new SourceFile(filename, content, i + 1));
			}
			catch
			{
				return new Status("Could not load file '{0}'", filename);
			}
		}

		Settings.SourceFiles = files;
		return Status.OK;
	}
}