using System;
using System.Linq;

public class UnresolvedType : Type, IResolvable
{
	private new string Name { get; }

	public UnresolvedType(Context context, string name) : base(context)
	{
		Name = name;
		TemplateArguments = Array.Empty<Type>();
	}

	public UnresolvedType(Context context, string name, Type[] arguments) : base(context)
	{
		Name = name;
		TemplateArguments = arguments;
	}

	public override bool IsResolved()
	{
		return false;
	}

	public Node? Resolve(Context context)
	{
		// Resolve potential template parameters
		for (var i = 0; i < TemplateArguments.Length; i++)
		{
			var parameter = TemplateArguments[i];
			var replacement = Resolver.Resolve(context, parameter);

			if (replacement == null)
			{
				continue;
			}

			TemplateArguments[i] = replacement;
		}

		// If any of the parameters is unresolved, then this type can not be resolved yet
		if (TemplateArguments.Any(i => i.IsUnresolved))
		{
			return null;
		}

		// Return if the type is not yet declared
		if (!context.IsTypeDeclared(Name))
		{
			return null;
		}

		var type = context.GetType(Name)!;

		if (!TemplateArguments.Any())
		{
			return new TypeNode(type);
		}

		if (type is TemplateType template_type)
		{
			return new TypeNode(template_type.GetVariant(TemplateArguments));
		}

		// Some base types are "manual template types" such as link meaning they can still receive template arguments even though they are not instances of a template type class
		if (type.IsTemplateType)
		{
			// Clone the type since it is shared and add the template types
			type = type.Clone();
			type.TemplateArguments = TemplateArguments;
			return new TypeNode(type);
		}

		return null;
	}

	public Type? TryResolveType(Context context)
	{
		return Resolve(context)?.TryGetType();
	}

	public Status GetStatus()
	{
		var template_parameters = string.Join(", ", TemplateArguments.Select(i => i.IsUnresolved ? "?" : i.ToString()));

		var descriptor = Name + (string.IsNullOrEmpty(template_parameters) ? string.Empty : $"<{template_parameters}>");

		return Status.Error(Position, $"Could not resolve type '{descriptor}'");
	}

	public override string ToString()
	{
		var template_parameters = string.Join(", ", TemplateArguments.Select(i => i.IsUnresolved ? "?" : i.ToString()));
		var descriptor = Name + (string.IsNullOrEmpty(template_parameters) ? string.Empty : $"<{template_parameters}>");

		return descriptor;
	}
}