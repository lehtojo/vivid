using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ResolverPhase : Phase
{
	private static void Complain(string report)
	{
		Console.Error.WriteLine(report);
	}

	private static string GetFunctionReport(FunctionImplementation implementation)
	{
		var builder = new StringBuilder();

		if (implementation.IsMember)
		{
			var type = implementation.GetTypeParent()!;
			builder.Append($"Member function '{implementation.GetHeader()}' of type '{type.Name}':\n");
		}
		else
		{
			builder.Append($"Global function '{implementation.GetHeader()}':\n");
		}
		
		var errors = new List<Status>();

		if (implementation.ReturnType?.IsUnresolved ?? false)
		{
			errors.Add(
				Status.Error("Couldn't resolve the return type")
			);
		}

		if (!Equals(implementation.Node, null))
		{
			errors.AddRange(
				implementation.Node
					.FindAll(n => n is IResolvable)
					.Cast<IResolvable>()
					.Select(r => r.GetStatus())
					.Where(s => s.IsProblematic)
					.ToList()
			);
		}
		
		errors.AddRange(
			implementation.Variables.Values
				.Where(v => v.IsUnresolved)
				.Select(v => Status.Error($"Couldn't resolve type of local variable '{v.Name}'"))
		);
		
		if (!errors.Any())
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

		foreach (var variable in context.Variables.Values.Where(variable => variable.IsUnresolved))
		{
			if (variable.Context.IsType)
			{
				var type = (Type)variable.Context;
				builder.Append($"ERROR: Couldn't resolve type of member variable '{variable.Name}' of type '{type.Name}'");
			}
			else if (variable.Context.IsGlobal)
			{
				builder.Append($"ERROR: Couldn't resolve type of global variable '{variable.Name}'");
			}
			else
			{
				var function = variable.Context.GetFunctionParent() ?? throw new ApplicationException("Couldn't get the function");
				builder.Append($"ERROR: Couldn't resolve type of local variable '{variable.Name}' of function '{function.GetHeader()}'");
			}
		}

		foreach (var report in context.Types.Values.Select(GetReport).Where(report => report.Length > 0))
		{
			builder.Append(report).Append("\n\n");
		}

		foreach (var implementation in context.GetFunctionImplementations())
		{
			var report = GetFunctionReport(implementation);
			var subreport = GetReport(implementation);

			if (subreport.Length > 0)
			{
				report += "\n" + subreport;
			}

			if (report.Length > 0)
			{
				builder.Append(report).Append("\n\n");
			}
		}
		
		return builder.ToString();
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains("parse"))
		{
			return Status.Error("Nothing to resolve");
		}

		var parse = bundle.Get<Parse>("parse");

		var context = parse.Context;
		var report = GetReport(context);

		// Try to resolve as long as errors change -- errors don't always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			// Try to resolve any problems in the node tree
			Resolver.ResolveContext(context);
			report = GetReport(context);

			// Try again only if the errors have changed
			if (report == previous)
			{
				break;
			}
		}

		// The compiler must not continue if the resolver phase has failed
		if (report.Length > 0)
		{
			Complain(report);

			return Status.Error("Compilation error");
		}

		// Build inline functions
		Inlines.Build(context);

		// Align variables in memory
		Aligner.Align(context);

		// Analyze the output
		Analyzer.Analyze(context);
		
		// Apply analysis to the functions
		Analysis.Analyze(context);

		return Status.OK;
	}
}