using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

public class Table
{
	public string Name { get; private set; }
	public bool IsBuilt { get; set; } = false;

	public List<object> Items { get; private set; } = new List<object>();
	public int Subtables { get; private set; } = 0;

	public Table(string name)
	{
		Name = name;
	}

	public void Add(object item, bool inline = true)
	{
		if (inline)
		{
			Items.Add(item);
			return;
		}

		var subtable = new Table(Name + $"_{Subtables++}");
		subtable.Add(item);

		Items.Add(subtable);
	}
}

public class RuntimeConfiguration
{
	public const string CONFIGURATION_TABLE_POSTFIX = "_configuration";
	public const string DESCRIPTOR_TABLE_POSTFIX = "_descriptor";

	public Table Entry { get; private set; }
	public Table Descriptor { get; private set; }

	public Variable Variable { get; private set; }

	public RuntimeConfiguration(Type type)
	{
		Variable = type.DeclareHidden(Types.LINK, VariableCategory.MEMBER);

		Entry = new Table(type.GetFullname() + CONFIGURATION_TABLE_POSTFIX);
		Descriptor = new Table(type.GetFullname() + DESCRIPTOR_TABLE_POSTFIX);

		Entry.Add(Descriptor);
		Descriptor.Add(type.Name, false);
	}
}

public class Type : Context
{
	public const string INDEXED_ACCESSOR_SETTER_IDENTIFIER = "set";
	public const string INDEXED_ACCESSOR_GETTER_IDENTIFIER = "get";

	public static readonly Dictionary<Operator, string> OPERATOR_OVERLOAD_FUNCTIONS = new Dictionary<Operator, string>();

