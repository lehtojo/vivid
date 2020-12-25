using System;
using System.Collections.Generic;
using System.Linq;

public class UnresolvedFunction : Node, IResolvable, IType
{
	public string Name { get; }
	public Type[] TemplateArguments { get; }

	/// <summary>
	/// Creates an unresolved function with a function name to look for
	/// </summary>
	/// <param name="name">Function name</param>
	public UnresolvedFunction(string name, Position? position)
	{
		Name = name;
		TemplateArguments = Array.Empty<Type>();
		Position = position;
	}

	/// <summary>
	/// Creates an unresolved function with a function name to look for with the specified template parameters
	/// </summary>
	/// <param name="name">Function name</param>
	public UnresolvedFunction(string name, Type[] template_arguments, Position? position)
	{
		Name = name;
		TemplateArguments = template_arguments;
		Position = position;
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
	/// Returns all parameters as pairs containing the parameter type and the node
	/// </summary>
	private List<(Type Type, Node Node)>? GetParameterTypes()
	{
		var types = new List<(Type, Node)>();

		foreach (var parameter in this)
		{
			var type = parameter.TryGetType();

			if (type == Types.UNKNOWN)
			{
				return null;
			}

			types.Add((type, parameter));
		}

		return types;
	}

	/// <summary>
	/// Tries to resolve any typeless short functions inside the parameters of this unresolved function
	/// </summary>
	private void TryResolveShortFunctionParameters(Context environment, List<(Type Type, Node Node)> parameters, List<Type> types)
	{
		// Collect all the parameters which are unresolved
		var unresolved = parameters.Where(p => p.Type.IsUnresolved).ToArray();

		// Ensure all the unresolved parameter types represent lambda types
		if (!unresolved.All(p => p.Type is CallDescriptorType && p.Node.Is(NodeType.LAMBDA)) || !environment.IsFunctionDeclared(Name))
		{
			return;
		}

		// Find all the functions overloads with the name of this unresolved function
		var functions = environment.GetFunction(Name)!;

		// Find all the function overloads that could accept the currently resolved parameter types
		var candidates = functions.Overloads.Where(f => f.Parameters
			.Select(p => p.Type)
			.Zip(types)
			.All(i => i.First == null || i.Second.IsUnresolved || Resolver.GetSharedType(i.First, i.Second) != null)

		).ToList();

		Type?[] expected_types;

		// Filter out all candidates where the type of the parameter matching the unresolved lambda type is not a lambda type
		for (var c = candidates.Count - 1; c >= 0; c--)
		{
			var candidate = candidates[c];
			expected_types = candidate.Parameters.Select(p => p.Type).ToArray();

			for (var i = 0; i < expected_types.Length; i++)
			{
				var expected = expected_types[i];
				var actual = types[i];

				// Skip all parameter types which don't represent lambda types
				if (expected == null || !(actual is CallDescriptorType))
				{
					continue;
				}

				if (!(expected is CallDescriptorType))
				{
					// Since the actual parameter type is lambda type and the expected is not, the current candidate can be removed
					candidates.RemoveAt(c);
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
			// Skip all parameter types which don't represent lambda types
			if (!(expected_types[i] is CallDescriptorType expected))
			{
				continue;
			}

			var actual = (CallDescriptorType)types[i];

			// Ensure the parameter types don't conflict
			if (expected.Parameters.Count != actual.Parameters.Count ||
				!expected.Parameters.Zip(actual.Parameters).All(i => i.Second == null || i.Second == i.First))
			{
				return;
			}

			// Since none of the parameters conflicted with the expected parameters types, the expected parameter types can be transfered
			for (var j = 0; j < expected.Parameters.Count; j++)
			{
				parameters[i].Node.To<LambdaNode>().Lambda.Parameters[j].Type = expected.Parameters[j];
			}
		}
	}

	public Node? Solve(Context environment, Context primary)
	{
		// Try to solve all the parameters
		foreach (var parameter in this)
		{
			Resolver.Resolve(environment, parameter);
		}

		// Get parameter types
		var parameters = GetParameterTypes();

		// Parameter types must be known
		if (parameters == null)
		{
			return null;
		}

		var types = parameters.Select(p => p.Type).ToList();

		if (types.Any(t => t.IsUnresolved))
		{
			TryResolveShortFunctionParameters(environment, parameters, types);
			return null;
		}

		// Try to find a suitable function by name and parameter types
		var function = Singleton.GetFunctionByName(primary, Name, types, TemplateArguments);

		// First, ensure this function can be a virtual or a lambda call
		if (function == null && primary == environment && !TemplateArguments.Any())
		{
			// Try to form a virtual function call
			var result = Common.TryGetVirtualFunctionCall(environment, Name, this, types!);

			if (result != null)
			{
				result.Position = Position;
				return result;
			}

			// Try to form a lambda function call
			result = Common.TryGetLambdaCall(environment, Name, this, types!);

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
			return new ConstructionNode(node, node.Position);
		}

		// When the environment context is the same as the current context it means that this function is not part of a link
		// When the function is a member function and the this function is not part of a link it means that the function needs the self pointer
		if (function.IsMember && environment == primary)
		{
			var self = environment.GetSelfPointer() ?? throw new ApplicationException("Missing self pointer");

			return new LinkNode(new VariableNode(self, Position), node, Position);
		}

		return node;
	}

	public new Type? GetType()
	{
		return Types.UNKNOWN;
	}

	public Node? Resolve(Context context)
	{
		return Solve(context, context);
	}

	public override NodeType GetNodeType()
	{
		return NodeType.UNRESOLVED_FUNCTION;
	}

	public Status GetStatus()
	{
		var template_parameters = string.Join(", ", TemplateArguments.Select(i => i.IsUnresolved ? "?" : i.ToString()));
		var descriptor = Name + (string.IsNullOrEmpty(template_parameters) ? string.Empty : $"<{template_parameters}>");

		descriptor += $"({string.Join(", ", ((IEnumerable<Node>)this).Select(p => p.TryGetType()?.ToString() ?? "?"))})";

		return Status.Error(Position, $"Could not find function or constructor '{descriptor}'");
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