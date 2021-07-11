using System.Collections.Generic;
using System.Linq;

public static class Warnings
{
	/// <summary>
	/// Finds assignments which convert types suspiciously and reports them
	/// </summary>
	private static void FindSuspiciousAssignments(List<Status> diagnostics, Node root)
	{
		var assignments = root.FindAll(i => i.Is(Operators.ASSIGN));

		foreach (var assignment in assignments)
		{
			var to = assignment.Left.GetType();
			var from = assignment.Right.GetType();

			if (!Common.IsCastSafe(from, to))
			{
				var message = $"Unsafe cast from '{from}' to '{to}'";
				diagnostics.Add(Status.Warning(assignment.Position, message));
			}
			else if (assignment.Right.Is(NodeType.NUMBER))
			{
				if (from.Format.IsDecimal() && !to.Format.IsDecimal())
				{
					var message = $"Possible loss of information while converting the number to type '{to}'";
					diagnostics.Add(Status.Warning(assignment.Position, message));
				}
				else if (assignment.Right.To<NumberNode>().Bits > Size.FromFormat(to.Format).Bits)
				{
					var message = $"Possible loss of information while converting the number to type '{to}'";
					diagnostics.Add(Status.Warning(assignment.Right.Position, message));
				}
				else if (Numbers.IsNegative(assignment.Right.To<NumberNode>().To<NumberNode>().Value) && to.Format.IsUnsigned())
				{
					diagnostics.Add(Status.Warning(assignment.Position, "Converting signed number into unsigned format"));
				}
			}
		}
	}

	/// <summary>
	/// Finds function arguments which are converted suspiciously to other types
	/// </summary>
	private static void FindSuspiciousFunctionArguments(List<Status> diagnostics, Node root)
	{
		var calls = root.FindAll(NodeType.FUNCTION, NodeType.CALL);

		foreach (var call in calls)
		{
			var arguments = (call.Is(NodeType.FUNCTION) ? call : call.To<CallNode>().Parameters).ToArray();
			var types = call.Is(NodeType.FUNCTION) ? call.To<FunctionNode>().Function.ParameterTypes : call.To<CallNode>().Descriptor.Parameters!;

			// If the amount of arguments does not match the amount of parameters, this call should be skipped, because this would be weird and warning system should not be the one discovering it
			if (arguments.Length != types.Count) continue;

			// Iterate through all the function arguments and check whether they are safe
			for (var i = 0; i < types.Count; i++)
			{
				var argument = arguments[i];

				var expected = types[i];
				var actual = argument.GetType();

				// If the expected parameter type can not be resolved for some reason, skip the current parameter
				if (expected == null)
				{
					continue;
				}

				if (!Common.IsCastSafe(actual, expected))
				{
					var message = $"Unsafe cast from '{actual}' to '{expected}'";
					diagnostics.Add(Status.Warning(argument.Position, message));
				}
				else if (argument.Is(NodeType.NUMBER))
				{
					if (actual.Format.IsDecimal() && !expected.Format.IsDecimal())
					{
						var message = $"Possible loss of information while converting the number to type '{expected}'";
						diagnostics.Add(Status.Warning(argument.Position, message));
					}
					else if (argument.To<NumberNode>().Bits > Size.FromFormat(expected.Format).Bits)
					{
						var message = $"Possible loss of information while converting the number to type '{expected}'";
						diagnostics.Add(Status.Warning(argument.Position, message));
					}
					else if (Numbers.IsNegative(argument.To<NumberNode>().Value) && expected.Format.IsUnsigned())
					{
						diagnostics.Add(Status.Warning(argument.Position, "Converting signed number into unsigned format"));
					}
				}
			}
		}
	}

	/// <summary>
	/// Analyzes the specified node tree and reports warnings
	/// </summary>
	private static void Analyze(List<Status> diagnostics, Node root)
	{
		FindSuspiciousAssignments(diagnostics, root);
		FindSuspiciousFunctionArguments(diagnostics, root);
	}

	/// <summary>
	/// Finds all the variables which are not used and reports them
	/// </summary>
	private static void FindAllUnusedVariables(List<Status> diagnostics, FunctionImplementation implementation)
	{
		var lambdas = implementation.Node!.FindAll(NodeType.LAMBDA).Cast<LambdaNode>();
		var captures = lambdas.Where(i => i.Implementation != null).Select(i => (LambdaImplementation)i.Implementation!).SelectMany(i => i.Captures).Select(i => i.Captured).ToHashSet();

		foreach (var iterator in implementation.Locals.Concat(implementation.Parameters))
		{
			if (iterator.References.Any() || captures.Contains(iterator)) continue;
			if (iterator.IsParameter && implementation.VirtualFunction != null) continue;

			var message = iterator.IsParameter ? $"Unused parameter '{iterator.Name}'" : $"Unused local variable '{iterator.Name}'";
			diagnostics.Add(Status.Warning(iterator.Position, message));
		}
	}

	/// <summary>
	/// Analyzes the specified function implementation tree and reports warnings
	/// </summary>
	private static void Analyze(List<Status> diagnostics, FunctionImplementation implementation)
	{
		if (!implementation.Metadata.IsImported)
		{
			FindAllUnusedVariables(diagnostics, implementation);
		}
		
		Analyze(diagnostics, implementation.Node!);
	}

	/// <summary>
	/// Analyzes the specified context and returns warnings concerning the functions and types in it
	/// </summary>
	public static List<Status> Analyze(Context context)
	{
		var diagnostics = new List<Status>();

		foreach (var implementation in Common.GetAllFunctionImplementations(context))
		{
			Analyze(diagnostics, implementation);
		}

		return diagnostics;
	}
}