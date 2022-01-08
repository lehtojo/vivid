using System;
using System.Collections.Generic;
using System.Linq;

[Flags]
public enum TableMarker
{
	None = 0,
	TextualAssembly = 1,
	DataEncoder = 2
}

public class Table
{
	public string Name { get; private set; }
	public Label Start { get; private set; }

	// Summary:
	// This is used to determine whether the table has been processed.
	// Bool is not enough, because there can be multiple runs and we do not want to reset all the tables before each run.
	public TableMarker Marker { get; set; } = TableMarker.None;

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
	public const string FULLNAME_END = "\\x01";

	public Table Entry { get; private set; }
	public Table Descriptor { get; private set; }

	public Variable Variable { get; private set; }
	public Variable? References { get; private set; }

	public bool IsCompleted { get; set; } = false;

	private static string GetFullname(Type type)
	{
		return type.Name + ZERO_TERMINATOR + string.Join(ZERO_TERMINATOR, type.GetAllSupertypes().Select(i => i.Name)) + FULLNAME_END;
	}

	public RuntimeConfiguration(Type type)
	{
		Variable = type.Declare(new Link(Primitives.CreateNumber(Primitives.U64, Format.UINT64)), VariableCategory.MEMBER, CONFIGURATION_VARIABLE);
		
		if (Analysis.IsGarbageCollectorEnabled)
		{
			References = type.Declare(new Link(), VariableCategory.MEMBER, REFERENCE_COUNT_VARIABLE);
		}

		Entry = new Table(type.GetFullname() + Mangle.CONFIGURATION_COMMAND + Mangle.END_COMMAND);
		Descriptor = new Table(type.GetFullname() + Mangle.DESCRIPTOR_COMMAND + Mangle.END_COMMAND);

		Entry.Add(Descriptor);
		Descriptor.Add(GetFullname(type), false);
	}
}

public class Type : Context
{
	public const string INDEXED_ACCESSOR_SETTER_IDENTIFIER = "set";
	public const string INDEXED_ACCESSOR_GETTER_IDENTIFIER = "get";

	public static readonly Dictionary<Operator, string> OPERATOR_OVERLOADS = new();

	static Type()
	{
		OPERATOR_OVERLOADS.Add(Operators.ADD, "plus");
		OPERATOR_OVERLOADS.Add(Operators.SUBTRACT, "minus");
		OPERATOR_OVERLOADS.Add(Operators.MULTIPLY, "times");
		OPERATOR_OVERLOADS.Add(Operators.DIVIDE, "divide");
		OPERATOR_OVERLOADS.Add(Operators.MODULUS, "remainder");
		OPERATOR_OVERLOADS.Add(Operators.ASSIGN_ADD, "assign_plus");
		OPERATOR_OVERLOADS.Add(Operators.ASSIGN_SUBTRACT, "assign_minus");
		OPERATOR_OVERLOADS.Add(Operators.ASSIGN_MULTIPLY, "assign_times");
		OPERATOR_OVERLOADS.Add(Operators.ASSIGN_DIVIDE, "assign_divide");
		OPERATOR_OVERLOADS.Add(Operators.ASSIGN_MODULUS, "assign_remainder");
		OPERATOR_OVERLOADS.Add(Operators.EQUALS, "equals");
	}

	public int Modifiers { get; set; }
	public Position? Position { get; set; }

	public bool IsInlining => Flag.Has(Modifiers, Modifier.INLINE);
	public bool IsStatic => Flag.Has(Modifiers, Modifier.STATIC);
	public bool IsImported => Flag.Has(Modifiers, Modifier.IMPORTED);
	public bool IsExported => Flag.Has(Modifiers, Modifier.EXPORTED);
	public bool IsUnresolved => !IsResolved();
	public bool IsPlain => Flag.Has(Modifiers, Modifier.PLAIN);
	public bool IsPrimitive => Flag.Has(Modifiers, Modifier.PRIMITIVE);
	public bool IsUserDefined => !IsPrimitive && Destructors.Overloads.Any();
	public bool IsGenericType => !Flag.Has(Modifiers, Modifier.TEMPLATE_TYPE);
	public bool IsTemplateType => Flag.Has(Modifiers, Modifier.TEMPLATE_TYPE);
	public bool IsPack => Flag.Has(Modifiers, Modifier.PACK);
	public bool IsUnnamedPack => IsPack && Name.IndexOf('.') != -1;
	public bool IsTemplateTypeVariant => Name.IndexOf('<') != -1;

	public Format Format => GetFormat();

	public int ReferenceSize => GetReferenceSize(); // Reference size describes how many bytes it requires to refer to an instance of this type
	public int AllocationSize => GetAllocationSize(); // Allocation size describes how many bytes this type requires from its container
	public int ContentSize => GetContentSize(); // Allocation size describes how many bytes this type contains

	public RuntimeConfiguration? Configuration { get; set; }

	public List<Type> Supertypes { get; } = new List<Type>();
	public Type[] TemplateArguments { get; set; } = Array.Empty<Type>();
	public Dictionary<string, FunctionList> Virtuals { get; } = new Dictionary<string, FunctionList>();
	public Dictionary<string, FunctionList> Overrides { get; } = new Dictionary<string, FunctionList>();
	public FunctionList Constructors { get; } = new FunctionList();
	public FunctionList Destructors { get; } = new FunctionList();

	public OperatorNode[] Initialization { get; set; } = Array.Empty<OperatorNode>();

