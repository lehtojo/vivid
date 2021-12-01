using System.Collections.Generic;
using System.Linq;

public class PackConstructionNode : Node, IResolvable
{
	public Type? Type { get; set; } = null;
	public List<string> Members { get; set; }

	public PackConstructionNode(List<string> members, List<Node> arguments, Position? position)
	{
		Instance = NodeType.PACK_CONSTRUCTION;
		Position = position;

		Members = members;

		// Add the arguments as children
		foreach (var argument in arguments)
		{
			Add(argument);
		}
	}

	public override Type? TryGetType()
	{
		return Type;
	}

	private bool ValidateMemberNames()
	{
		// Ensure that all member names are unique
		return Members.Distinct().Count() == Members.Count;
	}

	public Node? Resolve(Context context)
	{
		// Resolve the arguments
		foreach (var argument in this)
		{
			Resolver.Resolve(context, argument);
		}

		// Skip the process below, if it has been executed already
		if (Type != null) return null;

		// Try to resolve the type of the arguments, these types are the types of the members
		var types = Resolver.GetTypes(this);
		if (types == null) return null;

		// Ensure that all member names are unique
		if (!ValidateMemberNames()) return null;

		// Create a new pack type in order to construct the pack later
		Type = context.DeclareHiddenPack(Position);

		// Declare the pack members
		for (var i = 0; i < Members.Count; i++)
		{
			Type.Declare(types[i], VariableCategory.MEMBER, Members[i]);
		}

		return null;
	}

	public Status GetStatus()
	{
		// Ensure that all member names are unique
		if (!ValidateMemberNames()) return Status.Error(Position, "All pack members must be named differently");

		if (Type == null) return Status.Error(Position, "Can not resolve the types of the pack members");

		return Status.OK;
	}

	public override string ToString()
	{
		return $"Pack { string.Join(", ", Members) }";
	}
}