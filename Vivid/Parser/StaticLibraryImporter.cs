using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public static class StaticLibraryImporter
{
	public const int STATIC_LIBRARY_SYMBOL_TABLE_OFFSET = 68;
	public const int STATIC_LIBRARY_SYMBOL_TABLE_FIRST_LOCATION_ENTRY_OFFSET = STATIC_LIBRARY_SYMBOL_TABLE_OFFSET + sizeof(int);

	/// <summary>
	/// Iterates through the specified headers and looks for an export file and imports it.
	/// Export files contain exported source code such as template types and functions.
	/// </summary>
	private static bool ImportExportFile(Context context, byte[] bytes, List<StaticLibraryFormatFileHeader> headers, string library, List<SourceFile> files)
	{
		for (var i = 0; i < headers.Count; i++)
		{
			// Look for files which represent source code of this language
			var header = headers[i];

			// Ensure the file ends with the extension of this language
			if (!header.Filename.EndsWith(".exports")) continue;

			var start = header.PointerOfData;
			var end = start + header.Size;

			if (start < 0 || start > bytes.Length || end < 0 || end > bytes.Length) return false;
			
			// Since the file is source code, it can be converted into text
			var text = Encoding.UTF8.GetString(bytes[start..end]);
			var file = new SourceFile(library + "/" + header.Filename, text, files.Max(i => i.Index) + 1);

			files.Add(file);

			try
			{
				// Produce tokens from the template code
				file.Tokens.AddRange(Lexer.GetTokens(text));

				// Register the file to the produced tokens
				Lexer.RegisterFile(file.Tokens, file);

				// Parse all the tokens
				var root = new ScopeNode(context, null, null, false);

				Parser.Parse(root, context, file.Tokens);
				
				file.Root = root;
				file.Context = context;

				// Find all the types and parse them
				var type_nodes = root.FindAll(NodeType.TYPE_DEFINITION).Cast<TypeDefinitionNode>().ToArray();

				foreach (var type_node in type_nodes)
				{
					type_node.Parse();
				}

				// Find all the namespaces and parse them
				var namespace_nodes = root.FindAll(NodeType.NAMESPACE).Cast<NamespaceNode>().ToArray();

				foreach (var namespace_node in namespace_nodes)
				{
					namespace_node.Parse(context);
				}
			}
			catch
			{
				// If an exception is thrown, it means that the template code from library can not be compiled for some reason
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Iterates through the specified file headers and imports all object files by adding them to the specified object file list.
	/// Object files are determined using filenames stored in the file headers.
	/// </summary>
	private static void ImportObjectFilesFromStaticLibrary(string file, List<StaticLibraryFormatFileHeader> headers, byte[] bytes, Dictionary<SourceFile, BinaryObjectFile> object_files)
	{
		foreach (var header in headers)
		{
			// Ensure the file ends with the extension of this language
			var is_object_file = header.Filename.EndsWith(".o") || header.Filename.EndsWith(".obj") || header.Filename.EndsWith(".o/") || header.Filename.EndsWith(".obj/");
			if (!is_object_file) continue;

			var object_file_name = file + '/' + header.Filename;
			var object_file_bytes = bytes[header.PointerOfData..(header.PointerOfData + header.Size)];
			var object_file = Settings.IsTargetWindows
				? PeFormat.Import(object_file_name, object_file_bytes)
				: ElfFormat.Import(object_file_name, object_file_bytes);

			var object_file_source = new SourceFile(object_file_name, string.Empty, -1);
			object_files.Add(object_file_source, object_file);
		}
	}

	/// <summary>
	/// Imports all template type variants using the specified static library file headers
	/// </summary>
	private static void ImportTemplateTypeVariants(Context context, List<StaticLibraryFormatFileHeader> headers, byte[] bytes)
	{
		foreach (var header in headers)
		{
			if (!header.Filename.EndsWith(".types.templates")) continue;

			var template_variant_bytes = bytes[header.PointerOfData..(header.PointerOfData + header.Size)];
			var template_variants = Encoding.UTF8.GetString(template_variant_bytes).Split('\n');

			foreach (var template_variant in template_variants)
			{
				if (string.IsNullOrWhiteSpace(template_variant)) continue;

				// Create the template variant from the current line
				var imported_type = Common.ReadType(context, Lexer.GetTokens(template_variant));
				if (imported_type == null) throw new Exception("Could not to import template type variant");

				imported_type.Modifiers |= Modifier.IMPORTED;
			}
		}
	}

	/// <summary>
	/// Imports all template function variants using the specified static library file headers
	/// </summary>
	private static void ImportTemplateFunctionVariants(Context context, List<StaticLibraryFormatFileHeader> headers, byte[] bytes)
	{
		foreach (var header in headers)
		{
			if (!header.Filename.EndsWith(".functions.templates")) continue;

			var template_variant_bytes = bytes[header.PointerOfData..(header.PointerOfData + header.Size)];
			var template_variants = Encoding.UTF8.GetString(template_variant_bytes).Split('\n');

			foreach (var template_variant_text in template_variants)
			{
				if (string.IsNullOrWhiteSpace(template_variant_text)) continue;

				// Extract the container type components
				var tokens = Lexer.GetTokens(template_variant_text);
				var components = new List<UnresolvedTypeComponent>();

				while (tokens.Any())
				{
					components.Add(Common.ReadTypeComponent(context, tokens));

					// Stop collecting type components if there are no tokens left or if the next token is not a dot operator
					if (!tokens.Any() || !tokens.First().Is(Operators.DOT)) break;

					tokens.Pop();
				}

				if (tokens.Count != 1) throw new ApplicationException("Missing template function variant parameter types");
				
				// Extract the parameter types
				var parameter_types = new List<Type>();
				tokens = tokens.First().To<ParenthesisToken>().Tokens;

				while (tokens.Count > 0)
				{
					var parameter_type = Common.ReadType(context, tokens);
					if (parameter_type == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);

					parameter_types.Add(parameter_type);

					if (tokens.Count == 0) break;
					if (tokens.Pop()!.Is(Operators.COMMA)) continue;

					throw new ApplicationException("Could not import template function variant: " + template_variant_text);
				}

				// Extract the type, which will contain the template function variant
				var environment = context;

				if (components.Count > 1)
				{
					environment = new UnresolvedType(components.GetRange(0, components.Count - 1).ToArray(), null).ResolveOrNull(context);
					if (environment == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);
				}

				var template_function_name = components.Last().Identifier;

				// Find the template function from the container type
				var template_function = environment.GetFunction(template_function_name);
				if (template_function == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);

				// Now, find the overload which accepts the template arguments
				var template_variant = template_function.GetImplementation(parameter_types, components.Last().Arguments);
				if (template_variant == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);

				template_variant.IsImported = true;
			}
		}
	}

	/// <summary>
	/// Imports the specified static library by finding the exported symbols and importing them
	/// </summary>
	private static bool InternalImportStaticLibrary(Context context, string file, List<SourceFile> files, Dictionary<SourceFile, BinaryObjectFile> object_files)
	{
		var bytes = File.ReadAllBytes(file);
		var entries = BitConverter.ToInt32(bytes[STATIC_LIBRARY_SYMBOL_TABLE_OFFSET..(STATIC_LIBRARY_SYMBOL_TABLE_OFFSET + sizeof(int))].Reverse().ToArray());

		// Skip all of the location entries to reach the actual symbol names
		var position = STATIC_LIBRARY_SYMBOL_TABLE_FIRST_LOCATION_ENTRY_OFFSET + entries * sizeof(int);

		// Load all the exported symbols
		var exported_symbols = PeFormat.LoadNumberOfStrings(bytes, position, entries);
		if (exported_symbols == null) return false;

		var headers = LoadFileHeaders(bytes);
		if (!headers.Any()) return false;

		ImportExportFile(context, bytes, headers, file, files);
		ImportObjectFilesFromStaticLibrary(file, headers, bytes, object_files);
		ImportTemplateTypeVariants(context, headers, bytes);
		ImportTemplateFunctionVariants(context, headers, bytes);
		return true;
	}

	/// <summary>
	/// Assigns the actual filenames to the specified file headers from the specified filename table
	/// </summary>
	private static bool LoadFilenames(byte[] bytes, StaticLibraryFormatFileHeader filenames, List<StaticLibraryFormatFileHeader> headers)
	{
		foreach (var header in headers)
		{
			// Look for files which have names such as: /10
			if (!header.Filename.StartsWith("/") || header.Filename.Length <= 1) continue;

			var digits = header.Filename[1..];

			if (!digits.Any() || digits.Any(i => !char.IsDigit(i))) continue;

			// This still might fail if the index is too large
			if (!int.TryParse(digits.ToArray(), out int offset)) continue;

			// Compute the position of the filename
			var position = filenames.PointerOfData + offset;

			// Check whether the position is out of bounds
			if (position < 0 || position >= bytes.Length) continue;

			var name = Encoding.UTF8.GetString(bytes.Skip(position).TakeWhile(i => i != 0).ToArray());

			header.Filename = name;
		}

		return true;
	}

	/// <summary>
	/// Loads all static library file headers from the specified file.
	/// Returns an empty list if it fails, since static libraries should not be empty
	/// </summary>
	public static List<StaticLibraryFormatFileHeader> LoadFileHeaders(byte[] bytes)
	{
		var headers = new List<StaticLibraryFormatFileHeader>();
		var position = StaticLibraryFormat.SIGNATURE.Length;

		while (position < bytes.Length)
		{
			// If a line ending is encountered, it means that the file headers have been consumed
			if (bytes[position] == '\n') break;

			// Extract the file name
			var name_buffer = bytes[position..(position + StaticLibraryFormat.FILENAME_LENGTH)];
			var name = Encoding.UTF8.GetString(name_buffer.TakeWhile(i => i != StaticLibraryFormat.PADDING_VALUE).ToArray());
			
			// Extract the file size
			position += StaticLibraryFormat.FILENAME_LENGTH + StaticLibraryFormat.TIMESTAMP_LENGTH + StaticLibraryFormat.IDENTITY_LENGTH * 2 + StaticLibraryFormat.FILEMODE_LENGTH;
			
			// Load the file size text into a string
			var size_text_buffer = bytes[position..(position + StaticLibraryFormat.SIZE_LENGTH)];
			var size_text = Encoding.UTF8.GetString(size_text_buffer.TakeWhile(i => i != StaticLibraryFormat.PADDING_VALUE).ToArray());

			// Parse the file size
			if (!int.TryParse(size_text, out int size)) return new List<StaticLibraryFormatFileHeader>();

			// Go to the end of the header, that is the start of the file data
			position += StaticLibraryFormat.SIZE_LENGTH + StaticLibraryFormat.END_COMMAND.Length;

			headers.Add(new StaticLibraryFormatFileHeader(name, size, position));

			// Skip to the next header
			position += size;
			position += position % 2;
		}

		// Try to find the section which has the actual filenames
		/// NOTE: Sometimes file headers contain their actual filenames
		var i = headers.FindIndex(i => i.Filename == StaticLibraryFormat.FILENAME_TABLE_NAME);

		// If the filename table was found, apply it to the headers
		if (i != -1 && !LoadFilenames(bytes, headers[i], headers)) return new List<StaticLibraryFormatFileHeader>();

		return headers;
	}

	/// <summary>
	/// Imports the specified file.
	/// This function assumes the file represents a library
	/// </summary>
	public static bool Import(Context context, string file, List<SourceFile> files, Dictionary<SourceFile, BinaryObjectFile> object_files)
	{
		var import_context = Parser.CreateRootContext(file);

		InternalImportStaticLibrary(import_context, file, files, object_files);

		// Ensure all functions are marked as imported
		var functions = Common.GetAllVisibleFunctions(import_context);

		foreach (var function in functions)
		{
			function.Modifiers |= Modifier.IMPORTED;

			// Create default implementations for imported functions that do not require template arguments
			var parameter_types = function.Parameters.Select(i => i.Type).ToArray();

			if (!function.IsTemplateFunction && parameter_types.All(i => i != null && !i.IsUnresolved))
			{
				function.Get(parameter_types!);
			}

			// Register the default implementations as imported
			foreach (var implementation in function.Implementations)
			{
				implementation.IsImported = true;
			}
		}

		// Ensure all types are marked as imported
		var types = Common.GetAllTypes(import_context);

		foreach (var type in types)
		{
			type.Modifiers = Modifier.Combine(type.Modifiers, Modifier.IMPORTED);
		}

		// TODO: Verify all parameter types are resolved
		context.Merge(import_context);
		return true;
	}
}