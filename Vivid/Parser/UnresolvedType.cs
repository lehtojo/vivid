using System;
using System.Linq;

public class UnresolvedType : Type, IResolvable
{
	private string Name { get; }
	private Type[] Parameters { get; }

	public UnresolvedType(Context context, string name) : base(context)
	{
		Name = name;
		Parameters = Array.Empty<Type>();
	}

	public UnresolvedType(Context context, string name, Type[] parameters) : base(context)
	{
		Name = name;
		Parameters = parameters;
	}

	public override bool IsResolved()
	{
		return false;
	}

	public Node? Resolve(Context context)
	{
		// Resolve potential template parameters
		for (var i = 0; i < Parameters.Length; i++)
		{
			var parameter = Parameters[i];
			var replacement = Resolver.Resolve(context, parameter);

			if (replacement == null)
			{
				continue;
			}

			Parameters[i] = replacement;
		}

		// If any of the parameters is unresolved, then this type can not be resolved yet
		if (Parameters.Any(i => i.IsUnresolved))
		{
			return null;
		}

		// Return if the type is not yet declared
		if (!context.IsTypeDeclared(Name))
		{
			return null;
		}

		var type = context.GetType(Name)!;

		if (!Parameters.Any())
		{
			return new TypeNode(type);
		}

		if (type is TemplateType template)
		{
			return new TypeNode(template.GetVariant(Parameters));
		}

		return null;
	}

	public Type? TryResolveType(Context context)
	{
		return Resolve(context)?.GetType();
	}

	public Status GetStatus()
	{
		var template_parameters = string.Join(", ", Parameters.Select(i => i.IsUnresolved ? "?" : i.ToString()));

		var descriptor = Name + (string.IsNullOrEmpty(template_parameters) ? string.Empty : $"<{template_parameters}>");

		return Status.Error($"Could not resolve type '{descriptor}'");
	}
}