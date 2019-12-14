using System.Collections.Generic;
using System.Linq;

public class Type : Context
{
	public const int REFERENCE_SIZE = 4;
	public const string IDENTIFIER_PREFIX = "type_";

	public string Identifier => IDENTIFIER_PREFIX + Name + "_";
	public int Modifiers { get; private set; }

	public bool IsUnresolved => this is IResolvable;

	public int ContentSize => Variables.Values.Select(v => v.Type.Size).Sum();
	public int Size => GetSize();

	public List<Type> Supertypes { get; private set; } = new List<Type>();
	public FunctionList Constructors { get; private set; } = new FunctionList();
	public FunctionList Destructors { get; private set; } = new FunctionList();
	public FunctionList Operators { get; set; } = new FunctionList();

	public void AddConstructor(Constructor constructor)
	{
		Constructor first = (Constructor)Constructors.Overloads.First();

		if (first.IsDefault)
		{
			Constructors.Overloads.Remove(first);
		}

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

	public virtual int GetSize()
	{
		return REFERENCE_SIZE;
	}

	public bool IsSuperFunctionDeclared(string name)
	{
		return Supertypes.Any(t => t.IsLocalFunctionDeclared(name));
	}

	public bool IsSuperVariableDeclared(string name)
	{
		return Supertypes.Any(t => t.IsLocalVariableDeclared(name));
	}

	public FunctionList GetSuperFunction(string name)
	{
		return Supertypes.First(t => t.IsLocalFunctionDeclared(name)).GetFunction(name);
	}

	public Variable GetSuperVariable(string name)
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

	public override FunctionList GetFunction(string name)
	{
		if (base.IsLocalFunctionDeclared(name))
		{
			return base.GetFunction(name);
		}
		else if (IsSuperFunctionDeclared(name))
		{
			return GetSuperFunction(name);
		}
		else
		{
			return base.GetFunction(name);
		}
	}

	public override Variable GetVariable(string name)
	{
		if (base.IsLocalVariableDeclared(name))
		{
			return base.GetVariable(name);
		}
		else if (IsSuperVariableDeclared(name))
		{
			return GetSuperVariable(name);
		}
		else
		{
			return base.GetVariable(name);
		}
	}
}