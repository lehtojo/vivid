public abstract class Component
{
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