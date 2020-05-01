using System;
using System.Collections.Generic;

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

    public override bool Equals(object? obj)
    {
        return obj is InlineNode node &&
               base.Equals(obj) &&
               EqualityComparer<Node>.Default.Equals(Parameters, node.Parameters) &&
               EqualityComparer<Node>.Default.Equals(Body, node.Body) &&
               End == node.End;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Parameters);
        hash.Add(Body);
        hash.Add(End);
        return hash.ToHashCode();
    }
}
