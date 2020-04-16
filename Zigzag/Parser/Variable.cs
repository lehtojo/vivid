using System.Collections.Generic;

public class Variable
{
	public string Name { get; set; }
	public Type? Type { get; set; }
	public VariableCategory Category { get; set; }
	public int Modifiers { get; set; }
	public int Length { get; private set; }
	public bool IsArray => Length > 0;

	public Context Context { get; set; }

	public int Alignment { get; set; }

	public List<Node> References { get; private set; } = new List<Node>();
	public List<Node> Edits { get; private set; } = new List<Node>();
	public List<Node> Reads { get; private set; } = new List<Node>();
	
	public bool IsEdited => Edits.Count > 0;
	public bool IsRead => Reads.Count > 0;
	public bool IsUsed => References.Count > 1;

	public bool IsUnresolved => Type == Types.UNKNOWN || Type is IResolvable;
	
	public bool IsPredictable => Category == VariableCategory.PARAMETER || Category == VariableCategory.LOCAL;

	public Variable(Context context, Type? type, VariableCategory category, string name, int modifiers, int length = 0)
	{
		Name = name;
		Type = type;
		Category = category;
		Modifiers = modifiers;
		Context = context;
		Length = length;

		// Parameters mustn't be declared since they their own lists
		if (category != VariableCategory.PARAMETER)
		{
			context.Declare(this);
		}
	}
}