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
		context = new Context(ParserPhase.ROOT_CONTEXT_IDENTITY);
		root = new ScopeNode(context, null, null, false);

		// Now merge all the parsed source files
		foreach (var file in files.Values)
		{
			if (file.Root == null || file.Context == null) continue;

			context.Merge(file.Context, false);
			root.Merge(file.Root.Clone());
		}

		// Applies all the extension functions
		ParserPhase.ApplyExtensionFunctions(context, root);

		// TODO: Shell validation

		// Empty out all other function implementations other than the function filter
		if (function_filter != null)
		{
			var functions = Common.GetAllVisibleFunctions(context);

			foreach (var function in functions)
			{
				function.Implementations.Clear();
			}

			var parse = files[filter];
			function_filter.Blueprint = parse.Blueprints[function_filter];
		}

		// Preprocess the 'hull' of the code before creating functions
		Evaluator.Evaluate(context, root);

		var report = ResolverPhase.GetReport(context);
		var evaluated = false;

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			Resolver.ResolveContext(context);
			Resolver.Resolve(context, root);

			report = ResolverPhase.GetReport(context);

			if (report == previous) break;
		}

		if (function_filter != null)
		{
			var parameter_types = function_filter!.Parameters.Select(i => i.Type).ToArray();
			if (parameter_types.Any(i => i == null || i.IsUnresolved)) return new Parse(context, root, files[filter].Tokens);

			function_filter.Implement(parameter_types!);
		}

		report = ResolverPhase.GetReport(context);

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			if (function_filter != null)
			{
				foreach (var implementation in function_filter!.Implementations)
				{
					Resolver.ResolveImplementation(implementation);
				}
			}
			else
			{
				// Try to resolve any problems in the node tree
				ParserPhase.ApplyExtensionFunctions(context, root);
				ParserPhase.ImplementFunctions(context, all ? null : filter, true);

				Resolver.ResolveContext(context);
				Resolver.Resolve(context, root);

				report = ResolverPhase.GetReport(context);
			}

			// Try again only if the errors have changed
			if (report != previous) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		return new Parse(context, root, files[filter].Tokens);
	}
}