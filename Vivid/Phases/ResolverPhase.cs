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
			builder.Append($"Function {implementation.GetHeader()}:\n");
		}
		else
		{
			builder.Append($"Function {implementation.GetHeader()}:\n");
		}

		var errors = new List<Status>();

		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			errors.Add(Status.Error("Could not resolve return type"));
		}

		if (!Equals(implementation.Node, null))
		{
			errors.AddRange(implementation.Node
				.FindAll(n => n is IResolvable)
				.Cast<IResolvable>()
				.Select(r => r.GetStatus())
				.Where(s => s.IsProblematic)
				.ToList()
			);
		}

		errors.AddRange(implementation.Variables.Values
			.Where(v => v.IsUnresolved)
			.Select(v => Status.Error($"Could not resolve type of local variable '{v.Name}'"))
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
		var variables = new StringBuilder();

		foreach (var variable in context.Variables.Values.Where(variable => variable.IsUnresolved))
		{
			if (variable.Context.IsType)
			{
				variables.Append($"ERROR: Could not resolve type of member variable '{variable.Name}' of type '{(Type)variable.Context}'");
			}
			else if (variable.Context.Parent == null)
			{
				variables.Append($"ERROR: Could not resolve type of global variable '{variable.Name}'");
			}

			variables.AppendLine();
		}

		var types = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			var report = GetReport(type);

			if (string.IsNullOrEmpty(report))
			{
				continue;
			}

			types.Append(report);
			types.AppendLine();
		}

		var functions = new StringBuilder();

		foreach (var implementation in context.GetFunctionImplementations())
		{
			var report = GetFunctionReport(implementation);
			var subreport = GetReport(implementation);

			if (!string.IsNullOrEmpty(report))
			{
				report += "\n" + subreport;
			}

			if (!string.IsNullOrEmpty(report))
			{
				if (functions.Length > 0)
				{
					functions.AppendLine();
				}

				functions.Append(report);
			}
		}

		var final = new StringBuilder(variables.ToString());

		if (final.Length > 0)
		{
			final.AppendLine();
		}

		final.Append(types.ToString());

		if (final.Length > 0)
		{
			final.AppendLine();
		}

		final.Append(functions.ToString());

		return final.ToString();
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

		Analysis.Complete(context);

		// Build inline functions
		//Inlines.Build(context);

		// Align variables in memory
		Aligner.Align(context);

		// Apply analysis to the functions
		Analysis.Analyze(context);

		// Analyze the output
		Analyzer.Analyze(context, parse.Node);

		return Status.OK;
	}
}
