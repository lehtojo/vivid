using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class UnresolvedIdentifier : Node, IResolvable
{
	public string Value { get; private set; }

	public UnresolvedIdentifier(string value, Position? position)
	{
		Value = value;
		Position = position;
		Instance = NodeType.UNRESOLVED_IDENTIFIER;
	}
	
	/// <summary>
	/// Creates a lambda node from the specified function by copying it
	/// </summary>
	private Node? CreateLambdaByCopyingFunction(Context context, Function function, List<Type> types)
	{
		// Allow only member functions which are static
		if (function.IsMember && !function.IsStatic)
		{
			return null;
		}

		var name = context.CreateLambda().ToString(CultureInfo.InvariantCulture);
		var blueprint = function.Blueprint.Select(i => (Token)i.Clone()).ToList();

		var lambda = new Lambda(context, Modifier.DEFAULT, name, blueprint);
		var parameters = function.Parameters.Zip(types, (i, j) => new Parameter(i.Name, i.Position, j));

		lambda.Position = Position;
		lambda.Parameters.AddRange(parameters);

		return new LambdaNode(lambda.Implement(types), Position!);
	}

	/// <summary>
	/// Tries to convert this identifier into a function pointer
	/// </summary>
	private Node? TryResolveAsFunctionPointer(Context context)
	{
		// Check whether a function with same name as this identifier exists
		if (!context.IsFunctionDeclared(Value))
		{
			return null;
		}

		var function = context.GetFunction(Value) ?? throw new ApplicationException("Could not find the function");

		// There should be at least one overload
		if (!function.Overloads.Any())
		{
			return null;
		}

		Function? overload;

		if (function.Overloads.Count == 1)
		{
			overload = function.Overloads.First();

			// Require all of the parameters are resolved
			if (overload.Parameters.Any(i => i.Type == null || i.Type.IsUnresolved))
			{
				return null;
			}

			return CreateLambdaByCopyingFunction(context, overload, overload.Parameters.Select(i => i.Type!).ToList());
		}

		// Check if this identifier is casted and contains information about which overload to choose
		if (Parent == null || !Parent.Is(NodeType.CAST))
		{
			return null;
		}

		var type = Parent.To<CastNode>().TryGetType();

		// Require that the type of the cast is resolved
		if (type is not FunctionType descriptor)
		{
			return null;
		}

		var types = descriptor.Parameters;

		// Require all of the parameters are resolved
		if (types.Any(i => i == null || i.IsUnresolved))
		{
			return null;
		}

		// Try to get a function overload using the types of the call descriptor
		overload = function.GetOverload(types!);

		if (overload != null)
		{
			return CreateLambdaByCopyingFunction(context, overload, types!);
		}

		return null;
	}

	public Node? GetResolvedNode(Context context)
	{
		var linked = Parent != null && Parent.Is(NodeType.LINK);
		var result = Singleton.GetIdentifier(context, new IdentifierToken(Value, Position!), linked);

		return result.Is(NodeType.UNRESOLVED_IDENTIFIER) ? TryResolveAsFunctionPointer(context) : result;
	}

	public Node? Resolve(Context context)
	{
		return GetResolvedNode(context);
	}

	public override Type? TryGetType()
	{
		return Types.UNKNOWN;
	}

	public Status GetStatus()
	{
		return Status.Error(Position, $"Could not resolve identifier '{Value}'");
	}

	public override string ToString()
	{
		return "?";
	}

	public override bool Equals(object? other)
	{
		return other is UnresolvedIdentifier identifier &&
				base.Equals(other) &&
				Value == identifier.Value;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Value);
	}
}