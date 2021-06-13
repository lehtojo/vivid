using System.Collections.Generic;
using System.Linq;

public class WhenNode : Node, IResolvable
{
	public VariableNode Inspected => First!.To<VariableNode>();
	public Node Sections => Last!;

	public WhenNode(VariableNode value, List<Node> sections, Position? position)
	{
		Instance = NodeType.WHEN;
		Position = position;

		Add(value);
		Add(new Node());

		foreach (var section in sections) Sections.Add(section);
	}

	private Node GetSectionBody(Node section)
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
		Resolver.Resolve(environment, Inspected);
		Resolver.Resolve(environment, Sections);

		if (GetStatus().IsProblematic) return null;

		var inline = new InlineNode(Position);
		var return_type = Resolver.GetSharedType(Sections.Select(i => GetSectionBody(i).Last!.GetType()).ToArray()) ?? throw Errors.Get(Position, "Could not resolve the return type of the statement");

		// The return value of the when-statement must be loaded into a separate variable
		var return_value_variable = environment.DeclareHidden(return_type);
		inline.Add(new DeclareNode(return_value_variable, Position));

		foreach (var section in Sections)
		{
			var body = GetSectionBody(section);

			// Load the return value of the section to the return value variable
			var value = body.Last!;
			var destination = new Node();
			value.Replace(destination);

			destination.Replace(new OperatorNode(Operators.ASSIGN, value.Position).SetOperands(
				new VariableNode(return_value_variable, value.Position),
				value
			));

			inline.Add(section);
		}

		// When-statements are added inside inline nodes
		Parent!.Add(new VariableNode(return_value_variable, Position));
		return inline;
	}

	public Status GetStatus()
	{
		if (Inspected.TryGetType() == null) return Status.Error(Inspected.Position, "Can not resolve the type of the inspected value");

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