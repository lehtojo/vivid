using System;
using System.Collections.Generic;
using System.Linq;

public class Parse
{
	public Context Context { get; private set; }
	public Node Node { get; private set; }

	public Parse(Context context, Node node)
	{
		Context = context;
		Node = node;
	}
}

public class ParserPhase : Phase
{
	private List<Exception> Errors { get; } = new List<Exception>();

	private void ParseTypes(Node root)
	{
		foreach (var node in root)
		{
			if (!node.Is(NodeType.TYPE))
			{
				continue;
			}

			var type = (TypeNode)node;

			Run(() =>
			{
				try
				{
					type.Parse();
				}
				catch (Exception e)
				{
					Errors.Add(e);
				}

				return Status.OK;
			});

			ParseTypes(type);
		}
	}

	/// <summary>
	/// Ensures that exported functions and virtual functions are implemented
	/// </summary>
	public static void ImplementRequiredFunctions(Context context)
	{
		foreach (var function in context.Functions.Values.SelectMany(i => i.Overloads).ToArray())
		{
			// Skip all functions which are not exported
			if (!function.IsExported)
			{
				continue;
			}

			// Retrieve the types of all parameters
			var types = function.Parameters.Select(i => i.Type).ToList();

			// If any of the parameters has an undefined type, it can not be implemented
			if (types.Any(i => i == Types.UNKNOWN || i.IsUnresolved))
			{
				continue;
			}

			// Force implement the current exported function
			function.Get(types!);
		}

		// Implement all virtual function overloads
		foreach (var type in context.Types.Values)
		{
			var virtual_functions = type.GetAllVirtualFunctions();

			foreach (var virtual_function in virtual_functions)
			{
				var overloads = type.GetFunction(virtual_function.Name)?.Overloads;

				if (overloads == null)
				{
					if (virtual_function.Parent != type)
					{
						// TODO: This should not be allowed since virtual functions should always be overloaded
						Console.WriteLine($"NOTE: Type '{type.Name}' contains virtual function '{virtual_function}' but it is not implemented");
					}

					continue;
				}

				var expected = virtual_function.Parameters.Select(i => i.Type).ToList();

				foreach (var overload in overloads)
				{
					var actual = overload.Parameters.Select(i => i.Type).ToList();

					if (actual.Count != expected.Count || !actual.SequenceEqual(expected))
					{
						continue;
					}

					var implementation = overload.Get(expected!) ?? throw new ApplicationException("Could not implement virtual function");
					implementation.VirtualFunction = virtual_function;
					implementation.ReturnType = virtual_function.ReturnType;
					break;
				}
			}
		}
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains("input_file_tokens"))
		{
			return Status.Error("Nothing to parse");
		}

		var files = bundle.Get<List<Token>[]>("input_file_tokens");
		var parses = new Parse[files.Length];

		// Form the 'hull' of the code
		for (var i = 0; i < files.Length; i++)
		{
			var index = i;

			Run(() =>
			{
				var tokens = files[index];

				var node = new Node();
				var context = Parser.Initialize();

				try
				{
					Parser.Parse(node, context, tokens);
				}
				catch (Exception e)
				{
					return Status.Error(e.Message);
				}

				parses[index] = new Parse(context, node);

				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Parse types, subtypes and their members
		for (int i = 0; i < files.Length; i++)
		{
			int index = i;

			Run(() =>
			{
				ParseTypes(parses[index].Node);
				return Status.OK;
			});
		}

		Sync();

		if (Failed)
		{
			return Status.Error(GetTaskErrors());
		}

		// Merge all parsed files
		var context = new Context();
		var root = new Node();

		foreach (var parse in parses)
		{
			context.Merge(parse.Context);
			root.Merge(parse.Node);
		}

		// Ensure exported and virtual functions are implemented
		ImplementRequiredFunctions(context);

		// Preprocess the 'hull' of the code before creating functions
		Preprocessor.Evaluate(context, root);

		var function = context.GetFunction(Keywords.INIT.Identifier);

		if (function == null)
		{
			return Status.Error($"Could not find the entry function '{Keywords.INIT.Identifier}'");
		}

		function.Overloads.First().Implement(new List<Type>());

		bundle.Put("parse", new Parse(context, root));

		return Status.OK;
	}
}