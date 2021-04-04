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
			byte a => a,
			short b => b,
			int c => c,
			long d => d,
			char e => e,
			ushort f => f,
			uint g => g,
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

	private static AssertionException? InternalAreEqual(object? expected, object? actual)
	{
		var a = GetIntegerValue(expected);
		var b = GetIntegerValue(actual);

		if (a != null || b != null)
		{
			if (a != b)
			{
				return new AssertionException($"Values are not equal:\nExpected: {a}\nActual: {b}");
			}

			return null;
		}

		var i = GetDecimalValue(expected);
		var j = GetDecimalValue(actual);

		if (i != null || j != null)
		{
			if (i != j)
			{
				return new AssertionException($"Values are not equal:\nExpected: {i}\nActual: {j}");
			}

			return null;
		}

		var x = expected as IEnumerable<object>;
		var y = actual as IEnumerable<object>;

		if ((x == null) != (y == null))
		{
			return new AssertionException($"Values are not equal:\nExpected: {expected}\nActual: {actual}");
		}

		if (x != null)
		{
			SequenceEqual(x, y!);
			return null;
		}

		var s = expected as Array;
		var t = actual as Array;

		if ((s == null) != (t == null))
		{
			return new AssertionException($"Values are not equal:\nExpected: {expected}\nActual: {actual}");
		}

		if (s != null)
		{
			SequenceEqual(s, t!);
			return null;
		}

		if (expected == null && actual == null)
		{
			return null;
		}

		if (!(expected?.Equals(actual) ?? false))
		{
			return new AssertionException($"Values are not equal:\nExpected: {expected}\nActual: {actual}");
		}

		return null;
	}

	public static void AreEqual(object? expected, object? actual)
	{
		var exception = InternalAreEqual(expected, actual);

		if (exception != null)
		{
			throw exception;
		}
	}

	public static void AreNotEqual(object? expected, object? actual)
	{
		var exception = InternalAreEqual(expected, actual);

		if (exception == null)
		{
			throw new AssertionException("Values were equal");
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