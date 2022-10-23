using System;
using System.Collections.Generic;
using System.Linq;

public class FunctionImplementation : Context
{
	public Function Metadata { get; set; }

	public VirtualFunction? VirtualFunction { get; set; }
	public Variable? Self { get; protected set; }

	public Type[] TemplateArguments { get; set; }

	public List<Variable> Parameters => Variables.Values.Where(i => !i.IsSelfPointer && !i.IsHidden && i.IsParameter).ToList();
	public List<Type> ParameterTypes => Parameters.Where(i => !i.IsSelfPointer).Select(i => i.Type!).ToList();

	public int SizeOfLocals { get; set; } = 0;
	public int SizeOfLocalMemory { get; set; } = 0;

	public Node? Node { get; set; }

	public List<Node> Usages { get; } = new List<Node>();

	public Type? ReturnType { get; set; }

	public bool IsImported { get; set; } = false;
	public bool IsSelfReturning { get; set; } = false;
	public bool IsEmpty => (Node == null || Node.First == null) && !Metadata.IsImported;

	public bool IsConstructor => Metadata is Constructor;
	public bool IsStatic => Flag.Has(Metadata!.Modifiers, Modifier.STATIC);

	public override void OnMangle(Mangle mangle)
	{
		if (Metadata.Language == FunctionLanguage.OTHER)
		{
			mangle.Value = Name;
			return;
		}

		if (Metadata.Language == FunctionLanguage.CPP)
		{
			mangle.Value = Mangle.CPP_LANGUAGE_TAG;
		}

		if (IsMember)
		{
			mangle += Mangle.START_LOCATION_COMMAND;
			mangle.Path(GetParentTypes());
		}

		mangle += Identifier.Length.ToString() + Identifier;

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

		if (Metadata.Language == FunctionLanguage.VIVID && !Primitives.IsPrimitive(ReturnType, Primitives.UNIT))
		{
			mangle += Mangle.PARAMETERS_END;
			mangle += Mangle.START_RETURN_TYPE_COMMAND;
			mangle += ReturnType ?? throw new ApplicationException("Return type was not resolved and it was required to be mangled");
		}
	}

	/// <summary>
	/// Optionally links this function to some context
	/// </summary>
	public FunctionImplementation(Function metadata, List<Parameter> parameters, Type? return_type, Context context) : base(context)
	{
		Metadata = metadata;
		ReturnType = return_type;
		TemplateArguments = Array.Empty<Type>();
		IsLambdaContainer = true;

		// Copy the name properties
		Name = Metadata.Name;
		Identifier = Name;

		Connect(context);

		SetParameters(parameters);
	}

	/// <summary>
	/// Sets the function parameters
	/// </summary>
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
	public virtual void Implement(List<Token> blueprint)
	{
		if (Metadata.IsMember && !Metadata.IsStatic)
		{
			Self = new Variable(this, Metadata.FindTypeParent(), VariableCategory.PARAMETER, Function.SELF_POINTER_IDENTIFIER, Modifier.DEFAULT)
			{
				IsSelfPointer = true,
				Position = Metadata.Start
			};
		}

		Node = new ScopeNode(this, Metadata.Start, Metadata.End, false);
		Parser.Parse(Node, this, blueprint, Parser.MIN_PRIORITY, Parser.MAX_FUNCTION_BODY_PRIORITY);
	}

	/// <summary>
	/// Returns the header of the function.
	/// </summary>
	public virtual string GetHeader()
	{
		var start = Parent != null && Parent.IsType ? Parent.ToString() + '.' : string.Empty;
		var middle = $"{Metadata.Name}({string.Join(", ", Parameters.Select(i => i.ToString()).ToArray())}): ";
		var end = ReturnType == null ? "?" : ReturnType.ToString();

		return start + middle + end;
	}

	public override string ToString()
	{
		return GetHeader();
	}

	public override Variable? GetSelfPointer()
	{
		return Self;
	}

	public override bool Equals(object? other)
	{
		return other is FunctionImplementation implementation && Identity == implementation.Identity;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Identity, Parameters, ReturnType);
	}
}