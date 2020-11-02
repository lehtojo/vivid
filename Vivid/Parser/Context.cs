using System;
using System.Collections.Generic;
using System.Linq;

public class Mangle
{
	public const string LANGUAGE_TAG = "_V";

	private class Definition
	{
		public Type Type { get; }
		public int Index { get; }
		public int Pointers { get; }

		private string? Hexadecimal { get; set; }

		public Definition(Type type, int index, int pointers)
		{
			Type = type;
			Index = index;
			Pointers = pointers;
		}

		private const string Table = "0123456789ABCDEF";

		public override string ToString()
		{
			if (Hexadecimal == null)
			{
				var n = Index - 1;

				Hexadecimal = n == 0 ? "0" : string.Empty;

				while (n > 0)
				{
					var a = n / 16;
					var r = n - a * 16;
					n = a;

					Hexadecimal = Table[r] + Hexadecimal;
				}
			}

			return Index == 0 ? "S_" : $"S{Hexadecimal}_";
		}
	}

	private List<Definition> Definitions { get; set; } = new List<Definition>();
	public string Value { get; set; } = string.Empty;

	public Mangle(Mangle? from)
	{
		if (from != null)
		{
			Definitions = new List<Definition>(from.Definitions);
			Value = from.Value;
		}
		else
		{
			Value = LANGUAGE_TAG;
		}
	}

	public static Mangle operator +(Mangle mangle, string text)
	{
		mangle.Value += text;
		return mangle;
	}

	public static Mangle operator +(Mangle mangle, char character)
	{
		mangle.Value += character;
		return mangle;
	}

	public static Mangle operator +(Mangle mangle, IEnumerable<Type> types)
	{
		mangle.Add(types);
		return mangle;
	}

	public static Mangle operator +(Mangle mangle, Type type)
	{
		mangle.Add(new List<Type> { type });
		return mangle;
	}

	private void Push(Definition last, int delta)
	{
		for (var i = 0; i < delta; i++)
		{
			Definitions.Add(new Definition(last.Type, Definitions.Count, last.Pointers + i + 1));
			Value += 'P';
		}

		Value += last.ToString();
	}

	public void Add(Type type, int pointers = 0)
	{
		if (pointers == 0 && Types.IsPrimitive(type))
		{
			type.AddDefinition(this);
			return;
		}

		var i = -1;

		for (var j = 0; j < Definitions.Count; j++)
		{
			var t = Definitions[j];

			if (t.Type == type && t.Pointers <= pointers)
			{
				i = j;
			}
		}

		if (i == -1)
		{
			for (var j = 0; j < pointers; j++)
			{
				Value += 'P';
			}

			type.AddDefinition(this);

			if (!Types.IsPrimitive(type) && type != Types.LINK)
			{
				Definitions.Add(new Definition(type, Definitions.Count, 0));
			}

			for (var j = 0; j < pointers; j++)
			{
				Definitions.Add(new Definition(type, Definitions.Count, j + 1));
			}

			return;
		}

		var d = pointers - i;

		if (d <= 0)
		{
			Value += Definitions[i].ToString();
			return;
		}

		Push(Definitions[i], d);
	}

	public void Add(IEnumerable<Type> types)
	{
		foreach (var type in types)
		{
			Add(type, Types.IsPrimitive(type) ? 0 : 1);
		}
	}
}

public class Context
{
	private Mangle? Mangled { get; set; }

	public string Identifier { get; set; } = string.Empty;
	public string Name { get; protected set; } = string.Empty;

	public string Prefix { get; set; } = string.Empty;
	public string Postfix { get; set; } = string.Empty;

	public Context? Parent { get; set; }
	public List<Context> Subcontexts { get; private set; } = new List<Context>();

	public bool IsGlobal => GetTypeParent() == null;
	public bool IsMember => GetTypeParent() != null;
	public bool IsLambda => this is LambdaImplementation;
	public bool IsType => this is Type;
	public bool IsFunction => this is FunctionImplementation;

	public bool IsInsideFunction => IsFunction || GetFunctionParent() != null;
	public bool IsInsideType => IsType || GetTypeParent() != null;

	public Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();
	public Dictionary<string, FunctionList> Functions { get; } = new Dictionary<string, FunctionList>();
	public Dictionary<string, Type> Types { get; } = new Dictionary<string, Type>();
	public Dictionary<string, Label> Labels { get; } = new Dictionary<string, Label>();

	private int Lambdas { get; set; } = 0;
	private int Hiddens { get; set; } = 0;
	private int Sections { get; set; } = 0;

