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
	public static void LoadLibrary(Dictionary<SourceFile, DocumentParse> files, string library)
	{

	}

	/// <summary>
	/// Removes all the information from previous sessions and analyzes the files in the specified folder and in the standard library
	/// </summary>
	public static void OpenProject(Dictionary<SourceFile, DocumentParse> files, string folder)
	{
		// Remove all the files which were prepared previously
		files.Clear();

		// Load the standard library by default
		LoadLibrary(files, STANDARD_LIBRARY_FILENAME);

		// Prepare the opened folder if it is not empty
		if (!string.IsNullOrEmpty(folder)) LoadAndParseAll(files, folder);
	}

	/// <summary>
	/// Loads the specified source file and creates tokens from its content
	/// </summary>
	public static void LoadSourceFile(Dictionary<SourceFile, DocumentParse> files, string filename)
	{
		var content = File.ReadAllText(filename).Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

		// Find the source file which has the same filename as the specified filename
		var file = files.Keys.FirstOrDefault(i => i.Fullname == filename);
		var index = file != null ? file.Index : files.Count;

		if (file == null)
		{
			file = new SourceFile(filename, content, index);
		}

		files[file] = new DocumentParse(Lexer.GetTokens(content));
	}

	/// <summary>
	/// Builds all the source files in the specified folder
	/// </summary>
	public static void LoadAndParseAll(Dictionary<SourceFile, DocumentParse> files, string folder)
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

		foreach (var source_file in source_files)
		{
			LoadSourceFile(files, source_file);
		}

		ParseAll(files);
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
	/// Removes blueprints from all the functions inside the specified parse
	/// </summary>
	public static void RemoveFunctionBlueprints(DocumentParse parse)
	{
		var functions = Common.GetAllVisibleFunctions(parse.Context!);

		foreach (var function in functions)
		{
			function.Blueprint = new List<Token>();
		}
	}

	public static void ResolveOld(SourceFile file, DocumentParse parse)
	{
		var root = parse.Root;
		var context = parse.Context;

		if (root == null || context == null) throw new ApplicationException("Missing root context and node");

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
	/// Prepare the specified file for building
	/// </summary>
	public static void ParseAll(Dictionary<SourceFile, DocumentParse> files)
	{
		foreach (var file in files.Keys.ToArray())
		{
			Parse(file, files[file]);
		}

		foreach (var file in files.Keys.ToArray())
		{
			ResolveOld(file, files[file]);
		}
	}

	/// <summary>
	/// Prepare the specified file for building
	/// </summary>
	public static void Parse(SourceFile file, DocumentParse parse)
	{
		var tokens = new List<Token>(parse.Tokens);

		// Join the tokens now, because the 'source' list now contains the most accurate version of the document
		Lexer.Join(tokens);
		Lexer.RegisterFile(tokens, file);

		// Parse the document
		var context = Parser.CreateRootContext(file.Index);
		var root = new ScopeNode(context, null, null, false);

		Parser.Parse(root, context, tokens);

		parse.Root = root;
		parse.Context = context;
	}

	/// <summary>
	/// Prepare the specified file for building.
	/// If the specified parse was reused, tokens of the changed function are returned.
	/// Otherwise, null is returned.
	/// </summary>
	public static List<Token>? Parse(SourceFile file, DocumentParse parse, string document)
	{
		// Store the previous document for comparison
		var previous_document = parse.Document;

		// Update the document
		parse.Document = document;

		// Try to reuse the existing parse by comparing the old and the new document
		var tokens = TryReuseParse(file, parse, previous_document, document);

		// If the parse was reused, return the tokens of the changed function
		if (tokens != null) return tokens;

		// Tokenize the document
		tokens = Lexer.GetTokens(document);
		Lexer.Join(tokens);
		Lexer.RegisterFile(tokens, file);

		parse.Tokens = new List<Token>(tokens);

		// Parse the document
		var context = Parser.CreateRootContext(file.Index);
		var root = new ScopeNode(context, null, null, false);

		Parser.Parse(root, context, tokens);

		parse.Root = root;
		parse.Context = context;
		return null;
	}

	/// -------------------------------------------------------------------------------------

	public static void Resolve(DocumentParse parse, Context context, Node root)
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
	/// Tries to find the function that contains all the changes in the specified document and updates its blueprint.
	/// This function returns if the described process is successful.
	/// </summary>
	public static List<Token>? TryReuseParse(SourceFile file, DocumentParse parse, string previous_document, string current_document)
	{
		/// TODO: Look into this
		/// NOTE: Disable reusing for now, because the method down below does not adjust positions of shifted code, which can cause problems
		return null;

		// Get the changed line range between the old and the new document
		var result = CursorInformationProvider.GetChangedLineRange(previous_document, current_document);
		if (result == null) return null;

		var changes = result.Value;

		// Try to find the function, which contains the changed lines
		var changed_function = CursorInformationProvider.FindChangedFunction(changes, parse);
		if (changed_function == null) return null;

		// Get the tokens of the changed function
		var changed_tokens = CursorInformationProvider.GetChangedFunctionTokens(file, changes, changed_function);
		if (changed_tokens == null) return null;

		// Replace the tokens of the changed function with the new tokens
		parse.Blueprints[changed_function] = changed_tokens;

		return changed_tokens;
	}

	/// <summary>
	/// Prepare the specified file for building.
	/// If the specified parse was reused, tokens of the changed function are returned.
	/// Otherwise, null is returned.
	/// </summary>
	public static List<Token> Update(SourceFile file, DocumentParse parse, string document)
	{
		// Store the previous document for comparison
		var previous_document = parse.Document;

		// Update the document
		parse.Document = document;

		// Try to reuse the existing parse by comparing the old and the new document
		var tokens = TryReuseParse(file, parse, previous_document, document);

		// If the parse was reused, return the tokens of the changed function
		if (tokens != null)
		{
			Console.WriteLine("Reused");
			RemoveFunctionBlueprints(parse);
			return tokens;
		}

		// Tokenize the document
		tokens = Lexer.GetTokens(document);
		Lexer.Join(tokens);
		Lexer.RegisterFile(tokens, file);

		parse.Tokens = new List<Token>(tokens);

		// Parse the document
		var context = Parser.CreateRootContext(file.Index);
		var root = new ScopeNode(context, null, null, false);

		Parser.Parse(root, context, tokens);

		parse.Root = root;
		parse.Context = context;

		Resolve(parse, context, root);
		return parse.Tokens;
	}
}