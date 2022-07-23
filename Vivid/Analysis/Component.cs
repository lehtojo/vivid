public abstract class Component
{
	public const int COMPARISON_UNKNOWN = 2;

	public bool IsInteger => this is NumberComponent && ((NumberComponent)this).Value is long;

	public abstract void Negate();

	public virtual Component? Add(Component other)
	{
		return null;
	}

	public virtual Component? Subtract(Component other)
	{
		return null;
	}

	public virtual Component? Multiply(Component other)
	{
		return null;
	}

	public virtual Component? Divide(Component other)
	{
		return null;
	}

	public virtual Component? BitwiseAnd(Component other)
	{
		return null;
	}

	public virtual Component? BitwiseXor(Component other)
	{
		return null;
	}

	public virtual Component? BitwiseOr(Component other)
	{
		return null;
	}

	public virtual int Compare(Component component)
	{
		return COMPARISON_UNKNOWN;
	}

	public static Component? operator +(Component left, Component right)
	{
		return left.Add(right);
	}

	public static Component? operator -(Component left, Component right)
	{
		return left.Subtract(right);
	}

	public static Component? operator *(Component left, Component right)
	{
		return left.Multiply(right);
	}

	public static Component? operator /(Component left, Component right)
	{
		return left.Divide(right);
	}

	public virtual Component Clone()
	{
		return (Component)MemberwiseClone();
	}
}