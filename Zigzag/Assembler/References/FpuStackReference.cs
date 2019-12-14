using System;
using System.Collections.Generic;
using System.Text;

public class FpuStackReference : Reference
{
	public FpuStackElement Element { get; set; }

	public FpuStackReference(FpuStackElement element) : base(Size.DWORD)
	{
		Element = element;
	}

	public override string Use(Size size)
	{
		Element.Critical = false;

		return Element.Register.ToString();
	}

	public override string Use()
	{
		Element.Critical = false;

		return Element.Register.ToString();
	}

	public override LocationType GetType()
	{
		return LocationType.FPU;
	}

	public override bool Equals(object? obj)
	{
		return obj is FpuStackReference reference &&
			   Element.Register.Index == reference.Element.Register.Index;
	}

	public override bool IsComplex()
	{
		return false;
	}
}
