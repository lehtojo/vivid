using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ResolverPhase : Phase
{
	/*private static void Complain(List<string> errors)
	{
		foreach (var error in errors)
		{
			Console.Error.WriteLine($"ERROR: {error}");
		}
	}*/

	private static void Complain(string report)
	{
		Console.Error.WriteLine(report);
	}

	/*
	 *	Global function 'run':
	 *	ERROR: Couldn't resolve type of variable 'thread'
	 *	ERROR: Couldn't resolve function or constructor 'Thread'
	 * 
	 *	Terminated: Compilation failed
	 *	
	 *	Time: 199ms
	 */

	private static string GetFunctionReport(FunctionImplementation implementation)
	{
		var builder = new StringBuilder();

		if (implementation.IsMember)
		{
			var type = implementation.GetTypeParent();
			builder.Append($"Member function '{implementation.Metadata.Name}' of type '{type.Name}':\n");
		}
		else
		{
			builder.Append($"Global function '{implementation.Metadata.Name}':\n");
		}

		var errors = implementation.Node.FindAll(n => n is IResolvable).Cast<IResolvable>()
											.Select(r => r.GetStatus()).Where(s => s.IsProblematic);
		
		if (errors.Count() == 0)
		{
			return string.Empty;
		}

		foreach (var error in errors)
		{
			builder.Append($"ERROR: {error.Description}\n");
		}

		return builder.ToString();
	}

	private static string GetReport(Context context)
	{
		var builder = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			var report = GetReport(type); 
   
			if (report.Length > 0)
			{
				builder.Append(report).Append("\n\n");
			}
		}

		foreach (var implementation in context.GetImplementedFunctions())
		{
			var report = GetFunctionReport(implementation);

			if (report.Length > 0)
			{
				builder.Append(report).Append("\n\n");
			}
		}

		return builder.ToString();
	}

	public override Status Execute(Bundle bundle)
	{
		var parse = bundle.Get<Parse>("parse", null);

		if (parse == null)
		{
			return Status.Error("Nothing to resolve");
		}

		//var previous = new List<string>();
		var legacy = new List<string>();

		var context = parse.Context;
		var previous = string.Empty;
		var report = GetReport(context);

		if (report != previous)
		{
			// Try to resolve as long as errors change -- errors don't always decrease since the program may expand each cycle
			while (true)
			{
				previous = report;

				// Try to resolve any problems in the node tree
				Resolver.Resolve(context, legacy);
				report = GetReport(context);

				// Try again only if the errors have changed
				if (report == previous)
				{
					break;
				}
			}
		}

		// The program mustn't continue if the resolver phase failed
		if (report.Length > 0)
		{
			Complain(report);

			return Status.Error("Compilation error");
		}

		// Build inline functions
		Inlines.Build(context);

		// Align variables in memory
		Aligner.Align(context);

		return Status.OK;
	}
}