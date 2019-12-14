public abstract class Reference
{
	public object Metadata { get; set; }

	protected Size Size { get; set; }

	public Reference(Size size)
	{
		Size = size;
	}

	public abstract string Use(Size size);
	public abstract string Use();

	public abstract new LocationType GetType();

	public virtual void Lock() { }

	public virtual string Peek(Size size)
	{
		return Use(size);
	}

	public virtual string Peek()
	{
		return Use();
	}

	public abstract bool IsComplex();

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