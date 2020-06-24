using System;
using System.Collections.Generic;
using System.Linq;

public class Context
{
	public string Name { get; protected set; } = string.Empty;
	public string Prefix { get; protected set; } = string.Empty;
	public string Postfix { get; protected set; } = string.Empty;

	public Context? Parent { get; set; }
	public List<Context> Subcontexts { get; private set; } = new List<Context>();

	public bool IsGlobal => GetTypeParent() == null;
	public bool IsMember => !IsGlobal;
	public bool IsType => this is Type;
	public bool IsFunction => this is FunctionImplementation;
	public bool IsInsideFunction => IsFunction || GetFunctionParent() != null;
	public bool IsInsideType => IsType || GetTypeParent() != null;

	public Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();
	public Dictionary<string, FunctionList> Functions { get; } = new Dictionary<string, FunctionList>();
	public Dictionary<string, Type> Types { get; } = new Dictionary<string, Type>();
	public Dictionary<string, Label> Labels { get; } = new Dictionary<string, Label>();

	/// <summary>
	/// Returns whether the given context is a parent context or higher to this context
	/// </summary>
	public bool IsInside(Context context)
	{
		return Equals(context, this) || (Parent?.IsInside(context) ?? false);
	}

	/// <summary>
	/// Returns the global name of the context
	/// </summary>
	/// <returns>Global name of the context</returns>
	public virtual string GetFullname()
	{
		var parent = Parent?.GetFullname() ?? string.Empty;
		parent = string.IsNullOrEmpty(parent) ? parent : parent + '_';

		var prefix = Prefix.ToLower();
		prefix = string.IsNullOrEmpty(prefix) ? string.Empty : prefix;

		var name = Name.ToLower();
		name = string.IsNullOrEmpty(name) ? name : '_' + name;

		var postfix = Postfix.ToLower();
		postfix = string.IsNullOrEmpty(postfix) ? postfix : '_' + postfix;

		return parent + prefix + name + postfix;
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

	public virtual Label? GetLabel(string name)
	{
		if (Labels.ContainsKey(name))
		{
			return Labels[name];
		}
		else if (Parent != null)
		{
			return Parent.GetLabel(name);
		}
		else
		{
			return null;
		}
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