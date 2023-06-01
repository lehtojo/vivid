using System.Collections.Generic;
using System.Linq;

public class ContextTypeRecovery
{
	public Context? Parent { get; }
	public List<Type> Imports { get; }
	public List<Type> Supertypes { get; }

	private static bool IsTemplateTypeBase(Type type)
	{
		return !type.IsPrimitive && type.IsTemplateType && !type.IsTemplateTypeVariant;
	}

	public ContextTypeRecovery(Type from)
	{
		Parent = from.Parent;
		Imports = from.Imports.ToList();
		Supertypes = from.Supertypes.ToList();
	}

	public void Recover(Type to)
	{
		to.Parent = Parent;

		to.Imports.Clear();
		to.Imports.AddRange(Imports);

		to.Supertypes.Clear();
		to.Supertypes.AddRange(Supertypes);

		if (IsTemplateTypeBase(to) && Parent != null)
		{
			foreach (var variant in to.To<TemplateType>().Variants.Values)
			{
				if (variant.Type.Parent == Parent) continue;

				Parent.Types[variant.Type.Name] = variant.Type;
				variant.Type.Parent = Parent;
			}
		}
	}
}

public class ContextFunctionRecovery
{
	public Context? Parent { get; }
	public List<Parameter> Parameters { get; }
	public Type? ReturnType { get; }
	public Dictionary<string, Function>? Variants { get; set; } = null;

	private static bool IsTemplateFunctionBase(Function function)
	{
		return function.IsTemplateFunction && !function.IsTemplateFunctionVariant;
	}

	public ContextFunctionRecovery(Function from)
	{
		Parent = from.Parent;
		Parameters = from.Parameters.Select(i => new Parameter(i.Name, i.Position, i.Type)).ToList();
		ReturnType = from.ReturnType;
	}

	public void Recover(Function to)
	{
		to.Parent = Parent;

		to.Parameters.Clear();
		to.Parameters.AddRange(Parameters);

		to.ReturnType = ReturnType;

		if (IsTemplateFunctionBase(to) && Parent != null)
		{
			foreach (var variant in to.To<TemplateFunction>().Variants.Values)
			{
				if (variant.Parent == Parent) continue;

				if (Parent.Functions.TryGetValue(variant.Name, out var overloads))
				{
					var conflict = overloads.TryAdd(variant);

					// Todo: Maybe we should create a member function for this
					if (conflict != null)
					{
						overloads.Overloads.Remove(conflict);
						overloads.Overloads.Add(variant);
					}
				}
				else
				{
					overloads = new FunctionList();
					overloads.Add(variant);

					Parent.Functions.Add(variant.Name, overloads);
				}

				variant.Parent = Parent;
			}
		}
	}
}

public class ContextVariableRecovery
{
	public Context Parent { get; }
	public Type? Type { get; }

	public ContextVariableRecovery(Variable from)
	{
		Parent = from.Parent;
		Type = from.Type;
	}

	public void Recover(Variable to)
	{
		to.Parent = Parent;
		to.Type = Type;
	}
}

public class ContextRecovery
{
	public Dictionary<string, ContextTypeRecovery> Types { get; } = new();
	public Dictionary<string, ContextFunctionRecovery> Functions { get; } = new();
	public Dictionary<string, ContextVariableRecovery> Variables { get; } = new();

	public ContextRecovery(Context context)
	{
		var types = Common.GetAllTypes(context);
		var functions = Common.GetAllVisibleFunctions(context);
		var variables = Common.GetAllVariables(context);

		foreach (var type in types)
		{
			Types.TryAdd(type.Identity, new ContextTypeRecovery(type));
		}

		foreach (var function in functions)
		{
			Functions.TryAdd(function.Identity, new ContextFunctionRecovery(function));
		}

		foreach (var variable in variables)
		{
			Variables.TryAdd(variable.Parent!.Identity + '.' + variable.Name, new ContextVariableRecovery(variable));
		}
	}

	private void CleanContext(SourceFile file, Context context)
	{
		var function_remove_list = new List<string>();

		foreach (var iterator in context.Functions)
		{
			var function = iterator.Value;

			// Remove all function overloads that created in another file
			for (var i = function.Overloads.Count - 1; i >= 0; i--)
			{
				var position = function.Overloads[i].Start;
				if (position?.File == null || position.File.Fullname == file.Fullname) continue;

				function.Overloads.RemoveAt(i);
			}

			if (function.Overloads.Any()) continue;

			// Remove the function from the context, since it has no overloads in the specified file
			function_remove_list.Add(iterator.Key);
		}

		foreach (var key in function_remove_list)
		{
			context.Functions.Remove(key);
		}

		// Clean subcontexts as well
		foreach (var subcontext in context.Subcontexts)
		{
			CleanContext(file, subcontext);
		}
	}

	public void Recover(SourceFile file, Context context)
	{
		CleanContext(file, context);

		var types = Common.GetAllTypes(context);
		var functions = Common.GetAllVisibleFunctions(context);
		var variables = Common.GetAllVariables(context);

		foreach (var type in types)
		{
			if (Types.TryGetValue(type.Identity, out var recovery))
			{
				recovery.Recover(type);
			}
		}

		foreach (var function in functions)
		{
			if (Functions.TryGetValue(function.Identity, out var recovery))
			{
				recovery.Recover(function);
			}
		}

		foreach (var variable in variables)
		{
			if (Variables.TryGetValue(variable.Parent!.Identity + '.' + variable.Name, out var recovery))
			{
				recovery.Recover(variable);
			}
		}
	}
}