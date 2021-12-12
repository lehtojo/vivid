using System.Collections.Generic;
using System.Linq;
using System;

public enum FunctionLanguage
{
	OTHER,
	CPP,
	VIVID
}

public class Parameter
{
	public string Name { get; set; }
	public Position? Position { get; set; }
	public Type? Type { get; set; }
	public bool IsMemberParameter => Name.StartsWith(Function.SELF_POINTER_IDENTIFIER + '-');

	public Parameter(string name, Type? type = null)
	{
		Name = name;
		Type = type;
	}

	public Parameter(string name, Position? position, Type? type)
	{
		Name = name;
		Position = position;
		Type = type;
	}

	public string Export()
	{
		return Type == null ? Name : $"{Name} : {Type}";
	}

	public override string ToString()
	{
		if (Type == null)
		{
			return $"{Name}: any";
		}

		return $"{Name}: {(Type.IsUnresolved ? "?" : Type.ToString())}";
	}

	public override bool Equals(object? other)
	{
		return other is Parameter parameter &&
				 Name == parameter.Name &&
				 EqualityComparer<Type?>.Default.Equals(Type, parameter.Type);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Type);
	}
}

public class Function : Context
{
	public const string SELF_POINTER_IDENTIFIER = "this";

	public int Modifiers { get; set; }
	public FunctionLanguage Language { get; set; } = FunctionLanguage.VIVID;

	public Variable? Self { get; protected set; }
	public List<Parameter> Parameters { get; } = new List<Parameter>();
	public List<Token> Blueprint { get; protected set; }
	public Position? Start { get; set; }
	public Position? End { get; set; }

	public List<FunctionImplementation> Implementations { get; } = new List<FunctionImplementation>();

	public bool IsConstructor => this is Constructor;
	public bool IsPublic => Flag.Has(Modifiers, Modifier.PUBLIC);
	public bool IsProtected => Flag.Has(Modifiers, Modifier.PROTECTED);
	public bool IsPrivate => Flag.Has(Modifiers, Modifier.PRIVATE);
	public bool IsImported => Flag.Has(Modifiers, Modifier.IMPORTED);
	public bool IsExported => Flag.Has(Modifiers, Modifier.EXPORTED);
	public bool IsOutlined => Flag.Has(Modifiers, Modifier.OUTLINE);
	public bool IsStatic => Flag.Has(Modifiers, Modifier.STATIC);
	public bool IsTemplateFunction => Flag.Has(Modifiers, Modifier.TEMPLATE_FUNCTION);

	/// <summary>
	/// Creates a unimplemented function
	/// </summary>
	public Function(Context context, int modifiers, string name, List<Token> blueprint, Position? start, Position? end) : base(context)
	{
		Parent = context;
		Name = name;
		Modifiers = modifiers;
		Blueprint = blueprint;
		Start = start;
		End = end;
	}

	/// <summary>
	/// Creates a unimplemented function
	/// </summary>
	public Function(Context context, int modifiers, string name, Position? start, Position? end) : base(context)
	{
		Parent = context;
		Name = name;
		Modifiers = modifiers;
		Blueprint = new List<Token>();
		Start = start;
		End = end;
	}

	/// <summary>
	/// Creates a function with default implementation using the parameters and the return type
	/// </summary>
	public Function(Context context, int modifiers, string name, Type? return_type, params Parameter[] parameters) : base(context)
	{
		Modifiers = modifiers;
		Name = name;
		Parameters = parameters.ToList();
		Blueprint = new List<Token>();

		var implementation = new FunctionImplementation(this, parameters.ToList(), return_type, context);
		Implementations.Add(implementation);

		implementation.Implement(Blueprint);
	}

	/// <summary>
	/// Declares a self pointer inside this function
	/// </summary>
	public void DeclareSelfPointer()
	{
		Self = new Variable(this, FindTypeParent(), VariableCategory.PARAMETER, SELF_POINTER_IDENTIFIER, Modifier.DEFAULT)
		{
			IsSelfPointer = true,
			Position = Start
		};
	}

