using System;
using System.Collections.Generic;
using System.Linq;

public class Lambda : Function
{
	public new const string SELF_POINTER_IDENTIFIER = "lambda";

	public Lambda(Context context, int modifiers, string name, List<Token> blueprint, Position? start, Position? end) : base(context, modifiers, name, blueprint, start, end)
	{
		// Lambdas usually capture variables from the parent context
		Connect(context ?? throw new ApplicationException("Tried to define a short function outside a context"));
	}

	/// <summary>
	/// Implements the lambda using the specified parameter types
	/// </summary>
	public override FunctionImplementation Implement(IEnumerable<Type> types)
	{
		if (Implementations.Any())
		{
			throw new ApplicationException("Tried to implement a lambda twice");
		}

		// Pack parameters with names and types
		var parameters = Parameters.Zip(types, (a, b) => new Parameter(a.Name, a.Position, b)).ToList();

		// Create a function implementation
		var implementation = new LambdaImplementation(this, parameters, null, Parent ?? throw new ApplicationException("Missing function parent"));

		// Force the return type, if user added it
		implementation.ReturnType = ReturnType;

		// Add the created implementation to the implementations list
		Implementations.Add(implementation);

		implementation.Implement(Blueprint);

		return implementation;
	}
}