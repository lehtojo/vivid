using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

public enum CallingConvention
{
	CDECL,
	X64
}

public class FunctionImplementation : Context
{
	public Function Metadata { get; set; }
	public CallingConvention Convention { get; set; } = CallingConvention.X64;

	public Variable? Self { get; protected set; }

	public Type[] TemplateArguments { get; set; }

	public List<Variable> Parameters => Variables.Values
		.Where(v => !v.IsSelfPointer && v.Category == VariableCategory.PARAMETER)
		.ToList();

	public List<Type> ParameterTypes => Parameters
		.Where(p => !p.IsSelfPointer)
		.Select(p => p.Type!)
		.ToList();

	public List<Variable> Locals => base.Variables.Values
		.Where(v => v.Category == VariableCategory.LOCAL)
		.Concat(Subcontexts
			.SelectMany(c => c.Variables.Values
			.Where(v => v.Category == VariableCategory.LOCAL
		)))
		.ToList();

	public int LocalMemorySize => Variables.Values
		.Where(v => v.Category == VariableCategory.LOCAL)
		.Select(v => v.Type!.ReferenceSize)
		.Sum() + Subcontexts
		.Sum(c => c.Variables.Values
			.Where(v => v.Category == VariableCategory.LOCAL)
			.Select(v => v.Type!.ReferenceSize)
			.Sum()
		);

	public Node? Node { get; set; }

	public List<Node> References { get; } = new List<Node>();

	public Type? ReturnType { get; set; }
	public bool Returns => ReturnType != null;

	public bool IsInlined { get; set; } = false;
	public bool IsEmpty => Node == null || Node.First == null;

	public bool IsConstructor => Metadata is Constructor;
	public bool IsStatic => Flag.Has(Metadata!.Modifiers, AccessModifier.STATIC);
	public bool IsResponsible => Flag.Has(Metadata!.Modifiers, AccessModifier.RESPONSIBLE);

	protected override void OnMangle(Mangle mangle)
	{
		mangle += Identifier.Length.ToString(CultureInfo.InvariantCulture) + Identifier;

		if (TemplateArguments.Any())
		{
			mangle += 'I';
			TemplateArguments.ForEach(i => mangle += i);
			mangle += 'E';
		}

		if (IsMember)
		{
			mangle += 'E';
		}

		mangle += Parameters.Select(p => p.Type!);

		if (!Parameters.Any())
		{
			mangle += 'v';
		}

		if (ReturnType != null)
		{
			mangle += "_r";
			mangle += ReturnType!;
		}
	}

	/// <summary>
	/// Optionally links this function to some context
	/// </summary>
	/// <param name="context">Context to link into</param>
	public FunctionImplementation(Function metadata, List<Parameter> parameters, Type? return_type = null, Context? context = null)
	{
		Metadata = metadata;
		ReturnType = return_type;
		TemplateArguments = Array.Empty<Type>();

		// Copy the name properties
		Prefix = Metadata.Prefix;
		Name = Metadata.Name;
		Identifier = Name;

		if (context != null)
		{
			Link(context);
		}

		SetParameters(parameters);
	}

	/// <summary>
	/// Sets the function parameters
	/// </summary>
	/// <param name="parameters">Parameters packed with name and type</param>
	private void SetParameters(List<Parameter> parameters)
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
	public virtual void Implement(List<Token> blueprint)
	{
		if (Metadata.IsMember)
		{
			Self = new Variable(
				this,
				Metadata.GetTypeParent(),
				VariableCategory.PARAMETER,
				Function.SELF_POINTER_IDENTIFIER,
				AccessModifier.PUBLIC
			);
		}

		Node = new ImplementationNode(this);
		Parser.Parse(Node, this, blueprint, Parser.MIN_PRIORITY, Parser.MEMBERS);
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
	public virtual string GetHeader()
	{
		var prefix = string.Empty;
		var iterator = Parent;

		while (iterator != null)
		{
			if (iterator is FunctionImplementation x)
			{
				prefix = x.Name + '.' + prefix;
			}
			else if (iterator is Type y)
			{
				prefix = y.Name + '.' + prefix;
			}
			else
			{
				break;
			}

			iterator = iterator.Parent;
		}

		return prefix + $"{Metadata.Name}({string.Join(", ", ParameterTypes.Select(p => p.Name).ToArray())}) => " +
			(ReturnType == null ? "_" : ReturnType.ToString());
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

	public override Variable? GetSelfPointer()
	{
		return Self;
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

