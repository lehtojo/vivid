using System;
using System.Collections.Generic;

public class Label
{
	protected string Name { get; set; }
	public List<JumpNode> Jumps { get; private set; } = new List<JumpNode>();

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
		return Name;
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