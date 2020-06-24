using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

public class Variable
{
	public string Name { get; set; }
	public Type? Type { get; set; }
	public VariableCategory Category { get; set; }
	public int Modifiers { get; set; }

	public bool IsConstant => Flag.Has(Modifiers, AccessModifier.CONSTANT);
	public bool IsExternal => Flag.Has(Modifiers, AccessModifier.EXTERNAL);
	public bool IsStatic => Flag.Has(Modifiers, AccessModifier.STATIC);
	public bool IsThisPointer => Name == Function.THIS_POINTER_IDENTIFIER;
	
	public Context Context { get; set; }

	public int? Alignment { get; set; }

	public List<Node> References { get; private set; } = new List<Node>();
	public List<Node> Edits { get; private set; } = new List<Node>();
	public List<Node> Reads { get; private set; } = new List<Node>();
	
	public bool IsEdited => Edits.Count > 0;
	public bool IsRead => Reads.Count > 0;
	public bool IsUsed => References.Count > 1;

	public bool IsUnresolved => Type == Types.UNKNOWN || Type is IResolvable;
	
	public bool IsLocal => Category == VariableCategory.LOCAL;
	public bool IsPredictable => Category == VariableCategory.PARAMETER || Category == VariableCategory.LOCAL;

	public static void Create(Context context, Type? type, VariableCategory category, string name, int modifiers, bool declare = true)
	{
		new Variable(context, type, category, name, modifiers, declare);
	}

	public Variable(Context context, Type? type, VariableCategory category, string name, int modifiers, bool declare = true)
	{
		Name = name;
		Type = type;
		Category = category;
		Modifiers = modifiers;
		Context = context;

		if (declare)
		{
			context.Declare(this);
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1308", Justification = "Assembly style required lower case")]
	public string GetStaticName()
	{
		return Context.GetFullname() + '_' + Name.ToLowerInvariant();
	}

	public bool IsEditedInside(Node node)
	{
		return Edits.Any(e => e.FindParent(p => p == node) != null);
	}

	public override string ToString()
	{
		return $"{Name}: {(Type?.Name ?? "?")}";
	}

	public override bool Equals(object? obj)
	{
		return obj is Variable variable &&
			   Name == variable.Name &&
			   EqualityComparer<string?>.Default.Equals(Type?.Name, variable.Type?.Name) &&
			   Category == variable.Category &&
			   Modifiers == variable.Modifiers &&
			   EqualityComparer<int>.Default.Equals(References.Count, variable.References.Count) &&
			   EqualityComparer<int>.Default.Equals(Edits.Count, variable.Edits.Count) &&
			   EqualityComparer<int>.Default.Equals(Reads.Count, variable.Reads.Count);
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(Name);
		hash.Add(Type?.Name);
		hash.Add(Category);
		hash.Add(Modifiers);
		hash.Add(References.Count);
		hash.Add(Edits.Count);
		hash.Add(Reads.Count);
		return hash.ToHashCode();
	}
}