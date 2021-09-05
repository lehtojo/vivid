using System.Collections.Generic;

public class WhenNode : Node, IResolvable
{
	public Node Value => First!;
	public VariableNode Inspected => Value.Next!.To<VariableNode>();
	public Node Sections => Last!;

	public WhenNode(Node value, VariableNode inspected, List<Node> sections, Position? position)
	{
		Instance = NodeType.WHEN;
		Position = position;

		Add(value);
		Add(inspected);
		Add(new Node());

		foreach (var section in sections) Sections.Add(section);
	}

	public override Type? TryGetType()
	{
		var types = new List<Type>();

		foreach (var section in Sections)
		{
			var body = GetSectionBody(section);
			var value = body.Last;

			if (value == null) return null;

			var type = value.TryGetType();
			if (type == null) return null;
			
			types.Add(type);
		}

		return Resolver.GetSharedType(types);
	}

	public Node GetSectionBody(Node section)
	{
		return section.Instance switch
		{
			NodeType.IF => section.To<IfNode>().Body,
			NodeType.ELSE_IF => section.To<ElseIfNode>().Body,
			NodeType.ELSE => section.To<ElseNode>().Body,
			_ => throw Errors.Get(Position, "Unsupported section")
		};
	}

	public Node? Resolve(Context environment)
	{
		Resolver.Resolve(environment, Value);
		Resolver.Resolve(environment, Inspected);
		Resolver.Resolve(environment, Sections);
		return null;
	}

	public Status GetStatus()
	{
		var inspected_type = Value.TryGetType();
		Inspected.Variable.Type = inspected_type;

		if (inspected_type == null) return Status.Error(Inspected.Position, "Can not resolve the type of the inspected value");

		var types = new List<Type>();

		foreach (var section in Sections)
		{
			var body = GetSectionBody(section);
			var value = body.Last;

			if (value == null) return Status.Error(Position, "When-statement has an empty section");

			var type = value.TryGetType();
			if (type == null) return Status.Error(value.Position, "Can not resolve the section return type");
			
			types.Add(type);
		}

		if (Resolver.GetSharedType(types) == null) return Status.Error(Position, "Sections do not have a shared return type");
		return Status.OK;
	}

	public override string ToString() => $"When";
}