	/// <summary>
	/// Implements the function with the specified parameter type
	/// </summary>
	public FunctionImplementation Implement(Type type)
	{
		return Implement(new[] { type });
	}

	/// <summary>
	/// Implements the function with parameter types
	/// </summary>
	/// <returns>Function implementation</returns>
	public virtual FunctionImplementation Implement(IEnumerable<Type> types)
	{
		// Pack parameters with names and types
		var parameters = Parameters.Zip(types, (a, b) => new Parameter(a.Name, a.Position, b)).ToList();

		// Create a function implementation
		var implementation = new FunctionImplementation(this, parameters, null, Parent ?? throw new ApplicationException("Missing function parent"));

		// Add the created implementation to the list
		Implementations.Add(implementation);

		implementation.Implement(Blueprint.Select(i => (Token)i.Clone()).ToList());

		return implementation;
	}

	/// <summary>
	/// Returns whether the specified parameter types can implement this function
	/// </summary>
	public virtual bool Passes(List<Type> types)
	{
		if (types.Count != Parameters.Count) return false;

		for (var i = 0; i < Parameters.Count; i++)
		{
			var expected = Parameters[i].Type;
			if (expected == null) continue;

			var actual = types[i];
			if (Equals(expected, actual)) continue;
			
			if (!expected.IsPrimitive || !actual.IsPrimitive)
			{
				if (!expected.IsTypeInherited(actual) && !actual.IsTypeInherited(expected)) return false;
			}
			else if (Resolver.GetSharedType(expected, actual) == null)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Returns whether the specified parameter types can implement this function
	/// </summary>
	public virtual bool Passes(List<Type> types, Type[] arguments)
	{
		return arguments.Any() ? (IsTemplateFunction && To<TemplateFunction>().Passes(types, arguments)) : (!IsTemplateFunction && Passes(types));
	}

	/// <summary>
	/// Tries to find function implementation with the specified parameter type
	/// </summary>
	public FunctionImplementation? Get(Type type)
	{
		return Get(new[] { type });
	}

	/// <summary>
	/// Tries to find function implementation with the specified parameters
	/// </summary>
	public virtual FunctionImplementation? Get(IEnumerable<Type> parameters)
	{
		// Implementation should not be made if any of the parameters has a fixed type but it is unresolved
		if (Parameters.Any(i => i.Type != null && i.Type.IsUnresolved))
		{
			return null;
		}

		var types = Parameters.Zip(parameters).Select(i => i.First.Type ?? i.Second).ToList();
		var implementation = Implementations.Find(i => i.ParameterTypes.SequenceEqual(types));

		if (implementation != null || IsImported)
		{
			return implementation;
		}

		return Parameters.Count != parameters.Count() ? null : Implement(types);
	}

	public override Variable? GetSelfPointer()
	{
		return Self;
	}

	public override void OnMangle(Mangle mangle)
	{
		if (Language == FunctionLanguage.OTHER)
		{
			mangle.Value = Name;
			return;
		}

		if (Language == FunctionLanguage.CPP)
		{
			mangle.Value = Mangle.CPP_LANGUAGE_TAG;
		}

		if (IsMember)
		{
			mangle += Mangle.START_LOCATION_COMMAND;
			mangle.Path(GetParentTypes());
		}

		mangle += Name.Length.ToString() + Name;

		if (IsMember)
		{
			mangle += Mangle.END_COMMAND;
		}
	}

	public override string ToString()
	{
		var a = Parent != null && Parent.IsType ? Parent.ToString() : string.Empty;
		var b = $"{Name}({string.Join(", ", Parameters.Select(i => i.ToString()).ToArray())})";

		return string.IsNullOrEmpty(a) ? b : a + '.' + b;
	}

	public override bool Equals(object? other)
	{
		return other is Function function &&
			   EqualityComparer<List<Context>>.Default.Equals(Subcontexts, function.Subcontexts) &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, function.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, function.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, function.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, function.Labels) &&
			   Modifiers == function.Modifiers &&
			   EqualityComparer<List<Parameter>>.Default.Equals(Parameters, function.Parameters) &&
			   EqualityComparer<List<FunctionImplementation>>.Default.Equals(Implementations, function.Implementations);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
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