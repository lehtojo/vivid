using System;
using System.Collections.Generic;
using System.Linq;

public class Lambda : Function
{
	public new const string SELF_POINTER_IDENTIFIER = "lambda";

	public LambdaImplementation Implementation => (LambdaImplementation)Implementations.First();

	public Lambda(Context context, int modifiers, string name, List<Token> blueprint, Position? start, Position? end) : base(context, modifiers, name, blueprint, start, end)
	{
		// Lambdas usually capture variables from the parent context
		Link(context ?? throw new ApplicationException("Tried to define a short function outside a context"));

		context.Declare(this);
	}

	/// <summary>
	/// Implements the lambda with parameter types
	/// </summary>
	/// <param name="types">Parameter types</param>
	/// <returns>Lambda implementation</returns>
	public override FunctionImplementation Implement(IEnumerable<Type> types)
	{
		if (Implementations.Any())
		{
			throw new ApplicationException("Tried to implement a short function twice");
		}

		// Pack parameters with names and types
		var parameters = Parameters.Zip(types, (a, b) => new Parameter(a.Name, a.Position, b)).ToList();

		// Create a function implementation
		var implementation = new LambdaImplementation(this, parameters, null, Parent ?? throw new ApplicationException("Missing function parent"));

		// Add the created implementation to the list
		Implementations.Add(implementation);

		implementation.Implement(Blueprint);

		return implementation;
	}

	public List<Variable> GetCapturedVariables()
	{
		return Implementation.Node!
		   .FindAll(n => n.Is(NodeType.VARIABLE))
		   .Select(v => v.To<VariableNode>().Variable)
		   .Where(v => !v.Context.IsInside(this))
		   .ToList();
	}
}