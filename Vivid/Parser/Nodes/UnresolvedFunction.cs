using System;
using System.Collections.Generic;
using System.Linq;

public class UnresolvedFunction : Node, IResolvable
{
	public string Name { get; }
	public Type[] Arguments { get; }

	/// <summary>
	/// Creates an unresolved function with a function name to look for
	/// </summary>
	/// <param name="name">Function name</param>
	public UnresolvedFunction(string name, Position? position)
	{
		Name = name;
		Arguments = Array.Empty<Type>();
		Position = position;
		Instance = NodeType.UNRESOLVED_FUNCTION;
	}

	/// <summary>
	/// Creates an unresolved function with a function name to look for with the specified template parameters
	/// </summary>
	/// <param name="name">Function name</param>
	public UnresolvedFunction(string name, Type[] template_arguments, Position? position)
	{
		Name = name;
		Arguments = template_arguments;
		Position = position;
		Instance = NodeType.UNRESOLVED_FUNCTION;
	}

	/// <summary>
	/// Transfers the specified parameters to this unresolved function
	/// </summary>
	public UnresolvedFunction SetParameters(Node parameters)
	{
		foreach (var parameter in parameters)
		{
			Add(parameter);
		}

		return this;
	}

	/// <summary>
	/// Tries to resolve any typeless short functions inside the parameters of this unresolved function
	/// </summary>
	private void TryResolveShortFunctionParameters(Context primary, List<(Type? Type, Node Node)> parameters)
	{
		// Collect all the parameters which are unresolved
		var unresolved = parameters.Where(p => p.Type == null || p.Type.IsUnresolved).ToArray();

		// Ensure all the unresolved parameter types represent lambda types
		if (!unresolved.All(p => p.Node.Is(NodeType.LAMBDA)) || !primary.IsFunctionDeclared(Name))
		{
			return;
		}

		// Collect all parameter types leaving all lambda types as nulls
		var actual_types = parameters.Select(i => i.Type).ToList();

		// Find all the functions overloads with the name of this unresolved function
		var functions = primary.GetFunction(Name)!;

		// Find all the function overloads that could accept the currently resolved parameter types
		var candidates = functions.Overloads.Where(i =>
		{
			var types = actual_types.Zip(i.Parameters.Select(i => i.Type), (a, b) => a ?? b).ToList();
			return types.All(i => i != null && !i.IsUnresolved) && i.Passes(types!, Arguments);

		}).ToList();

		// Collect all parameter types but this time filling the unresolved lambda types with incomplete call descriptor types
		actual_types = parameters.Select(i => i.Type ?? i.Node.To<LambdaNode>().GetIncompleteType()).ToList()!;

		Type?[] expected_types;

		// Filter out all candidates where the type of the parameter matching the unresolved lambda type is not a lambda type
		for (var i = candidates.Count - 1; i >= 0; i--)
		{
			expected_types = candidates[i].Parameters.Select(p => p.Type).ToArray();

			for (var j = 0; j < expected_types.Length; j++)
			{
				var expected = expected_types[j];
				var actual = actual_types[j];

				// Skip all parameter types which do not represent lambda types
				if (expected == null || actual is not FunctionType)
				{
					continue;
				}

				if (expected is not FunctionType)
				{
					// Since the actual parameter type is lambda type and the expected is not, the current candidate can be removed
					candidates.RemoveAt(i);
					break;
				}
			}
		}

		// Resolve the lambda type only if there's only one option left since the analysis would go too complex
		if (candidates.Count != 1)
		{
			return;
		}

		var match = candidates.First();
		expected_types = match.Parameters.Select(p => p.Type).ToArray();

		for (var i = 0; i < expected_types.Length; i++)
		{
			// Skip all parameter types which do not represent lambda types
			/// NOTE: It is ensured that when the expected type is a call descriptor the actual type is as well
			if (expected_types[i] is not FunctionType expected)
			{
				continue;
			}

			var actual = (FunctionType)actual_types[i]!;

			// Ensure the parameter types do not conflict
			if (expected.Parameters.Count != actual.Parameters.Count ||
				!expected.Parameters.Zip(actual.Parameters).All(i => i.Second == null || i.Second == i.First))
			{
				return;
			}

			// Since none of the parameters conflicted with the expected parameters types, the expected parameter types can be transfered
			for (var j = 0; j < expected.Parameters.Count; j++)
			{
				parameters[i].Node.To<LambdaNode>().Function.Parameters[j].Type = expected.Parameters[j];
			}
		}
	}

	public Node? Solve(Context environment, Context primary)
	{
		var linked = environment != primary;

		// Try to solve all the parameters
		foreach (var parameter in this)
		{
			Resolver.Resolve(environment, parameter);
		}

		// Get parameter types
		var parameters = this.Select(i => (Type: i.TryGetType(), Node: i)).ToList();
		var types = parameters.Select(i => i.Type).ToList();

		if (types.Any(i => i == null || i.IsUnresolved))
		{
			TryResolveShortFunctionParameters(primary, parameters);
			return null;
		}

		// First, ensure this function can be a lambda call
		if (!linked && !Arguments.Any())
		{
			// Try to form a lambda function call
			var result = Common.TryGetLambdaCall(environment, Name, this, types!);

			if (result != null)
			{
				result.Position = Position;
				return result;
			}
		}

		// Try to find a suitable function by name and parameter types
		var function = Singleton.GetFunctionByName(primary, Name, types!, Arguments, linked);

		// Lastly, try to form a virtual function call if the function could not be found
		if (function == null && !linked && !Arguments.Any())
		{
			// Try to form a virtual function call
			var result = Common.TryGetVirtualFunctionCall(environment, Name, this, types!);

			if (result != null)
			{
				result.Position = Position;
				return result;
			}
		}

		if (function == null)
		{
			return null;
		}

		var node = new FunctionNode(function, Position).SetParameters(this);

		if (function.IsConstructor)
		{
			var type = function.GetTypeParent() ?? throw new ApplicationException("Missing constructor parent type");

			// Consider the following situations:
			// Namespace.Type() <- Construction
			// Namespace.Type<large>() <- Construction
			// Namespace.Type.init() <- Direct call
			// Namespace.Type<large>.init() <- Direct call
			// Therefore, construction is only needed when the function name matches the name of the constructed type
			return type.Identifier != Name ? node : (Node)new ConstructionNode(node, node.Position);
		}

		// When the environment context is the same as the current context it means that this function is not part of a link
		// When the function is a member function and the this function is not part of a link it means that the function needs the self pointer
		if (function.IsMember && !function.IsStatic && !linked)
		{
			var self = Common.GetSelfPointer(environment, Position);

			return new LinkNode(self, node, Position);
		}

		return node;
	}

	public override Type? TryGetType()
	{
		return Types.UNKNOWN;
	}

	public Node? Resolve(Context context)
	{
		return Solve(context, context);
	}

	public Status GetStatus()
	{
		var template_parameters = string.Join(", ", Arguments.Select(i => i.IsUnresolved ? "?" : i.ToString()));
		var descriptor = Name + (string.IsNullOrEmpty(template_parameters) ? string.Empty : $"<{template_parameters}>");

		descriptor += $"({string.Join(", ", ((IEnumerable<Node>)this).Select(p => p.TryGetType()?.ToString() ?? "?"))})";

		return Status.Error(Position, $"Could not find function '{descriptor}'");
	}

	public override bool Equals(object? other)
	{
		return other is UnresolvedFunction function &&
			   base.Equals(other) &&
			   Name == function.Name;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Name);
		return hash.ToHashCode();
	}
}