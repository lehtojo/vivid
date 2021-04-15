using System;
using System.Collections.Generic;
using System.Linq;

public class Table
{
	public string Name { get; private set; }
	public Label Start { get; private set; }
	public bool IsBuilt { get; set; } = false;
	public bool IsSection { get; set; } = false;

	public List<object> Items { get; private set; } = new List<object>();
	public int Subtables { get; private set; } = 0;

	public Table(string name)
	{
		Name = name;
		Start = new Label(name);
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
	public const string CONFIGURATION_VARIABLE = ".configuration";
	public const string REFERENCE_COUNT_VARIABLE = ".references";

	public const string ZERO_TERMINATOR = "\\x00";
	public const string INHERITANT_SEPARATOR = "\\x01";
	public const string FULLNAME_END = "\\x02";

	public Table Entry { get; private set; }
	public Table Descriptor { get; private set; }

	public Variable Variable { get; private set; }
	public Variable? References { get; private set; }

	public bool IsCompleted { get; set; } = false;

	private string GetFullname(Type type, bool start = false)
	{
		var a = type.Name;
		var b = INHERITANT_SEPARATOR;

		b += string.Join(string.Empty, type.Supertypes.Select(i => GetFullname(i)).ToArray());

		if (start)
		{
			a += ZERO_TERMINATOR;
			b += FULLNAME_END;
		}

		return a + b;
	}

	public RuntimeConfiguration(Type type)
	{
		Variable = type.Declare(Link.GetVariant(Types.U64), VariableCategory.MEMBER, CONFIGURATION_VARIABLE);
		
		if (Analysis.IsGarbageCollectorEnabled)
		{
			References = type.Declare(Types.LINK, VariableCategory.MEMBER, REFERENCE_COUNT_VARIABLE);
		}

		Entry = new Table(type.GetFullname() + Mangle.CONFIGURATION_COMMAND + Mangle.END_COMMAND);
		Descriptor = new Table(type.GetFullname() + Mangle.DESCRIPTOR_COMMAND + Mangle.END_COMMAND);

		Entry.Add(Descriptor);

		Descriptor.Add(GetFullname(type, true), false);
	}
}

public class Type : Context
{
	public const string INDEXED_ACCESSOR_SETTER_IDENTIFIER = "set";
	public const string INDEXED_ACCESSOR_GETTER_IDENTIFIER = "get";

	public static readonly Dictionary<Operator, string> OPERATOR_OVERLOAD_FUNCTIONS = new();

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
	public Position? Position { get; set; }

	public bool IsStatic => Flag.Has(Modifiers, Modifier.STATIC);
	public bool IsImported => Flag.Has(Modifiers, Modifier.IMPORTED);

	public bool IsUnresolved => !IsResolved();

	public bool IsUserDefined => Destructors.Overloads.Any();
	public bool IsGenericType => !Flag.Has(Modifiers, Modifier.TEMPLATE_TYPE);
	public bool IsTemplateType => Flag.Has(Modifiers, Modifier.TEMPLATE_TYPE);
	public bool IsTemplateTypeVariant => Name.IndexOf('<') != -1;

	public Format Format => GetFormat();
	public int ReferenceSize => GetReferenceSize();
	public int ContentSize => GetContentSize();

	public RuntimeConfiguration? Configuration { get; set; }

	public List<Type> Supertypes { get; } = new List<Type>();
	public Type[] TemplateArguments { get; set; } = Array.Empty<Type>();
	public Dictionary<string, FunctionList> Virtuals { get; } = new Dictionary<string, FunctionList>();
	public FunctionList Constructors { get; } = new FunctionList();
	public FunctionList Destructors { get; } = new FunctionList();

	public OperatorNode[] Initialization { get; set; } = Array.Empty<OperatorNode>();

	/// <summary>
	/// Adds the specified constructor to this type
	/// </summary>
	public void AddConstructor(Constructor constructor)
	{
		if (!Constructors.Overloads.Any() || !((Constructor)Constructors.Overloads.First()).IsDefault)
		{
			Constructors.Add(constructor);
			Declare(constructor);
			return;
		}

		// Remove the default constructor
		Functions[Keywords.INIT.Identifier].Overloads.Clear();
		Constructors.Overloads.Clear();

		Constructors.Add(constructor);

		Declare(constructor);
	}

