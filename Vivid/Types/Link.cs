using System.Linq;

public class Link : Number
{
	public Link() : base(Parser.Format, Parser.Size.Bits, true, "link")
	{
		Identifier = "Ph";
	}

	public override Type Clone()
	{
		var clone = base.Clone();
		clone.TemplateArguments = (Type[])TemplateArguments.Clone();

		return clone;
	}

	public override Type GetOffsetType()
	{
		return TemplateArguments.FirstOrDefault() ?? global::Types.TINY;
	}

	public override void AddDefinition(Mangle mangle)
	{
		mangle.Value += "Ph";
	}

	public override int GetContentSize()
	{
		return (TemplateArguments.FirstOrDefault() ?? global::Types.TINY).ReferenceSize;
	}
}