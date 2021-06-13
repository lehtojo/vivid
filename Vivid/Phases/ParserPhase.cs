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

	public const string OUTPUT = "parse";

	/// <summary>
	/// Parses all types under the specified root node
	/// </summary>
	public void ParseTypes(Node root)
	{
		var types = root.FindAll(NodeType.TYPE).Cast<TypeNode>().ToArray();

		foreach (var type in types)
		{
			Run(() =>
			{
				try
				{
					type.Parse();
				}
				catch (Exception e)
				{
					return Status.Error(e.Message);
				}

				return Status.OK;
			});
		}
	}

	/// <summary>
	/// Ensures that exported functions and virtual functions are implemented
	/// </summary>
	public static void ImplementFunctions(Context context, SourceFile? file, bool all = false)
	{
		foreach (var function in Common.GetAllVisibleFunctions(context))
		{
			// If the file filter is specified, skip all functions which are not defined inside that file
			if (file != null && function.Start?.File != file) continue;

			// Skip all functions which are not exported
			if (!all && !function.IsExported) continue;

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

					if (actual.Count != expected.Count || !actual.SequenceEqual(expected)) continue;

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

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains(LexerPhase.OUTPUT))
		{
			return Status.Error("Nothing to parse");
		}

		var files = bundle.Get<List<SourceFile>>(LexerPhase.OUTPUT);

		// Form the 'hull' of the code
		for (var i = 0; i < files.Count; i++)
		{
			var index = i;

			Run(() =>
			{
				var file = files[index];
				var context = Parser.CreateRootContext(index);
				var root = new ScopeNode(context, null, null);

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

				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Parse types, subtypes and their members
		for (var i = 0; i < files.Count; i++)
		{
			var index = i;

			Run(() =>
			{
				ParseTypes(files[index].Root!);
				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Merge all parsed files
		var context = new Context(ROOT_CONTEXT_IDENTITY);
		var root = Parser.CreateRootNode(context);
		
		// Prepare for importing libraries
		Importer.Initialize();

		// Import all the specified libraries
		var libraries = bundle.Get(ConfigurationPhase.LIBRARIES, Array.Empty<string>());

		foreach (var library in libraries)
		{
			Importer.Import(context, library, files);
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
			Run(() =>
			{
				try
				{
					iterator.Parse(context);
				}
				catch (Exception e)
				{
					return Status.Error(e.Message);
				}

				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Applies all the extension functions
		ApplyExtensionFunctions(context, root);

		// Ensure exported and virtual functions are implemented
		ImplementFunctions(context, null);

		// Preprocess the 'hull' of the code before creating functions
		Evaluator.Evaluate(context, root);
		
		// Implement the entry function if the output type does not represent library
		if (bundle.Get(ConfigurationPhase.OUTPUT_TYPE, BinaryType.EXECUTABLE) != BinaryType.STATIC_LIBRARY)
		{
			var function = context.GetFunction(Keywords.INIT.Identifier);

			if (function == null)
			{
				return Status.Error($"Can not find the entry function '{Keywords.INIT.Identifier}()'");
			}

			function.Overloads.First().Implement(new List<Type>());
		}

		// Save the parsed result
		bundle.Put(OUTPUT, new Parse(context, root));

		return Status.OK;
	}
}