	/// <summary>
	/// Adds the specified destructor to this type
	/// </summary>
	public void AddDestructor(Destructor destructor)
	{
		if (!IsUserDefined || !((Destructor)Destructors.Overloads.First()).IsDefault)
		{
			Destructors.Add(destructor);
			Declare(destructor);
			return;
		}

		// Remove the default destructor
		Functions[Keywords.DEINIT.Identifier].Overloads.Clear();
		Destructors.Overloads.Clear();

		Destructors.Add(destructor);

		Declare(destructor);
	}
	
	/// <summary>
	/// Returns all the constructors
	/// </summary>
	public FunctionList GetConstructors()
	{
		return Constructors;
	}

	/// <summary>
	/// Returns all the destructors
	/// </summary>
	public FunctionList GetDestructors()
	{
		return Destructors;
	}

	public Type(Context context, string name, int modifiers, Position position) : base(name)
	{
		Name = name;
		Identifier = Name;
		Modifiers = modifiers;
		Position = position;
		Supertypes = new List<Type>();

		AddConstructor(Constructor.Empty(this, position, position));
		AddDestructor(Destructor.Empty(this, position, position));

		Link(context);
		context.Declare(this);
	}

	public Type(Context context, string name, int modifiers) : base(name)
	{
		Name = name;
		Identifier = Name;
		Modifiers = modifiers;
		Supertypes = new List<Type>();

		Link(context);
		context.Declare(this);
	}

	public Type(string name, int modifiers) : base(name)
	{
		Name = name;
		Identifier = Name;
		Modifiers = modifiers;
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

	/// <summary>
	/// Returns whether this type inherits the specified type or if any of the supertypes inherits it
	/// </summary>
	public bool IsTypeInherited(Type type)
	{
		return Supertypes.Any(i => i == type || i.IsTypeInherited(type));
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

	public override bool IsFunctionDeclared(string name)
	{
		return base.IsFunctionDeclared(name) || IsSuperFunctionDeclared(name);
	}

	public override bool IsLocalFunctionDeclared(string name)
	{
		return Functions.ContainsKey(name) || IsSuperFunctionDeclared(name);
	}

	public override bool IsVariableDeclared(string name)
	{
		return base.IsVariableDeclared(name) || IsSuperVariableDeclared(name);
	}

	public override bool IsLocalVariableDeclared(string name)
	{
		return Variables.ContainsKey(name) || IsSuperVariableDeclared(name);
	}

	/// <summary>
	/// Returns whether the specified virtual function is declared in this type or in any of the supertypes
	/// </summary>
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

	public bool IsInheritingAllowed(Type inheritant)
	{
		// Type inheriting itself is not allowed
		if (inheritant == this)
		{
			return false;
		}

		// The inheritant should not have this type as its supertype
		var inheritant_supertypes = inheritant.GetAllSupertypes();

		if (inheritant_supertypes.Contains(this))
		{
			return false;
		}

		// Deny the inheritance if supertypes already contain the inheritant or if any supertype would be duplicated
		var inheritor_supertypes = GetAllSupertypes();

		return !inheritor_supertypes.Contains(inheritant) && !inheritant_supertypes.Any(i => inheritor_supertypes.Contains(i));
	}

	public virtual Format GetFormat()
	{
		return Size.FromBytes(ReferenceSize).ToFormat();
	}

	public bool Is(Type other)
	{
		return this == other || Supertypes.Any(i => i.Is(other));
	}

	public override void OnMangle(Mangle mangle)
	{
		mangle.Add(this);
	}

	public virtual Type? GetOffsetType()
	{
		return null;
	}

	public virtual Type Clone()
	{
		return (Type)MemberwiseClone();
	}

	public override bool Equals(object? other)
	{
		return other is Type type && Name == type.Name && Identity == type.Identity;
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Name);
		hash.Add(Identity);
		return hash.ToHashCode();
	}

	public override string ToString()
	{
		return string.Join('.', GetParentTypes().Select(i => i.Name).Concat(new[] { Name }));
	}
}