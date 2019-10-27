using System.Collections.Generic;

public class Variable
{
	public string Name { get; private set; }
	public Type Type { get; set; }
	public VariableCategory Category { get; set; }
	public int Modifiers { get; private set; }
	public int Length { get; private set; }
	public bool IsArray => Length > 0;

	public Context Context { get; set; }

	public int Alignment { get; set; }

	public List<Node> References = new List<Node>();

	public bool IsUnresolved => Type is IResolvable;

	public Variable(Context context, Type type, VariableCategory category, string name, int modifiers, int length = 0)
	{
		Name = name;
		Type = type;
		Category = category;
		Modifiers = modifiers;
		Context = context;
		Length = length;

		context.Declare(this);
	}
}