	/// <summary>
	/// Returns whether the given context is a parent context or higher to this context
	/// </summary>
	public bool IsInside(Context context)
	{
		return Equals(context, this) || (Parent?.IsInside(context) ?? false);
	}

	/// <summary>
	/// Appends the current context to the specified mangled name
	/// </summary>
	protected virtual void OnMangle(Mangle mangle) { }

	/// <summary>
	/// Generates a mangled name for this context
	/// </summary>
	private void Mangle()
	{
		if (Mangled != null)
		{
			return;
		}

		Parent?.Mangle();
		Mangled = new Mangle(Parent?.Mangled);

		OnMangle(Mangled);
	}

	/// <summary>
	/// Returns a mangled name corresponding this context
	/// </summary>
	public string GetFullname()
	{
		Mangle();

		return Mangled!.Value;
	}

	/// <summary>
	/// Updates types, function and variables when new context is linked
	/// </summary>
	public void Update()
	{
		foreach (var variable in Variables.Values)
		{
			if (variable.IsUnresolved)
			{
				var resolvable = (IResolvable?)variable.Type;

				if (resolvable != null)
				{
					// Try to solve the type
					var type = (TypeNode?)resolvable.Resolve(this);
					variable.Type = type?.Type;
				}
			}
		}

		foreach (var type in Types.Values)
		{
			type.Update();
		}

		foreach (var subcontext in Subcontexts)
		{
			subcontext.Update();
		}
	}

	/// <summary>
	/// Links this context with the given context, allowing access to the information of the given context
	/// </summary>
	/// <param name="context">Context to link with</param>
	public void Link(Context context)
	{
		Parent = context;
		Parent.Subcontexts.Add(this);
		Update();
	}

