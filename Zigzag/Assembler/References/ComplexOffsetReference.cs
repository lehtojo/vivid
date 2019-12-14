using System;
using System.Collections.Generic;
using System.Text;

class ComplexOffsetReference : Reference
{
	public Reference Start { get; private set; }
	public Reference Index { get; private set; }

	public int Stride { get; private set; }

	public ComplexOffsetReference(Reference start, Reference index, int stride) : base(Size.Get(stride))
	{
		Start = start;
		Index = index;
		Stride = stride;

		Lock();
	}

	public override void Lock()
	{
		Start.Lock();
		Index.Lock();
	}

	public override string Use(Size size)
	{
		return $"{size} {Arrays.GetOffsetCalculation(Start, Index, Stride)}";
	}

	public override string Use()
	{
		return Arrays.GetOffsetCalculation(Start, Index, Stride);
	}

	public override LocationType GetType()
	{
		return LocationType.OFFSET;
	}

	public override bool IsComplex()
	{
		return true;
	}
}
