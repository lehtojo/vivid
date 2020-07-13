using System;
using System.Collections.Generic;
using System.Linq;

public class Parameter
{
	public string Name { get; set; }
	public Type? Type { get; set; }

	public Parameter(string name, Type? type = Types.UNKNOWN)
	{
		Name = name;
		Type = type;
	}
}

public class Function : Context
{
	public const string THIS_POINTER_IDENTIFIER = "this";

	public int Modifiers { get; }

	public List<string> Parameters { get; set; } = new List<string>();
	public List<Token> Blueprint { get; }

	public List<FunctionImplementation> Implementations { get; } = new List<FunctionImplementation>();

	public bool IsConstructor => this is Constructor;
	public bool IsImported => Flag.Has(Modifiers, AccessModifier.EXTERNAL);
	public bool IsExported => Flag.Has(Modifiers, AccessModifier.GLOBAL);
	public bool IsResponsible => Flag.Has(Modifiers, AccessModifier.RESPONSIBLE);

	/// <summary>
	/// Creates a unimplemented function
	/// </summary>
	/// <param name="context">Context to link into</param>
	/// <param name="modifiers">Function access modifiers</param>
	/// <param name="name">Function name</param>
	/// <param name="blueprint">Function blueprint is used to create implementations of this function</param>
	public Function(Context context, int modifiers, string name, List<Token> blueprint)
	{
		Parent = context;
		Name = name;
		Prefix = "Function";
		Modifiers = modifiers;
		Blueprint = blueprint;
	}

	/// <summary>
	/// Creates a function with default implementation using the parameters and the return type
	/// </summary>
	/// <param name="modifiers">Function access modifiers</param>
	/// <param name="name">Function name</param>
	/// <param name="result">Function return type</param>
	/// <param name="parameters">Function parameters</param>
	public Function(int modifiers, string name, Type? result, params Parameter[] parameters)
	{
		Modifiers = modifiers;
		Name = name;
		Prefix = "Function";
		Parameters = parameters.Select(p => p.Name).ToList();
		Blueprint = new List<Token>();

		var implementation = new FunctionImplementation();
		implementation.SetParameters(parameters.ToList());
		implementation.ReturnType = result;
		implementation.Metadata = this;

		Implementations.Add(implementation);

		implementation.Implement(Blueprint);
	}

	/// <summary>
	/// Implements the function with parameter types
	/// </summary>
	/// <param name="types">Parameter types</param>
	/// <returns>Function implementation</returns>
	public FunctionImplementation Implement(IEnumerable<Type> types)
	{
		// Pack parameters with names and types
		var parameters = Parameters.Zip(types, (name, type) => new Parameter(name, type)).ToList();

		// Create a function implementation
		var implementation = new FunctionImplementation(Parent);
		implementation.SetParameters(parameters);
		implementation.Metadata = this;

		// Constructors must be set to return a link to the created object manually
		if (IsConstructor)
		{
			implementation.ReturnType = global::Types.LINK;
		}

		// Add the created implementation to the list
		Implementations.Add(implementation);

		implementation.Implement(Blueprint);

		return implementation;
	}

	/// </summary>
	/// Returns whether there are enough parameters to call this function
	/// </summary>
	public virtual bool Passes(List<Type> parameters)
	{
		return parameters.Count == Parameters.Count;
	}

	/// <summary>
	/// Tries to find function implementation with the given parameter
	/// </summary>
	/// <param name="parameter">Parameter type used in filtering</param>
	public FunctionImplementation? Get(Type parameter)
	{
		return Get(new List<Type> { parameter });
	}

	/// <summary>
	/// Tries to find function implementation with the given parameters
	/// </summary>
	/// <param name="parameter">Parameter types used in filtering</param>
	public virtual FunctionImplementation? Get(List<Type> parameters)
	{
		var implementation = Implementations.Find(f => f.ParameterTypes.SequenceEqual(parameters));

		if (implementation != null || IsImported)
		{
			return implementation;
		}
		
		return Parameters.Count != parameters.Count ? null : Implement(parameters);
	}

	public override string GetFullname()
	{
		return IsImported ? Name : base.GetFullname();
	}

	public override string ToString()
	{
		return (IsImported ? "import" : string.Empty) + Name + $"({string.Join(", ", Parameters)})";
	}

	public override bool Equals(object? obj)
	{
		return obj is Function function &&
			   EqualityComparer<List<Context>>.Default.Equals(Subcontexts, function.Subcontexts) &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, function.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, function.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, function.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, function.Labels) &&
			   Modifiers == function.Modifiers &&
			   EqualityComparer<List<string>>.Default.Equals(Parameters, function.Parameters) &&
			   EqualityComparer<List<FunctionImplementation>>.Default.Equals(Implementations, function.Implementations);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(Subcontexts);
		hash.Add(Variables);
		hash.Add(Functions);
		hash.Add(Types);
		hash.Add(Labels);
		hash.Add(Modifiers);
		hash.Add(Parameters);
		hash.Add(Implementations);
		return hash.ToHashCode();
	}
}