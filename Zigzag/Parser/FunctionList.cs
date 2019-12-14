using System;
using System.Collections.Generic;
using System.Text;

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

	public Function this[int parameters]
	{
		get => Overloads.Find(o => o.Parameters.Count == parameters);
	}

	public FunctionImplementation this[List<Type> parameters]
	{
		get => Overloads.Find(o => o.Parameters.Count == parameters.Count)?.Get(parameters);
	}
}
