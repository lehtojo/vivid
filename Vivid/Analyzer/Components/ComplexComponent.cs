public class ComplexComponent : Component
{
	public Node Node { get; private set; }
	public bool IsNegative { get; private set; }

	public override void Negate()
	{
		IsNegative = !IsNegative;
	}

	public ComplexComponent(Node node)
	{
		Node = node;
	}

	public override Component? Add(Component other)
	{
		return Numbers.IsZero(other) ? Clone() : null;
	}

	public override Component? Subtract(Component other)
	{
		return Numbers.IsZero(other) ? Clone() : null;
	}

	public override Component? Multiply(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		return Numbers.IsZero(other) ? new NumberComponent(0L) : null;
	}

	public override Component? Divide(Component other)
	{
		if (Numbers.IsOne(other))
		{
			return Clone();
		}

		return null;
	}

	public override Component Clone()
	{
		var clone = (ComplexComponent)MemberwiseClone();
		clone.Node = Node.Clone();

		return clone;
	}
}