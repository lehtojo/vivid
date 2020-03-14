public class InlineNode : Node, IType
{
	public FunctionImplementation Implementation { get; private set; }

	public Node Parameters { get; private set; } = new Node();
	public Node Body { get; private set; } = new Node();

	public bool End { get; private set; } = false;

	public InlineNode(FunctionImplementation implementation, Node parameters, Node body)
	{
		Implementation = implementation;

		var iterator = body.First;

		while (iterator != null)
		{
			var next = iterator.Next;
			Body.Add(iterator);
			iterator = next;
		}

		iterator = parameters.First;

		while (iterator != null)
		{
			var next = iterator.Next;
			Parameters.Add(iterator);
			iterator = next;
		}

		Add(Body);
		Add(Parameters);
	}

	public string GetEndLabel()
	{
		End = true;
		return $"inline_{Implementation.Metadata!.GetFullname()}_end";
	}

	public override NodeType GetNodeType()
	{
		return NodeType.INLINE_NODE;
	}

	public new Type? GetType()
	{
		return Implementation.ReturnType;
	}
}
