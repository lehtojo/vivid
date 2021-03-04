using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ResolverPhase : Phase
{
	/// <summary>
	/// Outputs the specified report to the error output
	/// </summary>
	public static void Complain(string report)
	{
		Console.Error.WriteLine(report);
	}

	/// <summary>
	/// Returns a string which describes the state of the specified function implementation
	/// </summary>
	public static string GetFunctionReport(FunctionImplementation implementation)
	{
		var builder = new StringBuilder();

		builder.Append($"Function {implementation.GetHeader()}:\n");

		var errors = new List<Status>();

		// Report if the return type is not resolved
		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			errors.Add(Status.Error(implementation.Metadata.Position, "Could not resolve the return type"));
		}

		// Look for errors under the implementation node
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

		// Look for variables which are not resolved
		errors.AddRange(implementation.Variables.Values
			.Where(v => v.IsUnresolved)
			.Select(v => Status.Error(v.Position, $"Could not resolve type of local variable '{v.Name}'"))
		);

		// Build the report if there are errors
		if (!errors.Any())
		{
			return string.Empty;
		}

		foreach (var error in errors)
		{
			builder.Append($"{error.Description}\n");
		}

		return builder.ToString();
	}

	/// <summary>
	/// Returns a string which describes the state of the specified context
	/// </summary>
	public static string GetReport(Context context)
	{
		var variables = new StringBuilder();

		foreach (var variable in context.Variables.Values.Where(variable => variable.IsUnresolved))
		{
			if (variable.Context.IsType)
			{
				variables.Append(Errors.Format(variable.Position, $"Could not resolve the type of the member variable '{variable.Name}'"));
			}
			else if (variable.Context.Parent == null)
			{
				variables.Append(Errors.Format(variable.Position, $"Could not resolve the type of the global variable '{variable.Name}'"));
			}

			variables.AppendLine();
		}

		var types = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			foreach (var supertype in type.Supertypes.Where(i => i.IsUnresolved))
			{
				types.AppendLine(Errors.Format(type.Position, $"Type '{type.Name}' could not inherit type '{supertype.Name}' since either it was not found or it would have caused a cyclic inheritance"));
			}

			var report = GetReport(type);

			if (string.IsNullOrEmpty(report))
			{
				continue;
			}

			types.Append(report);
			types.AppendLine();
		}

		var functions = new StringBuilder();

		foreach (var overload in context.Functions.Values.SelectMany(i => i.Overloads))
		{
			if (overload.Parameters.All(i => i.Type == null || !i.Type.IsUnresolved))
			{
				continue;
			}

			if (functions.Length > 0)
			{
				functions.AppendLine();
			}

			functions.AppendLine($"Function {overload}:");

			foreach (var parameter in overload.Parameters)
			{
				functions.AppendLine(Errors.Format(parameter.Position, $"Could not resolve the type of the parameter '{parameter.Name}'"));
			}
		}

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

		var builder = new StringBuilder(variables.ToString());

		if (builder.Length > 0)
		{
			builder.AppendLine();
		}

		builder.Append(types);

		if (builder.Length > 0)
		{
			builder.AppendLine();
		}

		builder.Append(functions);

		return builder.ToString();
	}

	/// <summary>
	/// Finds the implementations of the allocation and the inheritance functions and registers them to be used
	/// </summary>
	public static void RegisterDefaultFunctions(Context context)
	{
		var allocation_function = context.GetFunction("allocate") ?? throw new ApplicationException("Missing the allocation function, please implement it or include the standard library");
		var inheritance_function = context.GetFunction("inherits") ?? throw new ApplicationException("Missing the inheritance function, please implement it or include the standard library");

		Parser.AllocationFunction = allocation_function.GetImplementation(new List<Type> { Types.LARGE });
		Assembler.AllocationFunction = allocation_function.GetOverload(new List<Type> { Types.LARGE });

		Parser.InheritanceFunction = inheritance_function.GetImplementation(new List<Type> { Types.LINK, Types.LINK });
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains(ParserPhase.OUTPUT))
		{
			return Status.Error("Nothing to resolve");
		}

		var parse = bundle.Get<Parse>(ParserPhase.OUTPUT);

		var context = parse.Context;
		var report = GetReport(context);
		var evaluated = false;

		// Find the required functions
		RegisterDefaultFunctions(context);

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			// Try to resolve any problems in the node tree
			ParserPhase.ImplementRequiredFunctions(context);
			Resolver.ResolveContext(context);
			report = GetReport(context);

			// Try again only if the errors have changed
			if (report == previous)
			{
				if (!evaluated)
				{
					Analysis.Evaluate(context);
					evaluated = true;
					continue;
				}

				break;
			}
		}

		// The compiler must not continue if the resolver phase has failed
		if (report.Length > 0)
		{
			Complain(report);

			return Status.Error("Compilation error");
		}

		// Finds objects whose values should be evaluated or finalized
		Analysis.Complete(context);

		// Align variables in memory
		Aligner.Align(context);

		Analyzer.AnalyzeVariableUsages(parse.Node, context);

		// Apply analysis to the functions
		Analysis.Analyze(context);

		// Analyze the output
		Analyzer.Analyze(parse.Node, context);

		return Status.OK;
	}
}
