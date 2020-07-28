using System.Collections.Generic;
using System.Linq;
using System;

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
	}

	public const string IDENTIFIER_PREFIX = "type_";

	public string Identifier => IDENTIFIER_PREFIX + Name + "_";
	public int Modifiers { get; set; }

	public bool IsUnresolved => this is IResolvable;
	public bool IsTemplateType => Flag.Has(Modifiers, AccessModifier.TEMPLATE_TYPE);

	public Format Format => GetFormat();
	public int ReferenceSize => GetReferenceSize();
	public int ContentSize => GetContentSize();

	public List<Type> Supertypes { get; } = new List<Type>();
	public FunctionList Constructors { get; } = new FunctionList();
	public FunctionList Destructors { get; } = new FunctionList();

	public Node? Initialization { get; set; }

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
		Prefix = "Type";
		Modifiers = modifiers;
		Supertypes = supertypes;
		
		Constructors.Add(Constructor.Empty(this));

		Link(context);
		context.Declare(this);
	}

	public Type(string name, int modifiers)
	{
		Name = name;
		Prefix = "Type";
		Modifiers = modifiers;

		Constructors.Add(Constructor.Empty(this));
	}

	public Type(Context context)
	{
		Prefix = "Type";

		Link(context);
		Constructors.Add(Constructor.Empty(this));
	}

	public virtual int GetReferenceSize()
	{
		return Parser.Size.Bytes;
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
		return GetFunction(OPERATOR_OVERLOAD_FUNCTIONS[operation]) ?? throw new InvalidOperationException($"Couldn't find operator function '{OPERATOR_OVERLOAD_FUNCTIONS[operation]}'");
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

	public override FunctionList? GetFunction(string name)
	{
		if (base.IsLocalFunctionDeclared(name))
		{
			return base.GetFunction(name);
		}
		
		return IsSuperFunctionDeclared(name) ? GetSuperFunction(name) : base.GetFunction(name);
	}

	public override Variable? GetVariable(string name)
	{
		if (base.IsLocalVariableDeclared(name))
		{
			return base.GetVariable(name);
		}
		
		return IsSuperVariableDeclared(name) ? GetSuperVariable(name) : base.GetVariable(name);
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

	public T To<T>() where T : Type
	{
		return (T)this ?? throw new ApplicationException($"Couldn't convert 'Type' to '{typeof(T).Name}'");
	}

	public override bool Equals(object? obj)
	{
		return obj is Type type &&
			   Name == type.Name &&
			   EqualityComparer<List<Context>>.Default.Equals(Subcontexts, type.Subcontexts) &&
			   IsFunction == type.IsFunction &&
			   EqualityComparer<Dictionary<string, Variable>>.Default.Equals(Variables, type.Variables) &&
			   EqualityComparer<Dictionary<string, FunctionList>>.Default.Equals(Functions, type.Functions) &&
			   EqualityComparer<Dictionary<string, Type>>.Default.Equals(Types, type.Types) &&
			   EqualityComparer<Dictionary<string, Label>>.Default.Equals(Labels, type.Labels) &&
			   Identifier == type.Identifier &&
			   Modifiers == type.Modifiers &&
			   IsUnresolved == type.IsUnresolved &&
			   Format == type.Format &&
			   ReferenceSize == type.ReferenceSize &&
			   ContentSize == type.ContentSize &&
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
		hash.Add(Identifier);
		hash.Add(Modifiers);
		hash.Add(IsUnresolved);
		hash.Add(Format);
		hash.Add(ReferenceSize);
		hash.Add(ContentSize);
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