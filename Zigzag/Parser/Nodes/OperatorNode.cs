using System;

public class OperatorNode : Node, Contextable
{
	public Operator Operator { get; private set; }

	public Node Left => First;
	public Node Right => Last;

	public OperatorNode(Operator @operator)
	{
		Operator = @operator;
	}

	public OperatorNode SetOperands(Node left, Node right)
	{
		Add(left);
		Add(right);

		return this;
	}

	private Type GetClassicType()
	{
		Type left;

		if (Left is Contextable a)
		{
			Type type = a.GetContext();

			ClassicOperator @operator = Operator as ClassicOperator;

			if (!@operator.IsShared)
			{
				return type;
			}

			left = type;
		}
		else
		{
			return null;
		}

		Type right;

		if (Right is Contextable b)
		{
			right = b.GetContext();
		}
		else
		{
			return null;
		}

		return Resolver.GetSharedType(left, right);
	}

	private Type GetComparisonType()
	{
		return Types.BOOL;
	}

	private Type GetActionType()
	{
		if (Left is Contextable contextable)
		{
			return contextable.GetContext();
		}

		return null;
	}

	public virtual Type GetContext()
	{
		switch (Operator.Type)
		{
			case OperatorType.CLASSIC:
			{
				return GetClassicType();
			}

			case OperatorType.COMPARISON:
			{
				return GetComparisonType();
			}

			case OperatorType.ACTION:
			{
				return GetActionType();
			}

			default: throw new Exception("Independent operator shouldn't be processed here!");
		}
	}

	public override NodeType GetNodeType()
	{
		return NodeType.OPERATOR_NODE;
	}
}
