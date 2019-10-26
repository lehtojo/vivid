public class MemoryReference : Reference
{
	private Register Register { get; set; }
	private int Alignment { get; set; }

	public MemoryReference(Register register, int alignment, int size) : this(register, alignment, Size.Get(size)) { }

	public MemoryReference(Register register, int alignment, Size size) : base(size)
	{
		Register = register;
		Alignment = alignment;
	}

	public string GetContent()
	{
		if (Alignment > 0)
		{
			return $"{Register}+{Alignment}";
		}
		else if (Alignment < 0)
		{
			return $"{Register}{Alignment}";
		}
		else
		{
			return $"{Register}";
		}
	}

	public override string Use(Size size)
	{
		return $"{size} [{GetContent()}]";
	}

	public override bool IsComplex()
	{
		return true;
	}
	public override LocationType GetType()
	{
		return LocationType.MEMORY;
	}

	public static MemoryReference Local(Unit unit, int alignment, int size)
	{
		return new MemoryReference(unit.EBP, -alignment - size, size);
	}

	public static MemoryReference Parameter(Unit unit, int alignment, int size)
	{
		return new MemoryReference(unit.EBP, alignment + 8, size);
	}

	public static MemoryReference Member(Register register, int alignment, int size)
	{
		return new MemoryReference(register, alignment, size);
	}
}