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
	private Node? CreateLambdaFromFunction(Context context, Function function, List<Type> types)
	{
		// Allow only member functions which are static
		if (function.IsMember && !function.IsStatic) return null;

		// Template functions are not supported here
		if (function.IsTemplateFunction) return null;

		// Create the parameters from the specified function
		var parameters = function.Parameters.Zip(types, (i, j) => new Parameter(i.Name, i.Position, j));
		var position = Position!;

		// Create a blueprint which calls the specified function
		// Example: => Namespace.Type.function(a, b, c)
		var blueprint = new List<Token> { new OperatorToken(Operators.HEAVY_ARROW, position)};
		
		// Add the namespaces or types which contain the specified function before the function token
		if (function.Parent != null && function.Parent.IsType)
		{
			blueprint.AddRange(Common.GetTokens(function.Parent.To<Type>(), position));
			blueprint.Add(new OperatorToken(Operators.DOT, position));
		}

		// Now call the specified function by forwarding the lambda parameters
		blueprint.Add(new FunctionToken
		(
			new IdentifierToken(function.Name, position),
			new ContentToken(parameters.Select(i => (Token)new IdentifierToken(i.Name, position)).ToList()),
			position
		));

		var name = context.CreateLambda().ToString(CultureInfo.InvariantCulture);
		var lambda = new Lambda(context, Modifier.DEFAULT, name, blueprint, Position, null);

		lambda.Parameters.AddRange(parameters);

		return new LambdaNode(lambda.Implement(types), position);
	}

	/// <summary>
	/// Tries to convert this identifier into a function pointer
	/// </summary>
	private Node? TryResolveAsFunctionPointer(Context context, bool linked)
	{
		// Check whether a function with same name as this identifier exists
		if (!context.IsFunctionDeclared(Value, linked))
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

			return CreateLambdaFromFunction(context, overload, overload.Parameters.Select(i => i.Type!).ToList());
		}

		var parent = FindParent(i => !i.Is(NodeType.LINK));

		// Check if this identifier is casted and contains information about which overload to choose
		if (parent == null || !parent.Is(NodeType.CAST))
		{
			return null;
		}

		var type = parent.To<CastNode>().TryGetType();

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
			return CreateLambdaFromFunction(context, overload, types!);
		}

		return null;
	}

	public Node? Resolve(Context context)
	{
		var linked = Parent != null && Parent.Is(NodeType.LINK);
		var result = Singleton.GetIdentifier(context, new IdentifierToken(Value, Position!), linked);

		return result.Is(NodeType.UNRESOLVED_IDENTIFIER) ? TryResolveAsFunctionPointer(context, linked) : result;
	}

	public override Type? TryGetType()
	{
		return null;
	}

	public Status GetStatus()
	{
		return Status.Error(Position, $"Can not resolve identifier '{Value}'");
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

	public override string ToString() => $"Unresolved Identifier {Value}";
}