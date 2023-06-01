using System.Collections.Generic;
using System.Linq;

public static class ProjectBuilder
{
	/// <summary>
	/// Builds the specified file.
	/// If the flag 'all' is set to true, all the source files will be implemented
	/// </summary>
	public static Parse Build(Dictionary<SourceFile, DocumentParse> files, string filename, bool all = false, Function? function_filter = null)
	{
		// Find the source file which has the same filename as the specified filename
		var filter = files.Keys.First(i => i.Fullname == filename);

		// Revert changes in all the source files
		foreach (var iterator in files)
		{
			var file = iterator.Key;
			var parse = iterator.Value;

			if (parse.Recovery == null || parse.Context == null) continue;

			// Revert changes in the parsed context
			parse.Recovery.Recover(file, parse.Context);
		}

		var root = (Node?)null;
		var context = (Context?)null;

		// Merge all parsed files
		context = Parser.CreateRootContext(ParserPhase.ROOT_CONTEXT_IDENTITY);
		root = Parser.CreateRootNode(context);

		// Now merge all the parsed source files
		foreach (var file in files.Values)
		{
			if (file.Root == null || file.Context == null) continue;

			context.Merge(file.Context, false);
			root.Merge(file.Root.Clone());
		}

		// Applies all the extension functions
		ParserPhase.ApplyExtensionFunctions(context, root);

		// Empty out all other function implementations other than the function filter
		if (function_filter != null)
		{
			var functions = Common.GetAllVisibleFunctions(context);

			foreach (var function in functions)
			{
				function.Implementations.Clear();
			}

			var parse = files[filter];
			var blueprint = parse.Blueprints[function_filter];
	
			if (function_filter.IsTemplateFunction)
			{
				// Template functions save the function header in the blueprint: function() {...},
				// so add the tokens into the body.
				function_filter.Blueprint.Last().To<ParenthesisToken>().Tokens = blueprint;
			}
			else
			{
				function_filter.Blueprint = blueprint;
			}
		}

		// Preprocess the 'hull' of the code before creating functions
		Evaluator.Evaluate(context, root);

		var current = Resolver.GetReport(context, root);
		var evaluated = false;

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = current;

			Resolver.ResolveContext(context);
			Resolver.Resolve(context, root);

			current = Resolver.GetReport(context, root);

			if (current.SequenceEqual(previous)) break;
		}

		if (function_filter != null)
		{
			var parameter_types = function_filter!.Parameters.Select(i => i.Type).ToArray();
			if (parameter_types.Any(i => i == null || i.IsUnresolved)) return new Parse(context, root, files[filter].Tokens);

			function_filter.Implement(parameter_types!);
		}

		current = Resolver.GetReport(context, root);

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = current;

			if (function_filter == null)
			{
				ParserPhase.ImplementFunctions(context, all ? null : filter, true);
				GarbageCollector.CreateAllRequiredOverloads(context);
			}

			Resolver.ResolveContext(context);
			Resolver.Resolve(context, root);

			current = Resolver.GetReport(context, root);

			// Try again only if the errors have changed
			if (!current.SequenceEqual(previous)) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		return new Parse(context, root, files[filter].Tokens);
	}
}