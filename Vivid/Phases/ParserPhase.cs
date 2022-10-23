using System;
using System.Collections.Generic;
using System.Linq;

public class Parse
{
	public Context Context { get; private set; }
	public Node Node { get; private set; }
	public List<Token> Tokens { get; }

	public Parse(Context context, Node node)
	{
		Context = context;
		Node = node;
		Tokens = new List<Token>();
	}

	public Parse(Context context, Node node, List<Token> tokens)
	{
		Context = context;
		Node = node;
		Tokens = tokens;
	}
}

public class ParserPhase : Phase
{
	public const string ROOT_CONTEXT_IDENTITY = "0";

	/// <summary>
	/// Ensures that exported functions and virtual functions are implemented
	/// </summary>
	public static void ImplementFunctions(Context context, SourceFile? file, bool all = false)
	{
		foreach (var function in Common.GetAllVisibleFunctions(context))
		{
			// If the file filter is specified, skip all functions which are not defined inside that file
			if (file != null && function.Start?.File != file) continue;

			var is_function_exported = function.IsExported || (function.Parent != null && function.Parent.IsType && function.Parent.To<Type>().IsExported);

			// Skip all functions which are not exported
			if (!all && !is_function_exported) continue;

			// Template functions can not be implemented
			if (function.IsTemplateFunction) continue;

			// Retrieve the types of all parameters
			var types = function.Parameters.Select(i => i.Type).ToList();

			// If any of the parameters has an undefined type, it can not be implemented
			if (types.Any(i => i == null || i.IsUnresolved)) continue;

			// Force implement the current exported function
			function.Get(types!);
		}

		// Implement all virtual function overloads
		foreach (var type in Common.GetAllTypes(context))
		{
			// Find all virtual functions
			var virtual_functions = type.GetAllVirtualFunctions();

			foreach (var virtual_function in virtual_functions)
			{
				var overloads = type.GetOverride(virtual_function.Name)?.Overloads;

				if (overloads == null) continue;

				var expected = virtual_function.Parameters.Select(i => i.Type).ToList();

				foreach (var overload in overloads)
				{
					// If the file filter is specified, skip all functions which are not defined inside that file
					if (file != null && overload.Start?.File != file) continue;

					var actual = overload.Parameters.Select(i => i.Type).ToList();

					if (actual.Count != expected.Count || !actual.SequenceEqual(expected) || expected.Any(i => i == null || i.IsUnresolved)) continue;

					var implementation = overload.Get(expected!) ?? throw new ApplicationException("Could not implement virtual function");
					implementation.VirtualFunction = virtual_function;

					if (virtual_function.ReturnType != null)
					{
						implementation.ReturnType = virtual_function.ReturnType;
					}
					
					break;
				}
			}
		}
	}

	/// <summary>
	/// Finds all the extension functions under the specified node and tries to apply them
	/// </summary>
	public static void ApplyExtensionFunctions(Context context, Node root)
	{
		var extensions = root.FindAll(NodeType.EXTENSION_FUNCTION);

		foreach (var extension in extensions)
		{
			Resolver.Resolve(context, extension);
		}
	}

	/// <summary>
	/// Goes through all the specified types and ensures all their supertypes are resolved
	/// </summary>
	public static void ValidateSupertypes(List<Type> types)
	{
		foreach (var type in types)
		{
			Resolver.ResolveSupertypes(type.Parent!, type);
			if (type.Supertypes.All(i => i.IsResolved())) continue;

			throw new ApplicationException($"Could not resolve supertypes for type {type.Name}");
		}
	}

	/// <summary>
	/// Validates the shell of the context.
	/// Shell means all the types, functions and variables, but not the code.
	/// </summary>
	public static Status ValidateShell(Context context)
	{
		var types = Common.GetAllTypes(context);

		try
		{
			ValidateSupertypes(types);
		}
		catch (Exception e)
		{
			return Status.Error(e.Message);
		}

		return Status.OK;
	}

	public override Status Execute()
	{
		var files = Settings.SourceFiles;
		var context = (Context?)null;
		var root = (Node?)null;

		for (var i = 0; i < files.Count; i++)
		{
			var file = files[i];

			context = Parser.CreateRootContext(i);
			root = new ScopeNode(context, null, null, false);

			try
			{
				Parser.Parse(root, context, file.Tokens);
			}
			catch (Exception e)
			{
				return Status.Error(e.Message);
			}

			file.Root = root;
			file.Context = context;
		}

		// Parse all types and their members
		foreach (var file in files)
		{
			var types = file.Root!.FindAll(NodeType.TYPE_DEFINITION).Cast<TypeDefinitionNode>().ToList();
			foreach (var type in types) { type.Parse(); }
		}

		// Merge all parsed files
		context = Parser.CreateRootContext(ROOT_CONTEXT_IDENTITY);
		root = Parser.CreateRootNode(context);

		// Import all the specified libraries
		var libraries = Settings.Libraries;
		var object_files = Settings.ObjectFiles;

		foreach (var library in libraries)
		{
			if (library.EndsWith(".lib") || library.EndsWith(".a"))
			{
				StaticLibraryImporter.Import(context, library, files, object_files);
			}
		}

		// Now merge all the parsed source files
		foreach (var file in files)
		{
			context.Merge(file.Context!);
			root.Merge(file.Root!);
		}

		// Parse all the namespaces since all code has been merged
		foreach (var iterator in root.FindAll(NodeType.NAMESPACE).Cast<NamespaceNode>())
		{
			try
			{
				iterator.Parse(context);
			}
			catch (Exception e)
			{
				return Status.Error(e.Message);
			}
		}

		// Applies all the extension functions
		ApplyExtensionFunctions(context, root);

		// Validate the shell before proceeding
		var result = ValidateShell(context);

		if (result.IsProblematic)
		{
			return result;
		}

		// Ensure exported and virtual functions are implemented
		ImplementFunctions(context, null);

		// Preprocess the 'hull' of the code before creating functions
		Evaluator.Evaluate(context, root);
		
		// Implement the entry function if the output type does not represent library
		if (Settings.OutputType != BinaryType.STATIC_LIBRARY)
		{
			var function = context.GetFunction(Keywords.INIT.Identifier);

			if (function == null)
			{
				return Status.Error($"Can not find the entry function '{Keywords.INIT.Identifier}()'");
			}

			function.Overloads.First().Get(new List<Type>());
		}

		// Save the parsed result
		Settings.Parse = new Parse(context, root);

		return Status.OK;
	}
}