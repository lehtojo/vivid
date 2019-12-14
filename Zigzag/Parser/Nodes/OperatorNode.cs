using System;

public class OperatorNode : Node, IType
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

	private Type? GetClassicType()
	{
		Type? left;

		if (Left is IType a)
		{
			var type = a.GetType();

			if (!(Operator is ClassicOperator @operator))
			{
				throw new Exception("Invalid operator given");
			}

			if (!@operator.IsShared)
			{
				return type;
			}

			left = type;
		}
		else
		{
			return Types.UNKNOWN;
		}

		Type? right;

		if (Right is IType b)
		{
			right = b.GetType();
		}
		else
		{
			return Types.UNKNOWN;
		}

		return Resolver.GetSharedType(left, right);
	}

	private Type? GetComparisonType()
	{
		return Types.BOOL;
	}

	private Type? GetActionType()
	{
		if (Left is IType type)
		{
			return type.GetType();
		}

		return Types.UNKNOWN;
	}

	public virtual Type? GetType()
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
