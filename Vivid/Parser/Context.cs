using System.Collections.Generic;
using System.Linq;
using System;

public class MangleDefinition
{
	public Type? Type { get; set; }
	public int Index { get; }
	public int Pointers { get; }

	private string? Hexadecimal { get; set; }

	public MangleDefinition(Type? type, int index, int pointers)
	{
		Type = type;
		Index = index;
		Pointers = pointers;
	}

	private const string TABLE = "0123456789ABCDEF";

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

				Hexadecimal = TABLE[r] + Hexadecimal;
			}
		}

		return Index == 0 ? "S_" : $"S{Hexadecimal}_";
	}
}

public class Mangle
{
	public const string EXPORT_TYPE_TAG = "_T";
	public const string VIVID_LANGUAGE_TAG = "_V";
	public const string CPP_LANGUAGE_TAG = "_Z";

	public const char START_LOCATION_COMMAND = 'N';

	public const char TYPE_COMMAND = 'N';
	public const char START_TEMPLATE_ARGUMENTS_COMMAND = 'I';
	public const char STACK_REFERENCE_COMMAND = 'S';
	public const char STACK_REFERENCE_END = '_';
	public const char END_COMMAND = 'E';
	public const char POINTER_COMMAND = 'P';
	public const char PARAMETERS_END = '_';
	public const char NO_PARAMETERS_COMMAND = 'v';
	public const char START_RETURN_TYPE_COMMAND = 'r';
	public const char STATIC_VARIABLE_COMMAND = 'A';

	public const char CONFIGURATION_COMMAND = 'C';
	public const char DESCRIPTOR_COMMAND = 'D';
	
	public const char START_FUNCTION_POINTER_COMMAND = 'F';
	public const char START_MEMBER_VARIABLE_COMMAND = 'V';
	public const char START_MEMBER_VIRTUAL_FUNCTION_COMMAND = 'F';

	public const char START_PACK_TYPE_COMMAND = 'U';
	public const string START_ARRAY_LENGTH_COMMAND = "_x";

	public const string VIRTUAL_FUNCTION_POSTFIX = "_v";

	private List<MangleDefinition> Definitions { get; set; } = new List<MangleDefinition>();
	public string Value { get; set; } = string.Empty;

	public Mangle(Mangle? from)
	{
		if (from != null)
		{
			Definitions = new List<MangleDefinition>(from.Definitions);
			Value = from.Value;
		}
		else
		{
			Value = VIVID_LANGUAGE_TAG;
		}
	}

