using System;
using System.Collections.Generic;

public class InlineNode : Node, IType
{
	public FunctionImplementation Implementation { get; private set; }

	public Node Body => First!;
	public Node? Result => !Last!.Is(NodeType.NORMAL) ? Last : null;

	private Label? _End;

	public Label End
	{
		get
		{
			if (_End == null)
			{
				_End = Implementation.GetLabel();
			}

			return _End;
		}
	}

	public InlineNode(FunctionImplementation implementation, Variable? result)
	{
		Implementation = implementation;

		Add(new Node());

		if (result != null)
		{
			Add(new VariableNode(result));
		}
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
			   EqualityComparer<Node>.Default.Equals(Body, node.Body) &&
			   End == node.End;
	}

	public override int GetHashCode()
	{
		HashCode hash = new HashCode();
		hash.Add(base.GetHashCode());
		hash.Add(Body);
		hash.Add(End);
		return hash.ToHashCode();
	}
}
