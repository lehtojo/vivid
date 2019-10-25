public abstract class Reference
{
	protected Size Size { get; set; }

	public Reference(Size size)
	{
		Size = size;
	}

	public abstract string Use(Size size);
	public abstract new LocationType GetType();

	public virtual string Peek(Size size)
	{
		return Use(size);
	}

	public virtual bool IsComplex()
	{
		return false;
	}

	public virtual bool IsRegister()
	{
		return false;
	}

	public virtual Register GetRegister()
	{
		return null;
	}

	public virtual Size GetSize()
	{
		return Size;
	}
}