	public Mangle(string value)
	{
		Value = value;
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

	private void Push(MangleDefinition last, int delta)
	{
		for (var i = 0; i < delta; i++)
		{
			Definitions.Add(new MangleDefinition(last.Type, Definitions.Count, last.Pointers + i + 1));
			Value += POINTER_COMMAND;
		}

		Value += last.ToString();
	}

	public void Path(IEnumerable<Type> path)
	{
		foreach (var type in path)
		{
			// Try to find the current type from the definitions
			var definition = Definitions.Find(i => i.Type == type);

			if (definition != null)
			{
				Value += definition.ToString();
				break;
			}

			Add(type, 0, false);
		}
	}

	/// <summary>
	/// Adds the specified type to this mangled identifier
	/// </summary>
	public void Add(Type type, int pointers = 0, bool full = true)
	{
		if (pointers == 0 && type.IsPrimitive && type.Name != Primitives.LINK && type is not ArrayType)
		{
			Value += type.Identifier;
			return;
		}

		// Try to find the specified type from the definitions
		var i = -1;

		for (var j = 0; j < Definitions.Count; j++)
		{
			var definition = Definitions[j];

			if (Equals(definition.Type, type) && definition.Pointers <= pointers)
			{
				i = j;
			}
		}

		if (i == -1)
		{
			for (var j = 0; j < pointers; j++)
			{
				Value += POINTER_COMMAND;
			}

			// Add the default definition without pointers if the type is not a primitive
			if (!type.IsPrimitive || type.Name == Primitives.LINK || type is ArrayType)
			{
				Definitions.Add(new MangleDefinition(type, Definitions.Count, 0));
			}

			if (type.IsUnnamedPack)
			{
				// Pattern: U $type-1 $type-2 ... E
				Value += START_PACK_TYPE_COMMAND;
				Add(type.Variables.Values.Select(i => i.Type!));
				Value += END_COMMAND;
				return;
			}

			if (type is FunctionType function)
			{
				Value += START_FUNCTION_POINTER_COMMAND;
				Definitions.Add(new MangleDefinition(type, Definitions.Count, 1));

				type = function.ReturnType!;

				if (Primitives.IsPrimitive(type, Primitives.UNIT))
				{
					Value += NO_PARAMETERS_COMMAND;
				}
				else
				{
					pointers = type.IsPrimitive ? 0 : 1;
					Add(type, pointers);
				}

				Add(function.Parameters!);
				Value += END_COMMAND;
				return;
			}

			if (type is Link || type is ArrayType)
			{
				Value += POINTER_COMMAND;
				
				var argument = type.GetAccessorType() ?? throw new ApplicationException("Missing offset type");
				pointers = argument.IsPrimitive ? 0 : 1;
				
				Add(argument, pointers);

				// Add the array length
				if (type is ArrayType)
				{
					Value += START_ARRAY_LENGTH_COMMAND;
					Value += type.To<ArrayType>().Size;
				}

				return;
			}

			// Append the full location of the specified type if that is allowed
			var parents = type.GetParentTypes();

			if (full && parents.Any())
			{
				Value += START_LOCATION_COMMAND;
				Path(parents);
			}

			Value += type.Identifier.Length.ToString() + type.Identifier;

			if (type.IsTemplateType)
			{
				Value += START_TEMPLATE_ARGUMENTS_COMMAND;
				Add(type.TemplateArguments);
				Value += END_COMMAND;
			}

			// End the location command if there are any parents
			if (full && parents.Any())
			{
				Value += END_COMMAND;
			}

			for (var j = 0; j < pointers; j++)
			{
				Definitions.Add(new MangleDefinition(type, Definitions.Count, j + 1));
			}

			return;
		}

		// Determine the amount of nested pointers needed for the best definition to match the specified type
		var d = pointers - Definitions[i].Pointers;

		// The difference should never be negative but it can be zero
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
			Add(type, type.IsPrimitive || type.IsPack ? 0 : 1);
		}
	}

	public Mangle Clone()
	{
		return new Mangle(this);
	}
}

public class Context : IComparable<Context>
{
	public string Identity { get; private set; }
	public string Identifier { get; set; } = string.Empty;
	public string Name { get; protected set; } = string.Empty;
	public Mangle? Mangled { get; private set; }

	public Dictionary<string, Variable> Variables { get; } = new Dictionary<string, Variable>();
	public Dictionary<string, FunctionList> Functions { get; } = new Dictionary<string, FunctionList>();
	public Dictionary<string, Type> Types { get; } = new Dictionary<string, Type>();
	public Dictionary<string, Label> Labels { get; } = new Dictionary<string, Label>();

	public Context? Parent { get; set; }
	public List<Context> Subcontexts { get; private set; } = new List<Context>();
	public List<Type> Imports { get; } = new List<Type>();

	protected Indexer Indexer { get; set; } = new Indexer();

	public bool IsGlobal => FindTypeParent() == null;
	public bool IsMember => FindTypeParent() != null;
	public bool IsType => this is Type;
	public bool IsNamespace => this is Type && To<Type>().IsStatic;
	public bool IsFunction => this is Function;
	public bool IsLambda => this is Lambda;
	public bool IsImplementation => this is FunctionImplementation;
	public bool IsLambdaImplementation => this is LambdaImplementation;
	public bool IsLambdaContainer { get; set; } = false;

	public bool IsInsideLambda => IsLambdaImplementation || IsLambda || FindImplementationParent() is LambdaImplementation || FindFunctionParent() is Lambda;
	public bool IsInsideFunction => IsImplementation || IsFunction || FindImplementationParent() != null || FindFunctionParent() != null;

	public List<Variable> Locals => Variables.Values.Where(i => i.IsLocal).Concat(Subcontexts.Where(i => !i.IsImplementation && !i.IsFunction).SelectMany(i => i.Locals)).ToList();

	public List<Variable> GetAllVariables()
	{
		var result = new List<Variable>();

		foreach (var iterator in Variables)
		{
			var variable = iterator.Value;
			if (variable.Category == VariableCategory.LOCAL && variable.Category == VariableCategory.PARAMETER) continue;
			result.Add(variable);
		}

		foreach (var subcontext in Subcontexts)
		{
			if (IsType || IsFunction || IsImplementation) continue;
			result.AddRange(subcontext.GetAllVariables());
		}

		return result;
	}

