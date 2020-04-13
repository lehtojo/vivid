using System.Collections.Generic;
using System.Linq;
using System;

public class FunctionImplementation : Context
{
	public Function? Metadata { get; set; }

	public List<Variable> Parameters => Variables.Values.Where(v => v.Category == VariableCategory.PARAMETER).ToList();
	public List<Type> ParameterTypes => Parameters.Select(p => p.Type!).ToList();
	
	public List<Variable> Locals => base.Variables.Values.Where(v => v.Category == VariableCategory.LOCAL)
										.Concat(Subcontexts.SelectMany(c => c.Variables.Values.Where(v => v.Category == VariableCategory.LOCAL))).ToList();
	public int LocalMemorySize => Variables.Values.Where(v => v.Category == VariableCategory.LOCAL).Select(v => v.Type!.ReferenceSize).Sum() +
									Subcontexts.Sum(c => c.Variables.Values.Where(v => v.Category == VariableCategory.LOCAL).Select(v => v.Type!.ReferenceSize).Sum());
	
	public Node? Node { get; set; }

	public List<Node> References { get; private set; } = new List<Node>();

	public Type? ReturnType { get; set; }
	public bool Returns => ReturnType != null;

	public bool IsInline => References.Count == 1 && false;
	public bool IsEmpty => Node == null || Node.First == null;
	
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
			var parameter = new Variable(this, properties.Type, VariableCategory.PARAMETER, properties.Name, AccessModifier.PUBLIC);
			Variables.Add(parameter.Name, parameter);
		}
	}

	/// <summary>
	/// Implements the function using the given blueprint
	/// </summary>
	/// <param name="blueprint">Tokens from which to implement the function</param>
	public void Implement(List<Token> blueprint)
	{
		Node = new ImplementationNode(this);
		Parser.Parse(Node, this, blueprint, 0, 19);
	}

	/// <summary>
	/// Returns the header of the function.
	/// Examples:
	/// Name(Type, Type, ...) [-> Result]
	/// f(number, number) -> number
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
			header = header.Substring(0, header.Length - 2);
		}

		if (ReturnType != null)
		{
			header += $") -> {ReturnType.Name}";
		}
		else
		{
			header += ')';
		}

		return header;
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
}

