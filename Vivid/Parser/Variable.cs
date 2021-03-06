using System;
using System.Collections.Generic;
using System.Linq;

public class Variable
{
	public string Name { get; set; }
	public Type? Type { get; set; }
	public VariableCategory Category { get; set; }
	public int Modifiers { get; set; }
	public Position? Position { get; set; }

	public bool IsConstant => Flag.Has(Modifiers, Modifier.CONSTANT);
	public bool IsImported => Flag.Has(Modifiers, Modifier.IMPORTED);
	public bool IsPublic => Flag.Has(Modifiers, Modifier.PUBLIC);
	public bool IsProtected => Flag.Has(Modifiers, Modifier.PROTECTED);
	public bool IsPrivate => Flag.Has(Modifiers, Modifier.PRIVATE);
	public bool IsStatic => Flag.Has(Modifiers, Modifier.STATIC);
	public bool IsSelfPointer { get; set; } = false;

	public Context Context { get; set; }

	public int? LocalAlignment { get; set; }

	public List<Node> References { get; private set; } = new List<Node>();
	public List<Node> Writes { get; private set; } = new List<Node>();
	public List<Node> Reads { get; private set; } = new List<Node>();

	public bool IsEdited => Writes.Count > 0;
	public bool IsRead => Reads.Count > 0;
	public bool IsCopied => Reads.Any(i => !i.FindParent(i => !i.Is(NodeType.CAST, NodeType.CONTENT))?.Is(NodeType.LINK) ?? true);

	public bool IsUnresolved => Type == null || Type.IsUnresolved;
	public bool IsResolved => !IsUnresolved;

	public bool IsLocal => Category == VariableCategory.LOCAL;
	public bool IsParameter => Category == VariableCategory.PARAMETER;
	public bool IsMember => Category == VariableCategory.MEMBER;
	public bool IsPredictable => Category == VariableCategory.PARAMETER || Category == VariableCategory.LOCAL;
	public bool IsInlined => (Flag.Has(Modifiers, Modifier.INLINE) || !IsCopied) && !Flag.Has(Modifiers, Modifier.OUTLINE);
	public bool IsHidden => Name.Contains('.');

	public bool IsGenerated => Position == null;

	/// <summary>
	/// Creates a new variable with the specified properties
	/// </summary>
	public static Variable Create(Context context, Type? type, VariableCategory category, string name, int modifiers, bool declare = true)
	{
		return new Variable(context, type, category, name, modifiers, declare);
	}

	/// <summary>
	/// Creates a new variable with the specified properties
	/// </summary>
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

	/// <summary>
	/// Returns the mangled static name for this variable
	/// </summary>
	public string GetStaticName()
	{
		// Request the fullname in order to generate the mangled name object
		Context.GetFullname();

		var mangle = Context.Mangled!.Clone();
		var name = Name.ToLowerInvariant();

		mangle += $"{Mangle.STATIC_VARIABLE_COMMAND}{name.Length}{name}";
		mangle += Type!;
		mangle += Mangle.END_COMMAND;

		return mangle.Value;
	}

	/// <summary>
	/// Returns whether this variable is edited inside the specified node
	/// </summary>
	public bool IsEditedInside(Node node)
	{
		return Writes.Any(e => e.FindParent(p => p == node) != null);
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
		var a = Context != null && Context.IsType ? Context.ToString() : string.Empty;
		var b = (Type == null || Type.IsUnresolved) ? $"{Name}: ?" : $"{Name}: {(Type.IsUnresolved ? "?" : Type.ToString())}";
		
		return string.IsNullOrEmpty(a) ? b : a + '.' + b;
	}

	public override bool Equals(object? other)
	{
		return base.Equals(other) || other is Variable variable &&
			   Name == variable.Name &&
			   Type?.Identity == variable.Type?.Identity &&
			   Context.Identity == variable.Context.Identity &&
			   Category == variable.Category &&
			   Modifiers == variable.Modifiers;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(Name);
		hash.Add(Type?.Name);
		hash.Add(Category);
		hash.Add(Modifiers);
		hash.Add(Context.Identity);
		return hash.ToHashCode();
	}
}