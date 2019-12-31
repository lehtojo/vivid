using System;

public class Label
{
	protected string Name { get; set; }

	public Label(string name = "")
	{
		Name = name;
	}

	public virtual string GetName()
	{
		return Name;
	}

	public override string ToString()
	{
		throw new InvalidOperationException("Use method 'GetName' instead of 'ToString' when interacting with labels");
	}
}

public class RequestableLabel : Label
{
	public bool Used { get; private set; } = false;

	private Unit Unit { get; set; }

	public RequestableLabel(Unit unit)
	{
		Unit = unit;
	}

	public override string GetName()
	{
		if (!Used)
		{
			Used = true;
			Name = Unit.NextLabel;
		}
		
		return base.GetName();
	}
}