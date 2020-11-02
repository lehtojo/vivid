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

	public override bool Equals(object? other)
	{
		return other is Label label &&
			   Name == label.Name;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Name);
	}
}