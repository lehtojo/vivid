using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionList
{
	public List<Function> Overloads { get; } = new List<Function>();

	/// <summary>
	/// Adds the specified function to the overloads.
	/// This function throws an exception on fail.
	/// </summary>
	public void Add(Function function)
	{
		var conflict = TryAdd(function);

		if (conflict == null)
		{
			return;
		}

		if (conflict.Start != null)
		{
			throw Errors.Get(function.Start, $"Function overload can be confused with another function overload at {Errors.FormatPosition(conflict.Start)}");
		}
		else
		{
			throw Errors.Get(function.Start, $"Function overload can be confused with another function overload");
		}
	}

	/// <summary>
	/// Tries to add the specified function to the overloads.
	/// This function returns the conflicting overload on fail, otherwise null.
	/// </summary>
	public Function? TryAdd(Function function)
	{
		// Conflicts can only happen with functions which are similar kind (either a template function or a standard function) and have the same amount of parameters
		var is_template_function = function is TemplateFunction;

		var count = function.Parameters.Count;
		var conflicts = Overloads.Where(i => i.Parameters.Count == count && i is TemplateFunction == is_template_function);

		foreach (var conflict in conflicts)
		{
			var pass = false;

			for (var i = 0; i < count; i++)
			{
				var x = function.Parameters[i].Type;
				var y = conflict.Parameters[i].Type;

				if (x == null || y == null || x == y)
				{
					continue;
				}

				pass = true;
				break;
			}

			if (!pass)
			{
				return conflict;
			}
		}

		Overloads.Add(function);
		return null;
	}

	public override bool Equals(object? other)
	{
		return other is FunctionList list && EqualityComparer<List<Function>>.Default.Equals(Overloads, list.Overloads);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Overloads);
	}

	private static int GetCastCount(Function candidate, List<Type> parameters)
	{
		var casts = 0;

		for (var i = 0; i < parameters.Count; i++)
		{
			if (candidate.Parameters[i].Type == null || candidate.Parameters[i].Type!.Equals(parameters[i]))
			{
				continue;
			}

			casts++;
		}

		return casts;
	}

	public Function? GetOverload(List<Type> parameters, Type[] template_arguments)
	{
		if (template_arguments.Any())
		{
			var candidates = Overloads.FindAll(i => i is TemplateFunction function && function.TemplateArgumentNames.Count == template_arguments.Length && function.Passes(parameters, template_arguments)).Cast<TemplateFunction>().ToList();

			if (candidates.Count <= 1)
			{
				return candidates.FirstOrDefault();
			}

			return candidates.OrderBy(i => GetCastCount(i, parameters)).First();
		}
		else
		{
			var candidates = Overloads.FindAll(i => i is not TemplateFunction && i.Passes(parameters));

			if (candidates.Count <= 1)
			{
				return candidates.FirstOrDefault();
			}

			return candidates.OrderBy(i => GetCastCount(i, parameters)).First();
		}
	}

	public Function? GetOverload(params Type[] parameters)
	{
		return GetOverload(parameters.ToList(), Array.Empty<Type>());
	}

	public Function? GetOverload(List<Type> parameters)
	{
		return GetOverload(parameters, Array.Empty<Type>());
	}

	public FunctionImplementation? GetImplementation(List<Type> parameters, Type[] template_arguments)
	{
		if (template_arguments.Any())
		{
			var candidates = Overloads.FindAll(i => i is TemplateFunction function && function.TemplateArgumentNames.Count == template_arguments.Length && function.Passes(parameters, template_arguments)).Cast<TemplateFunction>().ToList();

			if (candidates.Count <= 1)
			{
				return candidates.FirstOrDefault()?.Get(parameters, template_arguments);
			}

			return candidates.OrderBy(i => GetCastCount(i, parameters)).First().Get(parameters, template_arguments);
		}
		else
		{
			var candidates = Overloads.FindAll(i => !(i is TemplateFunction) && i.Passes(parameters));

			if (candidates.Count <= 1)
			{
				return candidates.FirstOrDefault()?.Get(parameters);
			}

			return candidates.OrderBy(i => GetCastCount(i, parameters)).First().Get(parameters);
		}
	}

	public FunctionImplementation? GetImplementation(params Type[] parameters)
	{
		return GetImplementation(parameters.ToList(), Array.Empty<Type>());
	}

	public FunctionImplementation? GetImplementation(List<Type> parameters)
	{
		return GetImplementation(parameters, Array.Empty<Type>());
	}
}
