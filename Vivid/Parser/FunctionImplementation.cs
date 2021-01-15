using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

public class FunctionImplementation : Context
{
	public Function Metadata { get; set; }

	public VirtualFunction? VirtualFunction { get; set; }
	public Variable? Self { get; protected set; }

	public Type[] TemplateArguments { get; set; }

	public List<Variable> Parameters => Variables.Values
		.Where(v => !v.IsSelfPointer && v.Category == VariableCategory.PARAMETER)
		.ToList();

	public List<Type> ParameterTypes => Parameters
		.Where(p => !p.IsSelfPointer)
		.Select(p => p.Type!)
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

	public bool IsVirtual => Metadata is VirtualFunction;
	public bool IsConstructor => Metadata is Constructor;
	public bool IsStatic => Flag.Has(Metadata!.Modifiers, Modifier.STATIC);

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

		if (ReturnType != global::Types.UNIT)
		{
			mangle += "_r";
			mangle += ReturnType ?? throw new ApplicationException("Return type was not resolved and it was required to be mangled");
		}
	}

	/// <summary>
	/// Optionally links this function to some context
	/// </summary>
	/// <param name="context">Context to link into</param>
	public FunctionImplementation(Function metadata, List<Parameter> parameters, Type? return_type, Context context) : base(context)
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
			var parameter = new Variable(this, properties.Type, VariableCategory.PARAMETER, properties.Name, Modifier.PUBLIC, false)
			{
				Position = properties.Position
			};

			if (Variables.ContainsKey(parameter.Name))
			{
				throw Errors.Get(parameter.Position, $"Variable '{parameter.Name}' already exists in this context");
			}

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
				Modifier.PUBLIC

			) { IsSelfPointer = true, Position = Metadata.Position };
		}

		Node = new ImplementationNode(this, Metadata.Position);
		Parser.Parse(Node, this, blueprint, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
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

	public bool IsInlineable()
	{
		if (Metadata.IsOutlined || Metadata.IsImported || Node == null)
		{
			return false;
		}

		// Try to find a return statement which is inside a statement, if at least one is found this function should not be inlined
		if (Node.Find(i => i.Is(NodeType.RETURN) && i.FindParent(i => i is IContext && !i.Is(NodeType.IMPLEMENTATION)) != null) != null)
		{
			return false;
		}

		// Try to find nested if-statements, else-if-statements, else-statements or loop-statements, if at least one is found this function should not be inlined
		if (Node.Find(i => i.Is(NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE, NodeType.LOOP) && i.FindParent(i => i.Is(NodeType.IF, NodeType.ELSE_IF, NodeType.ELSE, NodeType.LOOP)) != null) != null)
		{
			return false;
		}

		return Node.Find(i => i.Is(NodeType.FUNCTION) && i.To<FunctionNode>().Function == this) == null;
	}

	public override bool Equals(object? other)
	{
		return other is FunctionImplementation implementation &&
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
		hash.Add(Name);
		hash.Add(Parameters);
		hash.Add(ParameterTypes);
		hash.Add(ReturnType);
		return hash.ToHashCode();
	}
}

