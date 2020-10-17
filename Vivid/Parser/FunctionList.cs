using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionList
{
	public List<Function> Overloads { get; } = new List<Function>();

	public void Add(Function function)
	{
		var count = function.Parameters.Count;

		if (Overloads.Exists(o => o.Parameters.Count == count))
		{
			throw new InvalidOperationException("Function overload with same amount of parameters already exists");
		}

		Overloads.Add(function);
	}

	public override bool Equals(object? obj)
	{
		return obj is FunctionList list &&
			   EqualityComparer<List<Function>>.Default.Equals(Overloads, list.Overloads);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Overloads);
	}

	public FunctionImplementation? GetImplementation(List<Type> parameters, Type[] template_arguments)
	{
		if (template_arguments.Any())
		{
			return ((TemplateFunction?)Overloads.Find(i => i is TemplateFunction function && function.TemplateArgumentNames.Count == template_arguments.Length && function.Passes(parameters, template_arguments)
			
			))?.Get(parameters, template_arguments);
		}

		return Overloads.Find(o => o.Passes(parameters))?.Get(parameters);
	}

	public FunctionImplementation? GetImplementation(List<Type> parameters)
	{
		return GetImplementation(parameters, Array.Empty<Type>());
	}
}
