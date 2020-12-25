using System;
using System.Collections.Generic;
using System.Linq;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1032")]
public class AssertionException : Exception
{
	public bool IsProblematic { get; set; }

	public AssertionException(string message, bool is_problematic = true) : base(message)
	{
		IsProblematic = is_problematic;
	}
}

public static class Assert
{
	private static long? GetIntegerValue(object? value)
	{
		return value switch
		{
			byte a => (long)a,
			short b => (long)b,
			int c => (long)c,
			long d => (long)d,
			char e => (long)e,
			ushort f => (long)f,
			uint g => (long)g,
			ulong h => (long)h,
			IntPtr i => i.ToInt64(),
			_ => null
		};
	}

	private static double? GetDecimalValue(object? value)
	{
		return value as double?;
	}

	private static void SequenceEqual(IEnumerable<object> expected, IEnumerable<object> actual)
	{
		if (!expected.Any() || !actual.Any())
		{
			if (expected.Any() != actual.Any())
			{
				throw new AssertionException("Sequences are not equal");
			}

			return;
		}

		var i = expected.GetEnumerator();
		var j = actual.GetEnumerator();

		while (true)
		{
			var a = i.MoveNext();
			var b = j.MoveNext();

			if (!a || !b)
			{
				if (a != b)
				{
					throw new AssertionException("Sequences are not equal");
				}

				return;
			}

			AreEqual(i.Current, j.Current);
		}
	}

	private static void SequenceEqual(Array expected, Array actual)
	{
		if (expected.Length != actual.Length)
		{
			throw new AssertionException("Sequences are not equal");
		}

		var i = expected.GetEnumerator();
		var j = actual.GetEnumerator();

		while (true)
		{
			var a = i.MoveNext();
			var b = j.MoveNext();

			if (!a || !b)
			{
				if (a != b)
				{
					throw new AssertionException("Sequences are not equal");
				}

				return;
			}

			AreEqual(i.Current, j.Current);
		}
	}

	public static void AreEqual(object? expected, object? actual)
	{
		var a = GetIntegerValue(expected);
		var b = GetIntegerValue(actual);

		if (a != null || b != null)
		{
			if (a != b)
			{
				throw new AssertionException($"Values are not equal:\nExpected: {a}\nActual: {b}");
			}

			return;
		}

		var i = GetDecimalValue(expected);
		var j = GetDecimalValue(actual);

		if (i != null || j != null)
		{
			if (i != j)
			{
				throw new AssertionException($"Values are not equal:\nExpected: {i}\nActual: {j}");
			}

			return;
		}

		var x = expected as IEnumerable<object>;
		var y = actual as IEnumerable<object>;

		if ((x == null) != (y == null))
		{
			throw new AssertionException($"Values are not equal:\nExpected: {expected}\nActual: {actual}");
		}

		if (x != null)
		{
			SequenceEqual(x, y!);
			return;
		}

		var s = expected as Array;
		var t = actual as Array;

		if ((s == null) != (t == null))
		{
			throw new AssertionException($"Values are not equal:\nExpected: {expected}\nActual: {actual}");
		}

		if (s != null)
		{
			SequenceEqual(s, t!);
			return;
		}

		if (expected == null && actual == null)
		{
			return;
		}

		if (!(expected?.Equals(actual) ?? false))
		{
			throw new AssertionException($"Values are not equal:\nExpected: {expected}\nActual: {actual}");
		}
	}

	public static void True(bool actual)
	{
		if (!actual)
		{
			throw new AssertionException("Assertion failed:\nExpected: true\nActual: false");
		}
	}

	public static void False(bool actual)
	{
		if (actual)
		{
			throw new AssertionException("Assertion failed:\nExpected: false\nActual: true");
		}
	}

	public static void Pass(string message)
	{
		throw new AssertionException($"Unit test passed: {message}", false);
	}

	public static void Fail(string message)
	{
		throw new AssertionException($"Unit test failed: {message}");
	}
}