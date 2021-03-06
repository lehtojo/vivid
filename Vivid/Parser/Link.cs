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

	public Link() : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = Primitives.LINK_IDENTIFIER;
		Modifiers |= Modifier.TEMPLATE_TYPE;
	}

	public Link(Type argument) : base(Parser.Format, Parser.Size.Bits, true, "link")
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

	public override Type GetOffsetType()
	{
		return TemplateArguments.FirstOrDefault() ?? Primitives.CreateNumber(Primitives.U8, Format.UINT8);
	}

	public override int GetContentSize()
	{
		return GetOffsetType().ReferenceSize;
	}
}