using System;
using System.Collections.Generic;

public class FunctionList
{
	public List<Function> Overloads { get; private set; } = new List<Function>();

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

	public Function? this[int parameters]
	{
		get => Overloads.Find(o => o.Parameters.Count == parameters);
	}

	public FunctionImplementation? this[List<Type> parameters]
	{
		get => Overloads.Find(o => o.Parameters.Count == parameters.Count)?.Get(parameters);
	}
}
