using System;
using System.Collections.Generic;
using System.Text;

public class Decimal : Number
{
	private const int BYTES = 4;

	public Decimal() : base(NumberType.DECIMAL32, 32, "decimal") { }

	public override int GetSize()
	{
		return BYTES;
	}
}
