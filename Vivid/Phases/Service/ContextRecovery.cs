using System.Collections.Generic;
using System.Linq;

public class ContextTypeRecovery
{
	public List<Type> Imports { get; }

	public ContextTypeRecovery(Type from)
	{
		Imports = from.Imports.ToList();
	}

	public void Recover(Type to)
	{
		to.Imports.Clear();
		to.Imports.AddRange(Imports);
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
			Variables.TryAdd(variable.Context!.Identity + '.' + variable.Name, new ContextVariableRecovery(variable));
		}
	}

	public void Recover(Context context)
	{
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
			if (Variables.TryGetValue(variable.Context!.Identity + '.' + variable.Name, out var recovery))
			{
				recovery.Recover(variable);
			}
		}
	}
}