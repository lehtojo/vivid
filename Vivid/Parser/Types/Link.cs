using System.Linq;

public class Link : Number
{
	public Link() : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = "Ph";
		Modifiers |= Modifier.TEMPLATE_TYPE;
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

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "P";
		mangle += GetOffsetType();
	}

	public override int GetContentSize()
	{
		return (TemplateArguments.FirstOrDefault() ?? global::Types.TINY).ReferenceSize;
	}
}