	static Type()
	{
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.ADD, "plus");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.SUBTRACT, "minus");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.MULTIPLY, "times");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.DIVIDE, "divide");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.MODULUS, "remainder");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.ASSIGN_ADD, "assign_plus");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.ASSIGN_SUBTRACT, "assign_minus");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.ASSIGN_MULTIPLY, "assign_times");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.ASSIGN_DIVIDE, "assign_divide");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.ASSIGN_MODULUS, "assign_remainder");
		OPERATOR_OVERLOAD_FUNCTIONS.Add(Operators.EQUALS, "equals");
	}

	public int Modifiers { get; set; }

	public bool IsUnresolved => !IsResolved();
	public bool IsTemplateType => Flag.Has(Modifiers, AccessModifier.TEMPLATE_TYPE);

	public Format Format => GetFormat();
	public int ReferenceSize => GetReferenceSize();
	public int ContentSize => GetContentSize();

	public RuntimeConfiguration? Configuration { get; private set; }

	public List<Type> Supertypes { get; } = new List<Type>();
	public Dictionary<string, FunctionList> Virtuals { get; } = new Dictionary<string, FunctionList>();
	public FunctionList Constructors { get; } = new FunctionList();
	public FunctionList Destructors { get; } = new FunctionList();

	public OperatorNode[] Initialization { get; set; } = Array.Empty<OperatorNode>();

	public Action<Mangle> OnAddDefinition { get; set; }

	public void AddConstructor(Constructor constructor)
	{
		if (!Constructors.Overloads.Any() || !((Constructor)Constructors.Overloads.First()).IsDefault)
		{
			Constructors.Add(constructor);
			return;
		}

		Constructors.Overloads.Remove(Constructors.Overloads.First());
		Constructors.Add(constructor);
	}

	public void AddDestructor(Function destructor)
	{
		Destructors.Add(destructor);
	}

	public FunctionList GetConstructors()
	{
		return Constructors;
	}

	public FunctionList GetDestructors()
	{
		return Destructors;
	}

	public Type(Context context, string name, int modifiers) : this(context, name, modifiers, new List<Type>()) { }

	public Type(Context context, string name, int modifiers, List<Type> supertypes)
	{
		Name = name;
		Identifier = Name;
		Prefix = $"N{Name.Length}";
		Modifiers = modifiers;
		Supertypes = supertypes;
		OnAddDefinition = OnAddDefaultDefinition;

		Constructors.Add(Constructor.Empty(this));

		Link(context);
		context.Declare(this);
	}

	public Type(string name, int modifiers)
	{
		Name = name;
		Identifier = Name;
		Prefix = $"N{Name.Length}";
		Modifiers = modifiers;
		OnAddDefinition = OnAddDefaultDefinition;

		Constructors.Add(Constructor.Empty(this));
	}

	public Type(Context context)
	{
		Prefix = "N";
		Identifier = Name;
		OnAddDefinition = OnAddDefaultDefinition;

		Link(context);
		Constructors.Add(Constructor.Empty(this));
	}

	public void AddRuntimeConfiguration()
	{
		if (Configuration != null)
		{
			return;
		}

		Configuration = new RuntimeConfiguration(this);
	}

	public virtual bool IsResolved()
	{
		return true;
	}

	public virtual int GetReferenceSize()
	{
		return Parser.Bytes;
	}

	public virtual int GetContentSize()
	{
		var local_content_size = Variables
			.Where(v => !v.Value.IsStatic)
			.Sum(v => v.Value.Type?.ReferenceSize ?? throw new ApplicationException("Tried to get reference size of a unresolved member"));

		return Supertypes.Sum(s => s.ContentSize) + local_content_size;
	}

	public FunctionList GetOperatorFunction(Operator operation)
	{
		return GetFunction(OPERATOR_OVERLOAD_FUNCTIONS[operation]) ?? throw new InvalidOperationException($"Could not find operator function '{OPERATOR_OVERLOAD_FUNCTIONS[operation]}'");
	}

	public bool IsOperatorOverloaded(Operator operation)
	{
		return OPERATOR_OVERLOAD_FUNCTIONS.TryGetValue(operation, out var name) && (IsLocalFunctionDeclared(name) || IsSuperFunctionDeclared(name));
	}

	public int? GetSupertypeBaseOffset(Type type)
	{
		var position = 0;

		if (type == this)
		{
			return position;
		}

		foreach (var supertype in Supertypes)
		{
			if (supertype == type)
			{
				return position;
			}

			var local_base_offset = supertype.GetSupertypeBaseOffset(type);

			if (local_base_offset != null)
			{
				return position + local_base_offset;
			}

			position += supertype.ContentSize;
		}

		return null;
	}

	public bool IsTypeInherited(Type type)
	{
		return Supertypes.Any(s => s == type || s.IsTypeInherited(type));
	}

	public bool IsSuperFunctionDeclared(string name)
	{
		return Supertypes.Any(t => t.IsLocalFunctionDeclared(name));
	}

	public bool IsSuperVariableDeclared(string name)
	{
		return Supertypes.Any(t => t.IsLocalVariableDeclared(name));
	}

	public bool IsSuperTypeDeclared(Type supertype)
	{
		return Supertypes.Contains(supertype) || Supertypes.Any(t => t.IsSuperTypeDeclared(supertype));
	}

	public FunctionList? GetSuperFunction(string name)
	{
		return Supertypes.First(t => t.IsLocalFunctionDeclared(name)).GetFunction(name);
	}

	public Variable? GetSuperVariable(string name)
	{
		return Supertypes.First(t => t.IsLocalVariableDeclared(name)).GetVariable(name);
	}

	public override bool IsLocalFunctionDeclared(string name)
	{
		return base.IsLocalFunctionDeclared(name) || IsSuperFunctionDeclared(name);
	}

	public override bool IsLocalVariableDeclared(string name)
	{
		return base.IsLocalVariableDeclared(name) || IsSuperVariableDeclared(name);
	}

	public override bool IsFunctionDeclared(string name)
	{
		return base.IsFunctionDeclared(name) || IsSuperFunctionDeclared(name);
	}

	public override bool IsVariableDeclared(string name)
	{
		return base.IsVariableDeclared(name) || IsSuperVariableDeclared(name);
	}

	public bool IsVirtualFunctionDeclared(string name)
	{
		return Virtuals.ContainsKey(name) || Supertypes.Any(i => i.IsVirtualFunctionDeclared(name));
	}

	public override FunctionList? GetFunction(string name)
	{
		if (base.IsLocalFunctionDeclared(name))
		{
			return base.GetFunction(name);
		}

		return IsSuperFunctionDeclared(name) ? GetSuperFunction(name) : base.GetFunction(name);
	}

	/// <summary>
	/// Retrieves the virtual function list which corresponds the specified name
	/// </summary>
	public FunctionList? GetVirtualFunction(string name)
	{
		return Virtuals.ContainsKey(name) ? Virtuals[name] : Supertypes.Select(i => i.GetVirtualFunction(name)).FirstOrDefault(i => i != null);
	}

	/// <summary>
	/// Returns all virtual function declarations contained in this type and its supertypes
	/// </summary>
	public List<VirtualFunction> GetAllVirtualFunctions()
	{
		return Virtuals.Values.SelectMany(i => i.Overloads).Cast<VirtualFunction>().Concat(Supertypes.SelectMany(i => i.GetAllVirtualFunctions())).ToList();
	}

	/// <summary>
	/// Returns all supertypes this type inherits
	/// </summary>
	public List<Type> GetAllSupertypes()
	{
		return Supertypes.Concat(Supertypes.SelectMany(i => i.GetAllSupertypes())).ToList();
	}

	/// <summary>
	/// Finds the first configuration variable in the hierarchy of this type
	/// </summary>
	public Variable GetConfigurationVariable()
	{
		if (Supertypes.Any())
		{
			var supertype = Supertypes.First();

			while (supertype.Supertypes.Any())
			{
				supertype = supertype.Supertypes.First();
			}

			return supertype.Configuration?.Variable ?? throw new ApplicationException("Could not get runtime configuration from an inherited supertype");
		}

		return Configuration?.Variable ?? throw new ApplicationException("Could not get runtime configuration");
	}

	public override Variable? GetVariable(string name)
	{
		if (base.IsLocalVariableDeclared(name))
		{
			return base.GetVariable(name);
		}

		return IsSuperVariableDeclared(name) ? GetSuperVariable(name) : base.GetVariable(name);
	}

	/// <summary>
	/// Declares a virtual function into the context
	/// </summary>
	/// <param name="function">Function to declare</param>
	public void Declare(VirtualFunction function)
	{
		FunctionList? entry;

		if (Virtuals.ContainsKey(function.Name))
		{
			entry = GetVirtualFunction(function.Name);

			if (entry == null)
			{
				throw new ApplicationException("Could not retrieve a virtual function list");
			}
		}
		else
		{
			if (Supertypes.Any(i => i.IsVirtualFunctionDeclared(function.Name)))
			{
				throw new InvalidOperationException("Tried to declare a virtual function with a name which was taken by one of supertypes");
			}

			Virtuals.Add(function.Name, (entry = new FunctionList()));
		}

		function.Ordinal = Virtuals.Values.Sum(i => i.Overloads.Count);

		entry.Add(function);
	}

	public override IEnumerable<FunctionImplementation> GetImplementedFunctions()
	{
		// Take all the standard member functions and also the constructors and destructors
		return base.GetImplementedFunctions()
			.Concat(Constructors.Overloads.Concat(Destructors.Overloads)
			.SelectMany(f => f.Implementations)
			.Where(i => i.Node != null));
	}

	public override IEnumerable<FunctionImplementation> GetFunctionImplementations()
	{
		// Take all the standard member functions and also the constructors and destructors
		return base.GetImplementedFunctions()
			.Concat(Constructors.Overloads.Concat(Destructors.Overloads)
			.SelectMany(f => f.Implementations));
	}

	public virtual Format GetFormat()
	{
		return Size.FromBytes(ReferenceSize).ToFormat();
	}

	public bool Is(Type other)
	{
		return this == other || Supertypes.Any(i => i.Is(other));
	}

	public T To<T>() where T : Type
	{
		return (T)this ?? throw new ApplicationException($"Could not convert 'Type' to '{typeof(T).Name}'");
	}

	protected override void OnMangle(Mangle mangle)
	{
		mangle += 'N';
		mangle.Add(this);
	}

	private Type[] GetTemplateArguments(string name)
	{
		if (!(GetType(name) is TemplateType template_type))
		{
			throw new ApplicationException("The base class of a template type variant was not a template type");
		}

		return template_type.GetVariantArguments(this) ?? throw new ApplicationException("Could not retrieve the template arguments of a template type variant");
	}

	/// <summary>
	/// Appends a definition of this type to the specified mangled identifier using the default method
	/// </summary>
	private void OnAddDefaultDefinition(Mangle mangle)
	{
		// Check if the name represents a template type
		if (Name.EndsWith('>'))
		{
			// Determine the start of template arguments, meaning the text inside the brackets of the following example: Dictionary[<string, int>]
			var template_arguments_start = Name.IndexOf('<');

			// Get the template argument types
			var name = Name[0..template_arguments_start];
			var template_arguments = GetTemplateArguments(name);

			mangle += name.Length.ToString(CultureInfo.InvariantCulture) + name;
			mangle += 'I';
			mangle += template_arguments;
			mangle += 'E';
		}
		else
		{
			mangle += Name.Length.ToString(CultureInfo.InvariantCulture) + Name;
		}
	}

	/// <summary>
	/// Appends a definition of this type to the specified mangled identifier
	/// </summary>
	public virtual void AddDefinition(Mangle mangle)
	{
		OnAddDefinition(mangle);
	}

	public override bool Equals(object? other)
	{
		return other is Type type &&
			   Name == type.Name &&
			   EqualityComparer<List<Context>>.Default.Equals(Subcontexts, type.Subcontexts) &&
			   IsFunction == type.IsFunction &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, type.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, type.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, type.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, type.Labels) &&
			   Modifiers == type.Modifiers &&
			   IsUnresolved == type.IsUnresolved &&
			   Format == type.Format &&
			   EqualityComparer<List<Type>>.Default.Equals(Supertypes, type.Supertypes) &&
			   EqualityComparer<FunctionList>.Default.Equals(Constructors, type.Constructors) &&
			   EqualityComparer<FunctionList>.Default.Equals(Destructors, type.Destructors);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(Name);
		hash.Add(Subcontexts);
		hash.Add(IsFunction);
		hash.Add(Variables);
		hash.Add(Functions);
		hash.Add(Types);
		hash.Add(Labels);
		hash.Add(Modifiers);
		hash.Add(IsUnresolved);
		hash.Add(Format);
		hash.Add(Supertypes);
		hash.Add(Constructors);
		hash.Add(Destructors);
		return hash.ToHashCode();
	}

	public override string ToString()
	{
		return Name;
	}
}