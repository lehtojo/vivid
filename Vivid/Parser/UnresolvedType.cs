using System;
using System.Linq;
using System.Text;

public class UnresolvedTypeComponent
{
	public string Identifier { get; }
	public Type[] Arguments { get; }

	public UnresolvedTypeComponent(string identifier, Type[] arguments)
	{
		Identifier = identifier;
		Arguments = arguments;
	}

	public UnresolvedTypeComponent(string identifier)
	{
		Identifier = identifier;
		Arguments = Array.Empty<Type>();
	}

	public void Resolve(Context context)
	{
		// Resolve the template parameters
		for (var i = 0; i < Arguments.Length; i++)
		{
			var parameter = Arguments[i];
			var replacement = Resolver.Resolve(context, parameter);

			if (replacement == null) continue;

			Arguments[i] = replacement;
		}
	}

	public override string ToString()
	{
		var arguments = string.Join(", ", (object[])Arguments);
		return Identifier + (string.IsNullOrEmpty(arguments) ? string.Empty : $"<{arguments}>");
	}
}

public class UnresolvedType : Type, IResolvable
{
	private UnresolvedTypeComponent[] Components { get; }
	public ParenthesisToken? Size { get; set; }
	public int Pointers { get; set; } = 0;

	public UnresolvedType(string identifier) : base(string.Empty, Modifier.DEFAULT)
	{
		Components = new[] { new UnresolvedTypeComponent(identifier) };
	}

	public UnresolvedType(UnresolvedTypeComponent[] components, Position? position) : base(string.Empty, Modifier.DEFAULT)
	{
		Components = components;
		Position = position;
	}

	public override bool IsResolved()
	{
		return false;
	}

	/// <summary>
	/// Tries to resolve the type using the internal components
	/// </summary>
	public virtual Node? Resolve(Context context)
	{
		var environment = context;

		foreach (var component in Components)
		{
			component.Resolve(environment);

			var local = component != Components.First();

			if (!context.IsTypeDeclared(component.Identifier, local))
			{
				return null;
			}

			var type = context.GetType(component.Identifier)!;

			if (component.Arguments.Any())
			{
				// Require all of the arguments to be resolved
				if (component.Arguments.Any(i => i.IsUnresolved))
				{
					return null;
				}

				// Since the component has template arguments, the type must be a template type
				if (type.IsGenericType) return null;

				if (type is TemplateType)
				{
					// Get a variant of the template type using the arguments of the component
					context = type.To<TemplateType>().GetVariant(component.Arguments);
				}
				else
				{
					// Some base types are 'manual template types' such as link meaning they can still receive template arguments even though they are not instances of a template type class
					type = type.Clone();
					type.TemplateArguments = component.Arguments;

					context = type;
				}

				continue;
			}

			context = type;
		}

		var result = context.To<Type>();

		// Array types:
		if (Size != null)
		{
			return new TypeNode(new ArrayType(environment, result, Size, Position));
		}

		// Wrap the result type around pointers
		for (var i = 0; i < Pointers; i++)
		{
			var pointer = new Link(result);
			result = pointer;
		}

		return new TypeNode(result);
	}

	public Type? ResolveOrNull(Context context)
	{
		return Resolve(context)?.TryGetType();
	}

	public Type ResolveOrThis(Context context)
	{
		return Resolve(context)?.TryGetType() ?? this;
	}

	public override bool Equals(object? other)
	{
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(base.GetHashCode(), Components, Pointers);
	}

	public Status GetStatus()
	{
		return Status.Error(Position, $"Can not resolve type '{this}'");
	}

	public override string ToString()
	{
		var result = new StringBuilder(string.Join('.', (object[])Components));

		// Add the array specifier to the type if it exists
		if (Size != null) { result.Append(Size.ToString()); }

		// Add pointers to the end of the type
		result.Append('*', Pointers);

		return result.ToString();
	}
}