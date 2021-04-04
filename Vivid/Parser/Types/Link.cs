using System.Linq;

public class Link : Number
{
	public Link() : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = "Ph";
		Modifiers |= Modifier.TEMPLATE_TYPE;
	}

	public Link(Type argument) : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = "Ph";
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
		return TemplateArguments.FirstOrDefault() ?? global::Types.U8;
	}

	public override int GetContentSize()
	{
		return (TemplateArguments.FirstOrDefault() ?? global::Types.TINY).ReferenceSize;
	}
}