using System.Linq;
using System;

public class ListConstructionNode : Node, IResolvable
{
	public Node Elements => this;
	public Type? Type { get; set; } = null;

	public ListConstructionNode(Node elements, Position position)
	{
		Instance = NodeType.LIST_CONSTRUCTION;
		Position = position;

		foreach (var element in elements)
		{
			Add(element);
		}
	}

	public override Type? TryGetType()
	{
		// If the type is already set, return it
		if (Type != null) return Type;

		// Resolve the type of a single element
		var element_type = Resolver.GetSharedType(Elements.Select(i => i.TryGetType()).ToList());
		if (element_type == null) return null;

		// Try to find the environment context
		var environment_node = FindContext();
		if (environment_node == null) return null;

		var environment = environment_node.GetContext();
		var list_type = environment.GetType(Parser.StandardListType);
		if (list_type == null || !list_type.IsTemplateType) return null;

		// Get a list type with the resolved element type
		Type = list_type.To<TemplateType>().GetVariant(new[] { element_type });
		Type.Constructors.GetImplementation(Array.Empty<Type>());
		(Type.GetFunction(Parser.StandardListAdder) ?? throw new ApplicationException("Standard list is missing adder function")).GetImplementation(new[] { element_type });
		return Type;
	}

	public Node? Resolve(Context context)
	{
		foreach (var element in Elements)
		{
			Resolver.Resolve(context, element);
		}

		return null;
	}

	public Status GetStatus()
	{
		if (Type == null) return Status.Error(Position, "Can not resolve the shared type between the elements");

		return Status.OK;
	}

	public override string ToString()
	{
		return "[ " + string.Join(", ", this) + " ]";
	}
}