	/// <summary>
	/// Moves all types, functions and variables from the given context to this context, and then destroyes the given context
	/// </summary>
	public void Merge(Context context)
	{
		foreach (var (key, value) in context.Types)
		{
			Types.TryAdd(key, value);
		}

		foreach (var (key, value) in context.Functions)
		{
			Functions.TryAdd(key, value);
		}

		foreach (var (key, value) in context.Variables)
		{
			Variables.TryAdd(key, value);
		}

		foreach (var type in context.Types.Values)
		{
			type.Parent = this;
		}

		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				overload.Parent = this;

				foreach (var implementation in overload.Implementations)
				{
					implementation.Parent = this;
				}
			}
		}

		foreach (var variable in context.Variables.Values)
		{
			variable.Context = this;
		}

		Update();

		context.Destroy();
	}

	/// <summary>
	/// Declares a type into the context
	/// </summary>
	/// <param name="type">Type to declare</param>
	public void Declare(Type type)
	{
		if (IsLocalTypeDeclared(type.Name))
		{
			throw new Exception($"Type '{type.Name}' already exists in this context");
		}

		Types.Add(type.Name, type);
	}

	/// <summary>
	/// Declares a function into the context
	/// </summary>
	/// <param name="function">Function to declare</param>
	public void Declare(Function function)
	{
		FunctionList entry;

		if (IsLocalFunctionDeclared(function.Name))
		{
			entry = Functions[function.Name];
		}
		else
		{
			Functions.Add(function.Name, (entry = new FunctionList()));
		}

		entry.Add(function);
	}

	/// <summary>
	/// Declares a variable into the context
	/// </summary>
	/// <param name="variable">Variable to declare</param>
	public void Declare(Variable variable)
	{
		if (IsLocalVariableDeclared(variable.Name))
		{
			throw new Exception($"Variable '{variable.Name}' already exists in this context");
		}

		// Update variable context
		variable.Context = this;

		// Add variable to the list
		Variables.Add(variable.Name, variable);
	}

	/// <summary>
	/// Declares a variable into the context
	/// </summary>
	public void Declare(Type? type, VariableCategory category, string name)
	{
		if (IsLocalVariableDeclared(name))
		{
			throw new Exception($"Variable '{name}' already exists in this context");
		}

		// When a variable is created this way it is automatically declared into this context
		Variable.Create(this, type, category, name, AccessModifier.PUBLIC);
	}

	/// <summary>
	/// Declares a hidden variable with the specified type
	/// </summary>
	public Variable DeclareHidden(Type? type, VariableCategory category = VariableCategory.LOCAL)
	{
		return Variable.Create(this, type, category, $".{Hiddens++}_{Guid.NewGuid()}", AccessModifier.PUBLIC);
	}

	/// <summary>
	/// Declares a label into the context
	/// </summary>
	/// <param name="label">Label to declare</param>
	public void Declare(Label label)
	{
		if (IsLocalLabelDeclared(label.GetName()))
		{
			throw new Exception($"Label '{label.GetName()}' already exists in this context");
		}

		Labels.Add(label.GetName(), label);
	}

	/// <summary>
	/// Declares an already existing type with different name
	/// </summary>
	public void DeclareTypeAlias(string alias, Type type)
	{
		if (IsLocalTypeDeclared(alias))
		{
			throw new Exception($"Tried to declare type alias '{alias}' but the name was already reserved");
		}

		Types.Add(alias, type);
	}

	public virtual bool IsLocalTypeDeclared(string name)
	{
		return Types.ContainsKey(name);
	}

	public virtual bool IsLocalFunctionDeclared(string name)
	{
		return Functions.ContainsKey(name);
	}

	public virtual bool IsLocalVariableDeclared(string name)
	{
		return Variables.ContainsKey(name);
	}

	public virtual bool IsLocalLabelDeclared(string name)
	{
		return Labels.ContainsKey(name);
	}

	public virtual bool IsVariableDeclared(string name)
	{
		return Variables.ContainsKey(name) || (Parent != null && Parent.IsVariableDeclared(name));
	}

	public virtual bool IsTypeDeclared(string name)
	{
		return Types.ContainsKey(name) || (Parent != null && Parent.IsTypeDeclared(name));
	}

	public virtual bool IsTemplateTypeDeclared(string name)
	{
		return IsTypeDeclared(name) && GetType(name)!.IsTemplateType;
	}

	public virtual bool IsFunctionDeclared(string name)
	{
		return Functions.ContainsKey(name) || (Parent != null && Parent.IsFunctionDeclared(name));
	}

	public virtual bool IsLabelDeclared(string name)
	{
		return Labels.ContainsKey(name) || (Parent != null && Parent.IsLabelDeclared(name));
	}

	public Type? GetType(string name)
	{
		if (Types.ContainsKey(name))
		{
			return Types[name];
		}
		else if (Parent != null)
		{
			return Parent.GetType(name);
		}
		else
		{
			return null;
		}
	}

	public virtual FunctionList? GetFunction(string name)
	{
		if (Functions.ContainsKey(name))
		{
			return Functions[name];
		}
		else if (Parent != null)
		{
			return Parent.GetFunction(name);
		}
		else
		{
			return null;
		}
	}

	public virtual Variable? GetVariable(string name)
	{
		if (Variables.ContainsKey(name))
		{
			return Variables[name];
		}
		else if (Parent != null)
		{
			return Parent.GetVariable(name);
		}
		else
		{
			return null;
		}
	}

	public virtual Label GetLabel()
	{
		return new Label(GetFullname() + "_I" + Sections++);
	}

	public virtual Variable? GetSelfPointer()
	{
		return Parent?.GetSelfPointer();
	}

	public Type? GetTypeParent()
	{
		if (IsType)
		{
			return (Type)this;
		}

		return Parent?.GetTypeParent();
	}

	public FunctionImplementation? GetFunctionParent()
	{
		if (IsFunction)
		{
			return (FunctionImplementation)this;
		}

		return Parent?.GetFunctionParent();
	}

	public virtual IEnumerable<FunctionImplementation> GetImplementedFunctions()
	{
		return Functions.Values
			.SelectMany(f => f.Overloads)
			.SelectMany(f => f.Implementations)
			.Where(i => i.Node != null);
	}

	public virtual IEnumerable<FunctionImplementation> GetFunctionImplementations()
	{
		return Functions.Values
			.SelectMany(f => f.Overloads)
			.SelectMany(f => f.Implementations);
	}

	public int GetNextLambda()
	{
		return Lambdas++;
	}

	public void Destroy()
	{
		Parent?.Subcontexts.Remove(this);
		Parent = null;
	}

	public override bool Equals(object? obj)
	{
		return obj is Context context &&
			   Name == context.Name &&
			   EqualityComparer<List<Context>>.Default.Equals(Subcontexts, context.Subcontexts) &&
			   IsType == context.IsType &&
			   IsFunction == context.IsFunction &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, context.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, context.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, context.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, context.Labels);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(Name);
		hash.Add(Subcontexts);
		hash.Add(IsType);
		hash.Add(IsFunction);
		hash.Add(Variables);
		hash.Add(Functions);
		hash.Add(Types);
		hash.Add(Labels);
		return hash.ToHashCode();
	}
}