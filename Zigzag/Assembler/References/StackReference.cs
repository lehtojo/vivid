using System;
using System.Collections.Generic;
using System.Text;

public class StackReference : Reference
{
	public StackElement Element { get; private set; }

	public StackReference(StackElement element) : base(Size.DWORD)
	{
		Element = element;
	}

	public override LocationType GetType()
	{
		return LocationType.STACK;
	}

	public string GetContent()
	{
		var alignment = Element.Alignment;

		if (alignment > 0)
		{
			return $"esp+{alignment}";
		}
		else if (alignment < 0)
		{
			return $"esp{alignment}";
		}
		else
		{
			return "esp";
		}
	}

	public override string Use(Size size)
	{
		return $"{size} [{GetContent()}]";
	}

	public override string Use()
	{
		return $"[{GetContent()}]";
	}

	public override bool IsComplex()
	{
		return true;
	}

	public override bool Equals(object? obj)
	{
		return obj is StackReference reference &&
			   Element.Alignment == reference.Element.Alignment;
	}
}
