using System;
using System.Collections.Generic;
using System.Globalization;

public static class Numbers
{
	private static Dictionary<Format, Number> Values { get; } = new Dictionary<Format, Number>();

	public static Number Get(Format format)
	{
		return Values[format];
	}

	private static void Define(Number number)
	{
		Values.Add(number.Type, number);
	}

	static Numbers()
	{
		Define(Primitives.CreateNumber(Primitives.TINY, Format.INT8));
		Define(Primitives.CreateNumber(Primitives.SMALL, Format.INT16));
		Define(Primitives.CreateNumber(Primitives.NORMAL, Format.INT32));
		Define(Primitives.CreateNumber(Primitives.LARGE, Format.INT64));
		Define(Primitives.CreateNumber(Primitives.U8, Format.UINT8));
		Define(Primitives.CreateNumber(Primitives.U16, Format.UINT16));
		Define(Primitives.CreateNumber(Primitives.U32, Format.UINT32));
		Define(Primitives.CreateNumber(Primitives.U64, Format.UINT64));
		Define(Primitives.CreateNumber(Primitives.DECIMAL, Format.DECIMAL));
	}

	/// <summary>
	/// Adds the two operands together
	/// </summary>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Add(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) + Convert.ToDouble(y);
		}

		return (long)x + (long)y;
	}

	/// <summary>
	/// Subtracts the two operands together
	/// </summary>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Subtract(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) - Convert.ToDouble(y);
		}

		return (long)x - (long)y;
	}

	/// <summary>
	/// Multiplies the two operands together
	/// </summary>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Multiply(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) * Convert.ToDouble(y);
		}

		return (long)x * (long)y;
	}

	/// <summary>
	/// Divides the two operands together
	/// </summary>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Divide(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) / Convert.ToDouble(y);
		}

		return (long)x / (long)y;
	}

	/// <summary>
	/// Divides the two operands together and returns the remainder
	/// </summary>
	/// <returns>Returns the result of the operation. The result is a double if any of the operands is a double, otherwise  </returns>
	public static object Remainder(object x, object y)
	{
		if (x is double || y is double)
		{
			return Convert.ToDouble(x) % Convert.ToDouble(y);
		}

		return (long)x % (long)y;
	}

	/// <summary>
	/// Returns the absolute value of the specified number
	/// </summary>
	public static object Abs(object x)
	{
		return x is long a ? Math.Abs(a) : Math.Abs((double)x);
	}

	/// <summary>
	/// Negates the specified number
	/// </summary>
	public static object Negate(object x)
	{
		return x is long a ? -a : -(double)x;
	}

	/// <summary>
	/// Returns whether the specified number is negative
	/// </summary>
	public static bool IsNegative(object x)
	{
		return x is long a ? a < 0 : (double)x < 0;
	}

	/// <summary>
	/// Returns whether the specified number is exactly zero
	/// </summary>
	public static bool IsZero(object x)
	{
		if (x is Component c)
		{
			return c is NumberComponent n && IsZero(n.Value);
		}

		return x is long a ? a == 0 : (double)x == 0.0;
	}

	/// <summary>
	/// Returns whether the specified number is exactly one
	/// </summary>
	public static bool IsOne(object x)
	{
		if (x is Component c)
		{
			return c is NumberComponent n && IsOne(n.Value);
		}

		return x is long a ? a == 1 : (double)x == 1.0;
	}

	/// <summary>
	/// Returns whether the specified number equals exactly the specified value
	/// </summary>
	public static bool Equals(object x, long value)
	{
		return x is long a ? a == value : (double)x == value;
	}
}