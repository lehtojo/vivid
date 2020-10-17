using System;
using System.Collections.Generic;
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
	public bool IsSelfPointer => Context.GetSelfPointer() == this;

	public Context Context { get; set; }

	public int? LocalAlignment { get; set; }

	public List<Node> References { get; private set; } = new List<Node>();
	public List<Node> Edits { get; private set; } = new List<Node>();
	public List<Node> Reads { get; private set; } = new List<Node>();

	public bool IsEdited => Edits.Count > 0;
	public bool IsRead => Reads.Count > 0;
	public bool IsUsed => References.Count > 1;

	public bool IsUnresolved => Type == Types.UNKNOWN || Type is IResolvable;
	public bool IsResolved => !IsUnresolved;

	public bool IsLocal => Category == VariableCategory.LOCAL;
	public bool IsParameter => Category == VariableCategory.PARAMETER;
	public bool IsMember => Category == VariableCategory.MEMBER;
	public bool IsPredictable => Category == VariableCategory.PARAMETER || Category == VariableCategory.LOCAL;

	public static Variable Create(Context context, Type? type, VariableCategory category, string name, int modifiers, bool declare = true)
	{
		return new Variable(context, type, category, name, modifiers, declare);
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

	public int? GetAlignment(Type parent)
	{
		if (Context == parent)
		{
			var local_alignment = LocalAlignment ?? throw new ApplicationException($"Variable '{Name}' was not aligned yet");

			return parent.Supertypes.Sum(s => s.ContentSize) + local_alignment;
		}

		var position = 0;

		foreach (var supertype in parent.Supertypes)
		{
			var local_alignment = GetAlignment(supertype);

			if (local_alignment != null)
			{
				return position + local_alignment;
			}

			position += supertype.ContentSize;
		}

		return null;
	}

	public override string ToString()
	{
		if (Type == null)
		{
			return $"{Name}: _";
		}

		return $"{Name}: {(Type.IsUnresolved ? "?" : Type.ToString())}";
	}

	public override bool Equals(object? other)
	{
		return base.Equals(other) || other is Variable variable &&
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