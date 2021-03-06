using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class FunctionImplementation : Context
{
	public Function Metadata { get; set; }

	public VirtualFunction? VirtualFunction { get; set; }
	public Variable? Self { get; protected set; }

	public Type[] TemplateArguments { get; set; }

	public List<Variable> Parameters => Variables.Values
		.Where(i => !i.IsSelfPointer && i.Category == VariableCategory.PARAMETER)
		.ToList();

	public List<Type> ParameterTypes => Parameters
		.Where(i => !i.IsSelfPointer)
		.Select(i => i.Type!)
		.ToList();

	public int SizeOfLocals { get; set; } = 0;
	public int SizeOfLocalMemory { get; set; } = 0;

	public Node? Node { get; set; }

	public List<Node> References { get; } = new List<Node>();

	public Type? ReturnType { get; set; }

	public bool IsInlined { get; set; } = false;
	public bool IsEmpty => (Node == null || Node.First == null) && !Metadata.IsImported;

	public bool IsVirtual => Metadata is VirtualFunction;
	public bool IsConstructor => Metadata is Constructor;
	public bool IsStatic => Flag.Has(Metadata!.Modifiers, Modifier.STATIC);

	public override void OnMangle(Mangle mangle)
	{
		if (IsMember)
		{
			mangle += Mangle.START_LOCATION_COMMAND;
			mangle.Path(GetParentTypes());
		}

		mangle += Identifier.Length.ToString(CultureInfo.InvariantCulture) + Identifier;

		if (TemplateArguments.Any())
		{
			mangle += Mangle.START_TEMPLATE_ARGUMENTS_COMMAND;
			mangle += TemplateArguments;
			mangle += Mangle.END_COMMAND;
		}

		if (IsMember)
		{
			mangle += Mangle.END_COMMAND;
		}

		mangle += Parameters.Select(i => i.Type!);

		if (!Parameters.Any())
		{
			mangle += Mangle.NO_PARAMETERS_COMMAND;
		}

		if (!Primitives.IsPrimitive(ReturnType, Primitives.UNIT))
		{
			mangle += Mangle.PARAMETERS_END;
			mangle += Mangle.START_RETURN_TYPE_COMMAND;
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
		Name = Metadata.Name;
		Identifier = Name;

		Link(context);

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
			var parameter = new Variable(this, properties.Type, VariableCategory.PARAMETER, properties.Name, Modifier.DEFAULT, false)
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
		if (Metadata.IsMember && !Metadata.IsStatic)
		{
			Self = new Variable(this, Metadata.GetTypeParent(), VariableCategory.PARAMETER, Function.SELF_POINTER_IDENTIFIER, Modifier.DEFAULT)
			{
				IsSelfPointer = true,
				Position = Metadata.Start
			};
		}

		Node = new ScopeNode(this, Metadata.Start, Metadata.End);
		Parser.Parse(Node, this, blueprint, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
	}

	/// <summary>
	/// Returns the header of the function.
	/// </summary>
	public virtual string GetHeader()
	{
		var a = Parent != null && Parent.IsType ? Parent.ToString() : string.Empty;
		var b = $"{Metadata.Name}({string.Join(", ", Parameters.Select(i => i.ToString()).ToArray())}): " + (ReturnType == null ? "?" : ReturnType.ToString());

		return string.IsNullOrEmpty(a) ? b : a + '.' + b;
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
		/// NOTE: If a scope node has a parent, it must be a statement
		if (Node.Find(i => i.Is(NodeType.RETURN) && i.FindParent(i => i.Is(NodeType.SCOPE) && i.Parent != null) != null) != null)
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
			   EqualityComparer<int>.Default.Equals(References.Count, implementation.References.Count) &&
			   EqualityComparer<Type?>.Default.Equals(ReturnType, implementation.ReturnType);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Name);
		hash.Add(Parameters);
		hash.Add(ParameterTypes);
		hash.Add(ReturnType);
		return hash.ToHashCode();
	}
}