	/// <summary>
	/// Create a new root context
	/// </summary>
	public Context(string identity)
	{
		Identity = identity;
	}

	/// <summary>
	/// Create a new context and link it to the specified parent
	/// </summary>
	public Context(Context parent)
	{
		Identity = parent.Identity + '.' + parent.Indexer[Indexer.CONTEXT];
		Connect(parent);
	}

	/// <summary>
	/// Returns whether the specified context is this context or one of the parent contexts
	/// </summary>
	public bool IsInside(Context context)
	{
		return Equals(context, this) || (Parent?.IsInside(context) ?? false);
	}

	/// <summary>
	/// Returns all parent contexts which are types
	/// </summary>
	public List<Type> GetParentTypes()
	{
		var result = new List<Type>();
		var iterator = Parent;

		while (iterator != null)
		{
			if (iterator.IsType) result.Add((Type)iterator);
			iterator = iterator.Parent;
		}

		result.Reverse();
		return result;
	}

	/// <summary>
	/// Appends the current context to the specified mangled name
	/// </summary>
	public virtual void OnMangle(Mangle mangle) { }

	/// <summary>
	/// Generates a mangled name for this context
	/// </summary>
	private void Mangle()
	{
		if (Mangled != null) return;

		Mangled = new Mangle((Mangle?)null);
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
	/// Links this context with the given context, allowing access to the information of the given context
	/// </summary>
	public void Connect(Context context)
	{
		Parent = context;
		Parent.Subcontexts.Add(this);
	}

	/// <summary>
	/// Moves all types, functions and variables from the specified context to this context
	/// NOTE: This function does not copy constructors or lambdas for example since this function should be used with normal contexts
	/// </summary>
	public void Merge(Context context, bool update = true)
	{
		foreach (var (key, value) in context.Types)
		{
			Types.TryAdd(key, value);
			value.Parent = this;
		}

		foreach (var (key, value) in context.Functions)
		{
			if (Functions.ContainsKey(key))
			{
				var functions = Functions[key];

				// Try to add the overloads separately
				value.Overloads.ForEach(i => functions.TryAdd(i));
			}
			else
			{
				Functions.Add(key, new FunctionList(value.Overloads.ToList()));
			}

			value.Overloads.ForEach(i => i.Parent = this);
		}

		foreach (var (key, value) in context.Variables)
		{
			Variables.TryAdd(key, value);
			value.Parent = this;
		}

		context.Subcontexts.ForEach(i => i.Parent = this);
		Subcontexts.AddRange(context.Subcontexts.Where(i => !Subcontexts.Any(j => ReferenceEquals(i, j))).ToArray());

		// Add all imports
		// TODO: Import list should be converted into a set, because imports should not be duplicated
		Imports.AddRange(context.Imports);

		context.Destroy();
	}

	/// <summary>
	/// Declares a type into the context
	/// </summary>
	public void Declare(Type type)
	{
		if (IsLocalTypeDeclared(type.Name))
		{
			throw Errors.Get(type.Position, $"Type '{type.Name}' already exists in this context");
		}

		Types.Add(type.Name, type);
	}

	/// <summary>
	/// Declares a function into the context
	/// </summary>
	public void Declare(Function function)
	{
		FunctionList entry;

		if (Functions.ContainsKey(function.Name))
		{
			entry = Functions[function.Name];
		}
		else
		{
			entry = new FunctionList();
			Functions.Add(function.Name, entry);
		}

		entry.Add(function);
	}

	/// <summary>
	/// Declares a variable into the context
	/// </summary>
	public void Declare(Variable variable)
	{
		if (Variables.ContainsKey(variable.Name))
		{
			throw Errors.Get(variable.Position, $"Variable '{variable.Name}' already exists in this context");
		}

		variable.Parent = this;
		Variables.Add(variable.Name, variable);
	}

	/// <summary>
	/// Declares a variable into the context
	/// </summary>
	public Variable Declare(Type? type, VariableCategory category, string name)
	{
		if (Variables.ContainsKey(name))
		{
			throw Errors.Get(null, $"Variable '{name}' already exists in this context");
		}

		// When a variable is created this way it is automatically declared into this context
		return Variable.Create(this, type, category, name, Modifier.DEFAULT);
	}

	/// <summary>
	/// Declares a hidden variable with the specified type
	/// </summary>
	public Variable DeclareHidden(Type? type, VariableCategory category = VariableCategory.LOCAL)
	{
		return Variable.Create(this, type, category, $"{Identity}.{Indexer[Indexer.HIDDEN]}", Modifier.PACK);
	}

	/// <summary>
	/// Declares an unnamed pack type
	/// </summary>
	public Type DeclareUnnamedPack(Position? position)
	{
		return new Type(this, $"{Identity}.{Indexer[Indexer.HIDDEN]}", Modifier.PACK, position);
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

	/// <summary>
	/// Returns whether the specified type is declared inside this context
	/// </summary>
	public virtual bool IsLocalTypeDeclared(string name)
	{
		return Types.ContainsKey(name);
	}

	/// <summary>
	/// Returns whether the specified function is declared inside this context
	/// </summary>
	public virtual bool IsLocalFunctionDeclared(string name)
	{
		return Functions.ContainsKey(name);
	}

	/// <summary>
	/// Returns whether the specified variable is declared inside this context
	/// </summary>
	public virtual bool IsLocalVariableDeclared(string name)
	{
		return Variables.ContainsKey(name);
	}

	/// <summary>
	/// Returns whether the specified label is declared inside this context
	/// </summary>
	public virtual bool IsLocalLabelDeclared(string name)
	{
		return Labels.ContainsKey(name);
	}

	/// <summary>
	/// Returns whether the specified property is declared inside this context
	/// </summary>
	public virtual bool IsLocalPropertyDeclared(string name)
	{
		return IsLocalFunctionDeclared(name) && GetFunction(name)!.GetOverload() != null;
	}

	/// <summary>
	/// Returns whether the specified type is declared inside this context or in the parent contexts depending on the specified flag
	/// </summary>
	public virtual bool IsTypeDeclared(string name, bool local)
	{
		return local ? IsLocalTypeDeclared(name) : IsTypeDeclared(name);
	}

	/// <summary>
	/// Returns whether the specified function is declared inside this context or in the parent contexts depending on the specified flag
	/// </summary>
	public bool IsFunctionDeclared(string name, bool local)
	{
		return local ? IsLocalFunctionDeclared(name) : IsFunctionDeclared(name);
	}

	/// <summary>
	/// Returns whether the specified variable is declared inside this context or in the parent contexts depending on the specified flag
	/// </summary>
	public bool IsVariableDeclared(string name, bool local)
	{
		return local ? IsLocalVariableDeclared(name) : IsVariableDeclared(name);
	}

	/// <summary>
	/// Returns whether the specified property is declared inside this context or in the parent contexts depending on the specified flag
	/// </summary>
	public virtual bool IsPropertyDeclared(string name, bool local)
	{
		return local ? IsLocalPropertyDeclared(name) : IsPropertyDeclared(name);
	}

	/// <summary>
	/// Returns whether the specified type is declared inside this context or in the parent contexts
	/// </summary>
	public virtual bool IsTypeDeclared(string name)
	{
		return Types.ContainsKey(name) || Imports.Any(i => i.IsLocalTypeDeclared(name)) || (Parent != null && Parent.IsTypeDeclared(name));
	}

	/// <summary>
	/// Returns whether the specified function is declared inside this context or in the parent contexts
	/// </summary>
	public virtual bool IsFunctionDeclared(string name)
	{
		return Functions.ContainsKey(name) || Imports.Any(i => i.IsLocalFunctionDeclared(name)) || (Parent != null && Parent.IsFunctionDeclared(name));
	}

	/// <summary>
	/// Returns whether the specified variable is declared inside this context or in the parent contexts
	/// </summary>
	public virtual bool IsVariableDeclared(string name)
	{
		return Variables.ContainsKey(name) || Imports.Any(i => i.IsLocalVariableDeclared(name)) || (Parent != null && Parent.IsVariableDeclared(name));
	}

	/// <summary>
	/// Returns whether the specified label is declared inside this context or in the parent contexts
	/// </summary>
	public virtual bool IsLabelDeclared(string name)
	{
		return Labels.ContainsKey(name) || Imports.Any(i => i.IsLocalLabelDeclared(name)) || (Parent != null && Parent.IsLabelDeclared(name));
	}

	/// <summary>
	/// Returns whether the specified property is declared inside this context or in the parent contexts
	/// </summary>
	public virtual bool IsPropertyDeclared(string name)
	{
		return IsFunctionDeclared(name) && GetFunction(name)!.GetOverload() != null;
	}

	/// <summary>
	/// Returns the specified type by searching it from the local types, imports and parent types
	/// </summary>
	public Type? GetType(string name)
	{
		if (Types.ContainsKey(name)) return Types[name];

		// Try to find the type from imports
		foreach (var imported in Imports)
		{
			if (imported.Types.ContainsKey(name)) return imported.Types[name];
		}
		
		if (Parent != null) return Parent.GetType(name);

		return null;
	}

	/// <summary>
	/// Returns the specified function by searching it from the local types, imports and parent types
	/// </summary>
	public virtual FunctionList? GetFunction(string name)
	{
		if (Functions.ContainsKey(name)) return Functions[name];

		// Try to find the function from imports
		foreach (var imported in Imports)
		{
			if (imported.Functions.ContainsKey(name)) return imported.Functions[name];
		}

		if (Parent != null) return Parent.GetFunction(name);

		return null;
	}

	/// <summary>
	/// Returns the specified variable by searching it from the local types, imports and parent types
	/// </summary>
	public virtual Variable? GetVariable(string name)
	{
		if (Variables.ContainsKey(name)) return Variables[name];

		// Try to find the variable from imports
		foreach (var imported in Imports)
		{
			if (imported.Variables.ContainsKey(name)) return imported.Variables[name];
		}

		if (Parent != null) return Parent.GetVariable(name);

		return null;
	}

	/// <summary>
	/// Returns the specified property by searching it from the local types, imports and parent types
	/// </summary>
	public virtual Function? GetProperty(string name)
	{
		return GetFunction(name)!.GetOverload();
	}

	/// <summary>
	/// Tries to find the self pointer variable
	/// </summary>
	public virtual Variable? GetSelfPointer()
	{
		return Parent?.GetSelfPointer();
	}

	/// <summary>
	/// Tries to returns the first parent context which is a type
	/// </summary>
	public Type? FindTypeParent()
	{
		if (IsType) return (Type)this;

		return Parent?.FindTypeParent();
	}

	/// <summary>
	/// Tries to returns the first parent context which is a function
	/// </summary>
	public Function? FindFunctionParent()
	{
		if (IsFunction) return (Function)this;

		return Parent?.FindFunctionParent();
	}

	/// <summary>
	/// Tries to returns the first parent context which is a function implementation
	/// </summary>
	public FunctionImplementation? FindImplementationParent()
	{
		if (IsImplementation) return (FunctionImplementation)this;

		return Parent?.FindImplementationParent();
	}

	/// <summary>
	/// Tries to find the first parent context which can contain a lambda
	/// </summary>
	public Context? FindLambdaContainerParent()
	{
		if (IsLambdaContainer) return this;

		return Parent?.FindLambdaContainerParent();
	}

	/// <summary>
	/// Returns the root context
	/// </summary>
	public Context GetRoot()
	{
		return Parent == null ? this : Parent.GetRoot();
	}

	public virtual Label CreateLabel()
	{
		return new Label(GetFullname() + "_I" + Indexer[Indexer.SECTION]);
	}

	public string CreateStackAddress()
	{
		return Indexer.STACK.ToLowerInvariant() + '.' + Identity + '.' + Indexer[Indexer.STACK];
	}

	public int CreateLambda()
	{
		return Indexer[Indexer.LAMBDA];
	}

	public void Destroy()
	{
		Parent?.Subcontexts.Remove(this);
		Parent = null;
	}

	public T To<T>() where T : Context
	{
		return (T)this;
	}

	public override bool Equals(object? other)
	{
		return other is Context context &&
			   Name == context.Name &&
			   EqualityComparer<List<Context>>.Default.Equals(Subcontexts, context.Subcontexts) &&
			   IsType == context.IsType &&
			   IsImplementation == context.IsImplementation &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, context.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, context.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, context.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, context.Labels);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Identity);
	}

	public int CompareTo(Context? other)
	{
		return other == null ? 0 : Identity.CompareTo(other.Identity);
	}

	public override string ToString()
	{
		return string.Empty;
	}
}