using System;
using System.Linq;

public class Link : Number
{
	/// <summary>
	/// Creates a link type which has the specified offset type
	/// </summary>
	public static Type GetVariant(Type argument)
	{
		var link = new Link();
		link.TemplateArguments = new[] { argument };
		return link;
	}

	/// <summary>
	/// Creates a link type which has the specified offset type and the specified name
	/// </summary>
	public static Type GetVariant(Type argument, string name)
	{
		var link = new Link();
		link.Name = name;
		link.TemplateArguments = new[] { argument };
		return link;
	}

	public Link() : base(Parser.Format, Parser.Bits, true, "link")
	{
		Identifier = Primitives.LINK_IDENTIFIER;
		Modifiers |= Modifier.TEMPLATE_TYPE;
	}

	public Link(Type argument) : base(Parser.Format, Parser.Bits, true, "link")
	{
		Identifier = Primitives.LINK_IDENTIFIER;
		Modifiers |= Modifier.TEMPLATE_TYPE;
		TemplateArguments = new[] { argument };
	}

	public override Type Clone()
	{
		var clone = base.Clone();
		clone.TemplateArguments = (Type[])TemplateArguments.Clone();

		return clone;
	}

	public override Type GetAccessorType()
	{
		return TemplateArguments.FirstOrDefault() ?? Primitives.CreateNumber(Primitives.U8, Format.UINT8);
	}

	public override bool Equals(object? other)
	{
		// Require the other type to be a link
		if (other is not Link link) return false;

		// Use name and identity to compare the accessor types.
		// Do not use this function recursively, because it would lead to a lot of calls 
		// and possibly to infinite recursion when a type contains a link to itself.
		var a = GetAccessorType();
		var b = link.GetAccessorType();
		return a.Name == b.Name && a.Identity == b.Identity;
	}

	public override int GetHashCode()
	{
		return GetAccessorType().GetHashCode() + 1;
	}

	public override string ToString()
	{
		return GetAccessorType().ToString() + '*';
	}
}