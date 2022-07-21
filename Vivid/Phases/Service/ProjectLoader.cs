using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

public static class ProjectLoader
{
	public const string STANDARD_LIBRARY_FILENAME = "v.lib";

	public static ProjectConfiguration? TryLoadProjectConfiguration(string folder)
	{
		try
		{
			// Load all file paths from the specified folder
			var files = Directory.GetFiles(folder).ToList();

			// Try to find the first project configuration file and load it
			var project_file_index = files.FindIndex(i => i.EndsWith(ProjectConfiguration.Extension));
			if (project_file_index < 0) return null;

			return ProjectConfiguration.Load(files[project_file_index]);
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Tries to find the specified library.
	/// If the function succeeds, it adds the library import file to the specified source files and returns true.
	/// Otherwise, nothing happens and false is returned.
	/// </summary>
	public static void LoadLibrary(Project project, string library)
	{

	}

	/// <summary>
	/// Removes all the information from previous sessions and analyzes the files in the specified folder and in the standard library
	/// </summary>
	public static void OpenProject(Project project, string folder)
	{
		// Remove all the files which were prepared previously
		project.Reset();

		// Load the standard library by default
		LoadLibrary(project, STANDARD_LIBRARY_FILENAME);

		// Prepare the opened folder if it is not empty
		if (!string.IsNullOrEmpty(folder)) LoadAll(project, folder);
	}

	/// <summary>
	/// Loads the project from the specified folder
	/// </summary>
	public static void LoadAll(Project project, string folder)
	{
		var configuration = TryLoadProjectConfiguration(folder);
		var source_files = (List<string>?)null;

		if (configuration != null)
		{
			var build_configuration = configuration.DefaultConfiguration;

			if (build_configuration != null)
			{
				source_files = build_configuration.FindSourceFiles(folder);
			}
		}

		if (source_files == null)
		{
			// Find all source files from the specified folder recursively
			source_files = Directory.GetFiles(folder, $"*{ConfigurationPhase.VIVID_EXTENSION}", SearchOption.AllDirectories).ToList();
		}

		foreach (var path in source_files)
		{
			var document = File.ReadAllText(path).Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');
			var file = project.GetSourceFile(path);
			var parse = project.GetParse(file);

			Parse(file, parse, document);
		}

		foreach (var parse in project.Documents.Values)
		{
			var context = parse.Context;
			var root = parse.Root;
			if (context == null || root == null) continue;

			Finalize(parse, context, root);
		}
	}

	/// <summary>
	/// Saves the blueprints of all the functions in the specified parse and then removes them from their functions.
	/// </summary>
	public static void ExtractFunctionBlueprints(DocumentParse parse)
	{
		var functions = Common.GetAllVisibleFunctions(parse.Context!);

		// Remove any previous blueprints
		parse.Blueprints.Clear();

		foreach (var function in functions)
		{
			parse.Blueprints[function] = function.Blueprint;
			function.Blueprint = new List<Token>();
		}
	}

	/// <summary>
	/// Tokenizes and parses the specified document
	/// </summary>
	public static void Parse(SourceFile file, DocumentParse parse, string document)
	{
		// Tokenize the document
		var tokens = Lexer.GetTokens(document);

		Lexer.Postprocess(tokens);
		Lexer.RegisterFile(tokens, file);

		parse.Tokens = new List<Token>(tokens);

		// Parse the document
		var context = Parser.CreateRootContext(file.Index);
		var root = new ScopeNode(context, null, null, false);

		Parser.Parse(root, context, tokens);

		parse.Root = root;
		parse.Context = context;
	}

	/// <summary>
	/// Finalizes the specified parse by parsing types and namespaces and then generating a context recovery and extracting function blueprints.
	/// </summary>
	public static void Finalize(DocumentParse parse, Context context, Node root)
	{
		// Parse all types
		var types = root.FindAll(NodeType.TYPE);

		foreach (var i in types)
		{
			i.To<TypeNode>().Parse();
		}

		// Parse all namespaces
		var namespaces = root.FindAll(NodeType.NAMESPACE);

		foreach (var i in namespaces)
		{
			i.To<NamespaceNode>().Parse(context);
		}

		ExtractFunctionBlueprints(parse);

		// Create a context recovery for reverting changes in the context
		parse.Recovery = new ContextRecovery(context);
	}

	/// <summary>
	/// Prepare the specified file for building.
	/// If the specified parse was reused, tokens of the changed function are returned.
	/// Otherwise, null is returned.
	/// </summary>
	public static List<Token> Update(SourceFile file, DocumentParse parse, string document)
	{
		// Update the document
		parse.Document = document;

		Parse(file, parse, document);
		Finalize(parse, parse.Context!, parse.Root!);

		return parse.Tokens;
	}
}