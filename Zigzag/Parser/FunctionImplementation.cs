using System.Collections.Generic;
using System.Linq;
using System;

public enum CallingConvention
{
	CDECL,
	X64
}

public class FunctionImplementation : Context
{
	public Function? Metadata { get; set; }
	public CallingConvention Convention { get; set; } = CallingConvention.X64;

	public List<Variable> Parameters => Variables.Values.Where(v => !v.IsThisPointer && v.Category == VariableCategory.PARAMETER).ToList();
	public List<Type> ParameterTypes => Parameters.Where(p => !p.IsThisPointer).Select(p => p.Type!).ToList();
	
	public List<Variable> Locals => base.Variables.Values.Where(v => v.Category == VariableCategory.LOCAL)
										.Concat(Subcontexts.SelectMany(c => c.Variables.Values.Where(v => v.Category == VariableCategory.LOCAL))).ToList();
	public int LocalMemorySize => Variables.Values.Where(v => v.Category == VariableCategory.LOCAL).Select(v => v.Type!.ReferenceSize).Sum() +
									Subcontexts.Sum(c => c.Variables.Values.Where(v => v.Category == VariableCategory.LOCAL).Select(v => v.Type!.ReferenceSize).Sum());
	
	public Node? Node { get; set; }

	public List<Node> References { get; } = new List<Node>();

	public Type? ReturnType { get; set; }
	public bool Returns => ReturnType != null;

	public bool IsInline => References.Count == 1 && false;
	public bool IsEmpty => Node == null || Node.First == null;

	public bool IsConstructor => Metadata is Constructor;
	public bool IsResponsible => Flag.Has(Metadata!.Modifiers, AccessModifier.RESPONSIBLE);
	
	/// <summary>
	/// Optionally links this function to some context
	/// </summary>
	/// <param name="context">Context to link into</param>
	public FunctionImplementation(Context? context = null)
	{
		if (context != null)
		{
			Link(context);
		}
	}

	/// <summary>
	/// Sets the function parameters
	/// </summary>
	/// <param name="parameters">Parameters packed with name and type</param>
	public void SetParameters(List<Parameter> parameters)
	{
		foreach (var properties in parameters)
		{
			var parameter = new Variable(this, properties.Type, VariableCategory.PARAMETER, properties.Name, AccessModifier.PUBLIC, false);
			Variables.Add(parameter.Name, parameter);
		}
	}

	/// <summary>
	/// Implements the function using the given blueprint
	/// </summary>
	/// <param name="blueprint">Tokens from which to implement the function</param>
	public void Implement(List<Token> blueprint)
	{
		if (Metadata != null && Metadata.IsMember)
		{
			var type = Metadata.GetTypeParent();
			Declare(type, VariableCategory.PARAMETER, Function.THIS_POINTER_IDENTIFIER);
		}

		Node = new ImplementationNode(this);
		Parser.Parse(Node, this, blueprint, 0, 19);
	}

	/// <summary>
	/// Returns the header of the function.
	/// Examples:
	/// Name(Type, Type, ...) [: Result]
	/// f(number, number): number
	/// g(A, B) -> C
	/// h() -> A
	/// i()
	/// </summary>
	/// <returns>Header of the function</returns>
	public string GetHeader()
	{
		if (Metadata == null)
		{
			throw new ApplicationException("Couldn't get the function header since the metadata was missing");
		}

		var header = Metadata.Name + '(';

		foreach (var type in ParameterTypes)
		{
			header += $"{type.Name}, ";
		}

		if (ParameterTypes.Count > 0)
		{
			header = header[0..^2];
		}

		if (ReturnType != null)
		{
			header += $"): {ReturnType.Name}";
		}
		else
		{
			header += ')';
		}

		return header;
	}

	public override string ToString()
	{
		return GetHeader();
	}

	public override bool IsLocalVariableDeclared(string name)
	{
		return Parameters.Any(p => p.Name == name) || base.IsLocalVariableDeclared(name);
	}

	public override bool IsVariableDeclared(string name)
	{
		return Parameters.Any(p => p.Name == name) || base.IsVariableDeclared(name);
	}

	public override Variable? GetVariable(string name)
	{
		if (Parameters.Any(p => p.Name == name))
		{
			return Parameters.Find(p => p.Name == name);
		}

		return base.GetVariable(name);
	}

	public override bool Equals(object? obj)
	{
		return obj is FunctionImplementation implementation &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, implementation.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, implementation.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, implementation.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, implementation.Labels) &&
			   EqualityComparer<string?>.Default.Equals(Metadata?.Name, implementation.Metadata?.Name) &&
			   EqualityComparer<List<Variable>>.Default.Equals(Parameters, implementation.Parameters) &&
			   EqualityComparer<List<Type>>.Default.Equals(ParameterTypes, implementation.ParameterTypes) &&
			   EqualityComparer<List<Variable>>.Default.Equals(Locals, implementation.Locals) &&
			   LocalMemorySize == implementation.LocalMemorySize &&
			   EqualityComparer<int>.Default.Equals(References.Count, implementation.References.Count) &&
			   EqualityComparer<Type?>.Default.Equals(ReturnType, implementation.ReturnType);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(Subcontexts);
		hash.Add(Variables);
		hash.Add(Functions);
		hash.Add(Types);
		hash.Add(Labels);
		hash.Add(Metadata?.Name);
		hash.Add(Parameters);
		hash.Add(ParameterTypes);
		hash.Add(Locals);
		hash.Add(LocalMemorySize);
		hash.Add(References.Count);
		hash.Add(ReturnType);
		return hash.ToHashCode();
	}
}

