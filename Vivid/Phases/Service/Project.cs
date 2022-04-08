using System.Collections.Generic;
using System;

public class Project
{
	public Dictionary<string, SourceFile> Files { get; } = new();
	public Dictionary<SourceFile, DocumentParse> Documents { get; } = new();

	private int FileId = 0;
	public int NextFileId => FileId++;

	public SourceFile GetSourceFile(string path)
	{
		if (!Files.TryGetValue(path, out var file))
		{
			file = new SourceFile(path, string.Empty, NextFileId);
			Files[path] = file;
		}

		return file;
	}

	public SourceFile GetSourceFile(Uri uri)
	{
		return GetSourceFile(ServiceUtility.ToPath(uri));
	}

	public DocumentParse GetParse(SourceFile file)
	{
		if (!Documents.TryGetValue(file, out var document))
		{
			document = new DocumentParse();
			Documents[file] = document;
		}

		return document;
	}

	public DocumentParse GetParse(string path)
	{
		return GetParse(GetSourceFile(path));
	}

	public DocumentParse GetParse(Uri uri)
	{
		return GetParse(ServiceUtility.ToPath(uri));
	}

	public List<Token> Update(string path, string document)
	{
		var file = GetSourceFile(path);
		var parse = GetParse(file);

		// Updated document
		return ProjectLoader.Update(file, parse, document);
	}

	public List<Token> Update(Uri uri, string document)
	{
		return Update(ServiceUtility.ToPath(uri), document);
	}

	public void Reset()
	{
		Files.Clear();
		Documents.Clear();
		FileId = 0;
	}
}