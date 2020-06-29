using System;
using System.Collections.Generic;

public class Numbers
{
	private static readonly Dictionary<Format, Number> Values = new Dictionary<Format, Number>();

	public static Number Get(Format type)
	{
		return Values[type];
	}

	private static void Define(Number number)
	{
		Values.Add(number.Type, number);
	}

	static Numbers()
	{
		Define(Types.TINY);
		Define(Types.SMALL);
		Define(Types.NORMAL);
		Define(Types.LARGE);
		
		Define(Types.U8);
		Define(Types.U16);
		Define(Types.U32);
		Define(Types.U64);

		Define(Types.DECIMAL);
	}

	/// <summary>
	/// Adds the two operands together
	/// </summary>
	/// <param name="x">First long or double operand</param>
	/// <param name="y">Second long or double operand</param>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Add(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) + Convert.ToDouble(y);
		}

		return (long) x + (long) y;
	}
	
	/// <summary>
	/// Subtracts the two operands together
	/// </summary>
	/// <param name="x">First long or double operand</param>
	/// <param name="y">Second long or double operand</param>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Subtract(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) - Convert.ToDouble(y);
		}

		return (long) x - (long) y;
	}
	
	/// <summary>
	/// Subtracts the two operands together
	/// </summary>
	/// <param name="x">First long or double operand</param>
	/// <param name="y">Second long or double operand</param>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Multiply(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) * Convert.ToDouble(y);
		}

		return (long) x * (long) y;
	}
	
	/// <summary>
	/// Subtracts the two operands together
	/// </summary>
	/// <param name="x">First long or double operand</param>
	/// <param name="y">Second long or double operand</param>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Divide(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) / Convert.ToDouble(y);
		}

		return (long) x / (long) y;
	}

	/// <summary>
	/// Returns the absolute value of the specified number
	/// </summary>
	public static object Abs(object x)
	{
		return x is long a ? Math.Abs(a) : Math.Abs((double) x);
	}
}