	/// <summary>
	/// Adds the specified constructor to this type
	/// </summary>
	public void AddConstructor(Constructor constructor)
	{
		if (!Constructors.Overloads.Any() || !Constructors.Overloads.First().To<Constructor>().IsDefault)
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
		if (!IsUserDefined || !Destructors.Overloads.First().To<Destructor>().IsDefault)
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

	public Type(Context context, string name, int modifiers, Position? position) : base(name)
	{
		Name = name;
		Identifier = Name;
		Modifiers = modifiers;
		Position = position;
		Supertypes = new List<Type>();

		AddConstructor(Constructor.Empty(this, position, position));
		AddDestructor(Destructor.Empty(this, position, position));

		Connect(context);
		context.Declare(this);
	}

	public Type(Context context, string name, int modifiers) : base(name)
	{
		Name = name;
		Identifier = Name;
		Modifiers = modifiers;
		Supertypes = new List<Type>();

		Connect(context);
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
		if (Configuration != null) return;

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

	public virtual int GetAllocationSize()
	{
		if (IsPack) return Supertypes.Select(i => i.GetAllocationSize()).Sum() + Variables.Values.Where(i => !i.IsStatic && !i.IsConstant).Sum(i => i.Type!.AllocationSize);
		return GetReferenceSize();
	}

	public virtual int GetContentSize()
	{
		var bytes = 0;

		foreach (var member in Variables.Values)
		{
			if (member.IsStatic || member.IsConstant) continue;
			if (member.Type == null) throw Errors.Get(member.Position, "Missing member variable type");

			if (member.IsInlined())
			{
				bytes += member.Type.ContentSize;
			}
			else
			{
				bytes += member.Type.AllocationSize;
			}
		}

		return Supertypes.Sum(i => i.ContentSize) + bytes;
	}

	public bool IsOperatorOverloaded(Operator operation)
	{
		return OPERATOR_OVERLOADS.TryGetValue(operation, out var name) && (IsLocalFunctionDeclared(name) || IsSuperFunctionDeclared(name));
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
		return Supertypes.Any(i => i.IsLocalFunctionDeclared(name));
	}

	public bool IsSuperVariableDeclared(string name)
	{
		return Supertypes.Any(i => i.IsLocalVariableDeclared(name));
	}

	public bool IsSuperTypeDeclared(Type supertype)
	{
		return Supertypes.Contains(supertype) || Supertypes.Any(i => i.IsSuperTypeDeclared(supertype));
	}

	public FunctionList? GetSuperFunction(string name)
	{
		return Supertypes.First(i => i.IsLocalFunctionDeclared(name)).GetFunction(name);
	}

	public Variable? GetSuperVariable(string name)
	{
		return Supertypes.First(i => i.IsLocalVariableDeclared(name)).GetVariable(name);
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
		return Supertypes.SelectMany(i => i.GetAllVirtualFunctions()).Concat(Virtuals.Values.SelectMany(i => i.Overloads).Cast<VirtualFunction>()).ToList();
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

			return supertype.Configuration?.Variable ?? throw new ApplicationException("Could not find runtime configuration from an inherited supertype");
		}

		return Configuration?.Variable ?? throw new ApplicationException("Could not find runtime configuration");
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
	public void Declare(VirtualFunction function)
	{
		FunctionList? entry;

		if (Virtuals.ContainsKey(function.Name))
		{
			entry = GetVirtualFunction(function.Name);
			if (entry == null) throw new ApplicationException("Could not retrieve a virtual function list");
		}
		else
		{
			if (Supertypes.Any(i => i.IsVirtualFunctionDeclared(function.Name)))
			{
				throw new InvalidOperationException("Tried to declare a virtual function with a name which was taken by one of supertypes");
			}

			entry = new FunctionList();
			Virtuals.Add(function.Name, entry);
		}

		entry.Add(function);
	}

	/// <summary>
	/// Declares the specfied virtual function overload
	/// </summary>
	public void DeclareOverride(Function function)
	{
		FunctionList entry;

		if (Overrides.ContainsKey(function.Name))
		{
			entry = Overrides[function.Name];
		}
		else
		{
			entry = new FunctionList();
			Overrides.Add(function.Name, entry);
		}

		entry.Add(function);
	}

	/// <summary>
	/// Tries to find virtual function overrides with the specified name
	/// </summary>
	public FunctionList? GetOverride(string name)
	{
		if (Overrides.ContainsKey(name)) return Overrides[name];

		foreach (var supertype in Supertypes)
		{
			var result = supertype.GetOverride(name);
			if (result != null) return result;
		}

		return null;
	}

	public bool IsInheritingAllowed(Type inheritant)
	{
		// Type inheriting itself is not allowed
		if (inheritant == this) return false;

		// The inheritant should not have this type as its supertype
		var inheritant_supertypes = inheritant.GetAllSupertypes();
		if (inheritant_supertypes.Contains(this)) return false;

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
		return Equals(this, other) || Supertypes.Any(i => i.Is(other));
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

	private bool Equals(Type other)
	{
		if (IsPack)
		{
			// The other type should also be a pack as well
			if (!other.IsPack) return false;

			// Verify the members are compatible with each other
			return Common.Compatible(Variables.Select(i => i.Value.Type), other.Variables.Select(i => i.Value.Type));
		}

		return Name == other.Name && Identity == other.Identity;
	}

	public override bool Equals(object? other)
	{
		return other is Type type && Equals(type);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name, Identity);
	}

	public override string ToString()
	{
		// Handle unnamed packs seperately
		if (IsUnnamedPack)
		{
			// Pattern: { $member-1: $type-1, $member-2: $type-2, ... }
			return "{ " + string.Join(", ", Variables.Select(i => $"{i.Key}: {(i.Value.Type != null ? i.Value.Type.ToString() : "?")}")) + " }";
		}

		return string.Join('.', GetParentTypes().Select(i => i.Name).Concat(new[] { Name }));
	}
}