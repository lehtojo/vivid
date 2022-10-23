using System.Collections.Generic;
using System.Linq;

public class ContextTypeRecovery
{
	public List<Type> Imports { get; }
	public List<Type> Supertypes { get; }

	public ContextTypeRecovery(Type from)
	{
		Imports = from.Imports.ToList();
		Supertypes = from.Supertypes.ToList();
	}

	public void Recover(Type to)
	{
		to.Imports.Clear();
		to.Imports.AddRange(Imports);

		to.Supertypes.Clear();
		to.Supertypes.AddRange(Supertypes);
	}
}

public class ContextFunctionRecovery
{
	public List<Parameter> Parameters { get; }

	public ContextFunctionRecovery(Function from)
	{
		Parameters = from.Parameters.Select(i => new Parameter(i.Name, i.Position, i.Type)).ToList();
	}

	public void Recover(Function to)
	{
		to.Parameters.Clear();
		to.Parameters.AddRange(Parameters);
	}
}

public class ContextVariableRecovery
{
	public Type? Type { get; }

	public ContextVariableRecovery(Variable from)
	{
		Type = from.Type;
	}

	public void Recover(Variable to)
	{
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

	/// <summary>
	/// Removes all function overloads from the specified context that are not created in the specified file
	/// </summary>
	private void RemoveExternalFunctions(SourceFile file, Context context)
	{
		var external = new List<string>();

		foreach (var iterator in context.Functions)
		{
			var function = iterator.Value;

			// Remove all function overloads that created in another file
			for (var i = function.Overloads.Count - 1; i >= 0; i--)
			{
				var position = function.Overloads[i].Start;
				if (position == null || position.File == null || position.File.Fullname == file.Fullname) continue;

				function.Overloads.RemoveAt(i);
			}

			if (function.Overloads.Any()) continue;

			// Remove the function from the context, since it has no overloads in the specified file
			external.Add(iterator.Key);
		}

		foreach (var key in external)
		{
			context.Functions.Remove(key);
		}

		// Remove external functions in the subcontexts as well
		foreach (var subcontext in context.Subcontexts)
		{
			RemoveExternalFunctions(file, subcontext);
		}
	}

	public void Recover(SourceFile file, Context context)
	{
		RemoveExternalFunctions(file, context);

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