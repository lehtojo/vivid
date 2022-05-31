using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public static class TestSettings
{
	public const int TypeBaseOffset = 8;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Foo
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public byte A;
	[FieldOffset(TestSettings.TypeBaseOffset + 1)] public short B;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Bar
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public int C;
	[FieldOffset(TestSettings.TypeBaseOffset + 4)] public long D;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Baz
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public byte A;
	[FieldOffset(TestSettings.TypeBaseOffset + 1)] public short B;
	[FieldOffset(TestSettings.TypeBaseOffset * 2 + 3)] public int C;
	[FieldOffset(TestSettings.TypeBaseOffset * 2 + 7)] public long D;
	[FieldOffset(TestSettings.TypeBaseOffset * 3 + 15)] public double E;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class B
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public long X;
	[FieldOffset(TestSettings.TypeBaseOffset + 8)] public short Y;
	[FieldOffset(TestSettings.TypeBaseOffset + 10)] public double Z;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class A
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public IntPtr B;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Holder
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public int Normal;
	[FieldOffset(TestSettings.TypeBaseOffset + 4)] public byte Tiny;
	[FieldOffset(TestSettings.TypeBaseOffset + 5)] public double Double;
	[FieldOffset(TestSettings.TypeBaseOffset + 13)] public long Large;
	[FieldOffset(TestSettings.TypeBaseOffset + 21)] public short Small;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Sequence
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public IntPtr Address;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Apple
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public long Weight;
	[FieldOffset(TestSettings.TypeBaseOffset + 8)] public double Price;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class Car
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public long Weight;
	[FieldOffset(TestSettings.TypeBaseOffset + 8)] public IntPtr Brand;
	[FieldOffset(TestSettings.TypeBaseOffset + 16)] public double Price;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[StructLayout(LayoutKind.Explicit)]
public class String
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public IntPtr Data;

	public static String From(IntPtr data)
	{
		return (String?)Marshal.PtrToStructure(data, typeof(String)) ?? throw new ApplicationException("Native code returned an invalid string object");
	}

	public String(string text)
	{
		Data = Marshal.AllocHGlobal(text.Length + 1);
		Marshal.Copy(Encoding.UTF8.GetBytes(text), 0, Data, text.Length);
		Marshal.WriteByte(Data, text.Length, 0);
	}

	public String() {}

	public void Assert(string expected)
	{
		var expected_bytes = Encoding.UTF8.GetBytes(expected);
		var actual_bytes = new byte[expected_bytes.Length];
		Marshal.Copy(Data, actual_bytes, 0, actual_bytes.Length);

		global::Assert.AreEqual(expected_bytes, actual_bytes);
		global::Assert.AreEqual((byte)0, Marshal.ReadByte(Data, expected_bytes.Length));
	}
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[SuppressMessage("Microsoft.Maintainability", "CA1815")]
[StructLayout(LayoutKind.Explicit)]
public class IterationArray
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public IntPtr Data;
	[FieldOffset(TestSettings.TypeBaseOffset + 8)] public long Count;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[SuppressMessage("Microsoft.Maintainability", "CA1815")]
[StructLayout(LayoutKind.Explicit)]
public class IterationRange
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public long Start;
	[FieldOffset(TestSettings.TypeBaseOffset + 8)] public long End;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[SuppressMessage("Microsoft.Maintainability", "CA1815")]
[StructLayout(LayoutKind.Explicit)]
public class IterationObject
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public double Value;
	[FieldOffset(TestSettings.TypeBaseOffset + 8)] public bool Flag;
}

[SuppressMessage("Microsoft.Maintainability", "CA1051")]
[SuppressMessage("Microsoft.Maintainability", "CA1815")]
[StructLayout(LayoutKind.Explicit)]
public struct MemoryObject
{
	[FieldOffset(TestSettings.TypeBaseOffset)] public int X;
	[FieldOffset(TestSettings.TypeBaseOffset + 4)] public double Y;
	[FieldOffset(TestSettings.TypeBaseOffset + 12)] public IntPtr Other;
}

namespace Vivid.Unit
{
	public static class AssemblerTests
	{
		public static int OptimizationLevel { get; set; } = 0;

		private static string Prefix => !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "libUnit_" : "Unit_";
		private static string ObjectFileExtension => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".obj" : ".o";

		private const string STANDARD_LIBRARY_FOLDER = "libv";
		private const string TESTS_CORE_FOLDER = "libv/tests";
		private static string PLATFORM_INTERNAL_FOLDER => STANDARD_LIBRARY_FOLDER + '/' + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows-x64/" : "linux-x64/");
		private const string TESTS = "Tests";

		private static string[] StandardLibraryUtility { get; set; } = Array.Empty<string>();


		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V10arithmeticxxx_rx(long a, long b, long c);

		[DllImport("Unit_Conditionals", ExactSpelling = true)]
		private static extern long _V12conditionalsxx_rx(long a, long b);

		[DllImport("Unit_Evacuation", ExactSpelling = true)]
		private static extern long _V10evacuationxx_rx(long a, long b);

		[DllImport("Unit_Evacuation", ExactSpelling = true)]
		private static extern long _V33basic_call_evacuation_with_memoryxx_rx(long a, long b);

		[DllImport("Unit_ConditionallyChangingConstant", ExactSpelling = true)]
		private static extern long _V49conditionally_changing_constant_with_if_statementxx_rx(long a, long b);

		[DllImport("Unit_ConditionallyChangingConstant", ExactSpelling = true)]
		private static extern long _V51conditionally_changing_constant_with_loop_statementxx_rx(long a, long b);

		[DllImport("Unit_ConstantPermanence", ExactSpelling = true)]
		private static extern void _V34constant_permanence_and_array_copyPhS_([MarshalAs(UnmanagedType.LPArray)] byte[] source, [MarshalAs(UnmanagedType.LPArray)] byte[] destination);

		[DllImport("Unit_Linkage", ExactSpelling = true)]
		private static extern long _V9linkage_1x_rx(long b);

		[DllImport("Unit_Linkage", ExactSpelling = true)]
		private static extern long _V9linkage_2x_rx(long b);

		[DllImport("Unit_Linkage", ExactSpelling = true)]
		private static extern long _V9linkage_3x_rx(long b);

		[DllImport("Unit_Linkage", ExactSpelling = true)]
		private static extern long _V9linkage_4x_rx(long b);

		[DllImport("Unit_Stack", ExactSpelling = true)]
		private static extern long _V12multi_returnxx_rx(long a, long b);

		[DllImport("Unit_RegisterUtilization", ExactSpelling = true)]
		private static extern long _V20register_utilizationxxxxxxx_rx(long a, long b, long c, long d, long e, long f, long g);

		[DllImport("Unit_SpecialMultiplications", ExactSpelling = true)]
		private static extern long _V23special_multiplicationsxx_rx(long a, long b);

		[DllImport("Unit_LargeFunctions", ExactSpelling = true)]
		private static extern long _V1xxx_rx(long a, long b);

		[DllImport("Unit_LargeFunctions", ExactSpelling = true)]
		private static extern double _V1yxx_rd(long a, long b);

		public static void Initialize()
		{
			StandardLibraryUtility = new[]
			{
				GetProjectFile("array.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("console.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("exceptions.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("list.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("math.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("memory-utility.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("sequential-iterator.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("sort.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("string-builder.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("string-utility.v", STANDARD_LIBRARY_FOLDER),
				GetProjectFile("string.v", STANDARD_LIBRARY_FOLDER)
			};
		}

		private static string GetExecutablePostfix()
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty;
		}

		private static char GetSeparator()
		{
			return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';
		}

		private static string GetProjectRoot()
		{
			return Directory.GetCurrentDirectory();
		}

		private static string GetProjectFolder(params string[] path)
		{
			return GetProjectRoot() + GetSeparator() + string.Join(GetSeparator(), path);
		}

		private static string GetProjectFile(string file, params string[] path)
		{
			var separator = GetSeparator();
			return GetProjectRoot() + separator + string.Join(separator, path) + separator + file;
		}

		/// <summary>
		/// Returns the mandatory object files, which must be linked together to create the executable
		/// </summary>
		private static string[] GetMinimumObjects()
		{
			return new[]
			{
				"minimum.math" + ObjectFileExtension,
				"minimum.memory" + ObjectFileExtension,
				"minimum.tests" + ObjectFileExtension
			};
		}

		private static bool Compile(string output, params string[] source_files)
		{
			// Configure the flow of the compiler
			var chain = new Chain
			(
				typeof(ConfigurationPhase),
				typeof(FilePhase),
				typeof(LexerPhase),
				typeof(ParserPhase),
				typeof(ResolverPhase),
				typeof(AssemblyPhase)
			);

			var files = source_files.Select(f => Path.IsPathRooted(f) ? f : GetProjectFile(f, TESTS)).ToArray();
			var arguments = new List<string>() { "-shared", "-assembly", "-f", "-o", Prefix + output };

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				arguments.Add("-l");
				arguments.Add("kernel32");
			}

			if (OptimizationLevel > 0)
			{
				arguments.Add("-O" + OptimizationLevel.ToString());
			}

			// Pack the program arguments in the chain
			var bundle = new Bundle();

			bundle.Put("arguments", arguments.Concat(files).Concat(new[]
			{
				GetProjectFile("core.v", TESTS_CORE_FOLDER),
				GetProjectFile("application.v", PLATFORM_INTERNAL_FOLDER),
				GetProjectFile("internal-console.v", PLATFORM_INTERNAL_FOLDER),
				GetProjectFile("internal-memory.v", PLATFORM_INTERNAL_FOLDER)
			}).Concat(GetMinimumObjects()).ToArray());

			// Execute the chain
			return chain.Execute(bundle);
		}

		private static bool CompileExecutable(string output, string[] source_files)
		{
			// Configure the flow of the compiler
			var chain = new Chain
			(
				typeof(ConfigurationPhase),
				typeof(FilePhase),
				typeof(LexerPhase),
				typeof(ParserPhase),
				typeof(ResolverPhase),
				typeof(AssemblyPhase)
			);

			var files = source_files.Select(i => Path.IsPathRooted(i) ? i : GetProjectFile(i, TESTS)).ToArray();
			var arguments = new List<string>() { "-assembly", "-f", "-o", Prefix + output };

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				arguments.Add("-l");
				arguments.Add("kernel32");
			}

			if (OptimizationLevel > 0)
			{
				// The condition depends on the constant boolean which is used manually to control the optimization of the tests
				// NOTE: This is practically redundant since this could be automated
				arguments.Add("-O" + OptimizationLevel.ToString());
			}

			// Pack the program arguments in the chain
			var bundle = new Bundle();

			bundle.Put("arguments", arguments.Concat(files).Concat(new[]
			{
				GetProjectFile("core.v", TESTS_CORE_FOLDER),
				GetProjectFile("application.v", PLATFORM_INTERNAL_FOLDER),
				GetProjectFile("internal-console.v", PLATFORM_INTERNAL_FOLDER),
				GetProjectFile("internal-memory.v", PLATFORM_INTERNAL_FOLDER)
			}).Concat(GetMinimumObjects()).ToArray());

			// Execute the chain
			return chain.Execute(bundle);
		}

		private static string Execute(string name)
		{
			var configuration = new ProcessStartInfo()
			{
				FileName = Prefix + name + GetExecutablePostfix(),
				WorkingDirectory = Environment.CurrentDirectory,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			try
			{
				var process = Process.Start(configuration) ?? throw new ApplicationException($"Could not start process '{configuration.FileName}'");
				process.WaitForExit();

				var output = process.StandardOutput.ReadToEnd();

				if (process.ExitCode != 0)
				{
					Assert.Fail("Executed process exited with non-zero code");
				}

				return output;
			}
			catch
			{
				Assert.Fail("Failed to execute");

				throw new Exception("Failed to execute");
			}
		}

		private static string LoadAssemblyOutput(string project)
		{
			return LoadAssemblyOutput(project, project);
		}

		private static string LoadAssemblyOutput(string project, string file)
		{
			return File.ReadAllText(Prefix + project + '.' + file + ".asm");
		}

		/// <summary>
		/// Loads the specified assembly output file and returns the section which represents the specified function
		/// </summary>
		private static string LoadAssemblyFunction(string output, string function)
		{
			var assembly = File.ReadAllText(Prefix + output + ".asm");
			var start = assembly.IndexOf(function + ':');
			var end = assembly.IndexOf("\n\n", start);

			if (start == -1 || end == -1)
			{
				Assert.Fail($"Could not load assembly function '{function}' from file '{Prefix + output + ".asm"}'");
			}

			return assembly[start..end];
		}

		/// <summary>
		/// Returns the section which represents the specified function
		/// </summary>
		private static string GetFunctionFromAssembly(string assembly, string function)
		{
			var start = assembly.IndexOf(function + ':');
			var end = assembly.IndexOf("\n\n", start);

			if (start == -1 || end == -1)
			{
				Assert.Fail($"Could not load assembly function '{function}' from '{assembly}'");
			}

			return assembly[start..end];
		}

		private static int GetCountOf(string assembly, string pattern)
		{
			var count = 0;

			foreach (var line in assembly.Split('\n'))
			{
				if (Regex.IsMatch(line, pattern))
				{
					count++;
				}
			}

			return count;
		}

		private static int GetMemoryAddressCount(string assembly)
		{
			var count = 0;

			foreach (var line in assembly.Split('\n'))
			{
				if (Regex.IsMatch(line, "\\[.*\\]") && !line.Contains(Instructions.X64.EVALUATE))
				{
					count++;
				}
			}

			return count;
		}

		private static void AssertNoMemoryAddress(string assembly)
		{
			foreach (var line in assembly.Split('\n'))
			{
				if (Regex.IsMatch(line, "\\[.*\\]") && !line.Contains(Instructions.X64.EVALUATE))
				{
					Assert.Fail("Assembly contained memory address(es)");
				}
			}
		}

		/// <summary>
		/// Fills the specified memory range with zeroes
		/// </summary>
		private static void Zero(IntPtr address, int bytes)
		{
			for (var i = 0; i < bytes; i++)
			{
				Marshal.WriteByte(address, i, 0);
			}
		}

		/// <summary>
		/// Allocates the specified amount of memory (filled with zeroes)
		/// </summary>
		private static IntPtr Allocate(int bytes)
		{
			var address = Marshal.AllocHGlobal(bytes);
			Zero(address, bytes);
			return address;
		}

		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V8additionxx_rx(long a, long b);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V11subtractionxx_rx(long a, long b);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V14multiplicationxx_rx(long a, long b);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V8divisionxx_rx(long a, long b);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V22addition_with_constantx_rx(long a);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V25subtraction_with_constantx_rx(long a);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V28multiplication_with_constantx_rx(long a);
		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V22division_with_constantx_rx(long a);

		private static void Arithmetic_Test()
		{
			var result = _V10arithmeticxxx_rx(6, 7, 9);

			Assert.AreEqual(42069, result);

			Assert.AreEqual(3, _V8additionxx_rx(1, 2));
			Assert.AreEqual(-90, _V11subtractionxx_rx(10, 100));
			Assert.AreEqual(49, _V14multiplicationxx_rx(7, 7));
			Assert.AreEqual(7, _V8divisionxx_rx(42, 6));

			Assert.AreEqual(64, _V22addition_with_constantx_rx(44));
			Assert.AreEqual(-1, _V25subtraction_with_constantx_rx(19));
			Assert.AreEqual(1300, _V28multiplication_with_constantx_rx(13));
			Assert.AreEqual(1, _V22division_with_constantx_rx(10));
		}

		public static void Arithmetic()
		{
			if (!Compile("Arithmetic", new[] { "Arithmetic.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Arithmetic_Test();
		}

		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V16decimal_additiondd_rd(double a, double b);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V19decimal_subtractiondd_rd(double a, double b);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V22decimal_multiplicationdd_rd(double a, double b);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V16decimal_divisiondd_rd(double a, double b);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V30decimal_addition_with_constantd_rd(double a);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V33decimal_subtraction_with_constantd_rd(double a);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V36decimal_multiplication_with_constantd_rd(double a);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V30decimal_division_with_constantd_rd(double a);
		[DllImport("Unit_Decimals", ExactSpelling = true)]
		private static extern double _V22decimal_operator_orderdd_rd(double a, double b);

		private static void Decimals_Test()
		{
			Assert.AreEqual(3.141 + 2.718, _V16decimal_additiondd_rd(3.141, 2.718));
			Assert.AreEqual(3.141 - 2.718, _V19decimal_subtractiondd_rd(3.141, 2.718));
			Assert.AreEqual(3.141 * 2.718, _V22decimal_multiplicationdd_rd(3.141, 2.718));
			Assert.AreEqual(3.141 / 2.718, _V16decimal_divisiondd_rd(3.141, 2.718));

			Assert.AreEqual(1.414 + 4.474 + 1.414, _V30decimal_addition_with_constantd_rd(4.474));
			Assert.AreEqual(-1.414 + 3.363 - 1.414, _V33decimal_subtraction_with_constantd_rd(3.363));
			Assert.AreEqual(1.414 * 2.252 * 1.414, _V36decimal_multiplication_with_constantd_rd(2.252));
			Assert.AreEqual(2.0 / 1.414 / 1.414, _V30decimal_division_with_constantd_rd(1.414));

			Assert.AreEqual(9.870 + 7.389 * 9.870 - 7.389 / 9.870, _V22decimal_operator_orderdd_rd(9.870, 7.389));
		}

		public static void Decimals()
		{
			if (!Compile("Decimals", new[] { "Decimals.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Decimals_Test();
		}

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern double _V27automatic_number_conversionx_rd(long a);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern double _V7casts_1x_rd(long a);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V7casts_2d_rx(double a);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern byte _V7casts_3x_rb(long a);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern IntPtr _V10create_bazv_rP3Baz();

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern IntPtr _V7casts_4d_rP3Baz(double a);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern IntPtr _V7casts_5P3Baz_rP3Foo(IntPtr baz);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern IntPtr _V7casts_6P3Baz_rP3Bar(IntPtr baz);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern IntPtr _V16automatic_cast_1P3Baz_rP3Bar(IntPtr baz);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern double _V16automatic_cast_2P3Baz_rd(IntPtr baz);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V22automatic_conversion_1Ph_rx(IntPtr bytes);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V22automatic_conversion_2Ps_rx(IntPtr shorts);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V22automatic_conversion_3Pi_rx(IntPtr ints);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V22automatic_conversion_4Px_rx(IntPtr longs);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V22automatic_conversion_5Pd_rx(IntPtr doubles);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V17assign_addition_1xxP1Ax_rx(long a, long b, IntPtr i, long j);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V20assign_subtraction_1xxP1Ax_rx(long a, long b, IntPtr i, long j);

		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V23assign_multiplication_1xxP1Ax_rx(long a, long b, IntPtr i, long j);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V17assign_division_1xxP1Ax_rx(long a, long b, IntPtr i, long j);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V18assign_remainder_1xxP1Ax_rx(long a, long b, IntPtr i, long j);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V20assign_bitwise_and_1xxP1Ax_rx(long a, long b, IntPtr i, long j);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V19assign_bitwise_or_1xxP1Ax_rx(long a, long b, IntPtr i, long j);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V20assign_bitwise_xor_1xxP1Ax_rx(long a, long b, IntPtr i, long j);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V23assign_multiplication_2xxxxP1AS0_S0_S0__rx(long a, long b, long c, long d, IntPtr i, IntPtr j, IntPtr k, IntPtr l);


		[DllImport("Unit_Conversions", ExactSpelling = true)]
		private static extern long _V17assign_division_2xxxxP1AS0_S0_S0__rx(long a, long b, long c, long d, IntPtr i, IntPtr j, IntPtr k, IntPtr l);

		private static void Conversions_Test()
		{
			Assert.AreEqual(6.0, _V27automatic_number_conversionx_rd(3));
			Assert.AreEqual(-15.0, _V27automatic_number_conversionx_rd(-15));
			Assert.AreEqual(1.0, _V27automatic_number_conversionx_rd(0));

			Assert.AreEqual(7.0, _V7casts_1x_rd(7));
			Assert.AreEqual(123, _V7casts_2d_rx(123.456));
			Assert.AreEqual(100, _V7casts_3x_rb(100));

			var result = Marshal.PtrToStructure<Baz>(_V7casts_4d_rP3Baz(100));
			Assert.AreEqual(100, result!.A);
			Assert.AreEqual(101, result.B);
			Assert.AreEqual(102, result.C);
			Assert.AreEqual(103, result.D);
			Assert.AreEqual(104.0, result.E);

			var baz = _V10create_bazv_rP3Baz();
			Assert.AreEqual(baz, _V7casts_5P3Baz_rP3Foo(baz));
			Assert.AreEqual(baz + TestSettings.TypeBaseOffset + 3, _V7casts_6P3Baz_rP3Bar(baz));

			Marshal.WriteInt64(baz, TestSettings.TypeBaseOffset * 3 + 15, BitConverter.DoubleToInt64Bits(-3.0)); // baz.e = -3.0
			Assert.AreNotEqual(baz + TestSettings.TypeBaseOffset + 3, _V16automatic_cast_1P3Baz_rP3Bar(baz));

			Marshal.WriteInt64(baz, TestSettings.TypeBaseOffset * 3 + 15, BitConverter.DoubleToInt64Bits(2.5)); // baz.e = 2.5
			Assert.AreEqual(baz + TestSettings.TypeBaseOffset + 3, _V16automatic_cast_1P3Baz_rP3Bar(baz));

			Marshal.WriteByte(baz, TestSettings.TypeBaseOffset, 10); // baz.a = 10
			Marshal.WriteInt16(baz, TestSettings.TypeBaseOffset + 1, 1000); // baz.b = 1000

			Marshal.WriteInt32(baz, TestSettings.TypeBaseOffset * 2 + 3, 505); // baz.c = 505
			Marshal.WriteInt64(baz, TestSettings.TypeBaseOffset * 2 + 7, 505); // baz.d = 505

			Assert.AreEqual(1010.0, _V16automatic_cast_2P3Baz_rd(baz));

			Marshal.WriteInt32(baz, TestSettings.TypeBaseOffset * 2 + 3, 0); // baz.c = 0
			Assert.AreEqual(505.0, _V16automatic_cast_2P3Baz_rd(baz));

			var bytes = BitConverter.GetBytes(3.14159);
			Marshal.WriteInt64(baz, 0, BitConverter.DoubleToInt64Bits(3.14159));

			Assert.AreEqual((long)bytes[0], _V22automatic_conversion_1Ph_rx(baz));
			Assert.AreEqual(BitConverter.ToInt16(bytes), _V22automatic_conversion_2Ps_rx(baz));
			Assert.AreEqual(BitConverter.ToInt32(bytes), _V22automatic_conversion_3Pi_rx(baz));
			Assert.AreEqual(BitConverter.ToInt64(bytes), _V22automatic_conversion_4Px_rx(baz));
			Assert.AreEqual(3L, _V22automatic_conversion_5Pd_rx(baz));

			Assert.AreEqual(0L, _V22automatic_conversion_1Ph_rx(IntPtr.Zero));
			Assert.AreEqual(0L, _V22automatic_conversion_2Ps_rx(IntPtr.Zero));
			Assert.AreEqual(0L, _V22automatic_conversion_3Pi_rx(IntPtr.Zero));
			Assert.AreEqual(0L, _V22automatic_conversion_4Px_rx(IntPtr.Zero));
			Assert.AreEqual(0L, _V22automatic_conversion_5Pd_rx(IntPtr.Zero));

			var b = Allocate(TestSettings.TypeBaseOffset + 18);
			Marshal.WriteInt64(b, TestSettings.TypeBaseOffset, 66);
			Marshal.WriteInt16(b, TestSettings.TypeBaseOffset + 8, 33);
			Marshal.WriteInt64(b, TestSettings.TypeBaseOffset + 10, BitConverter.DoubleToInt64Bits(99.99));

			var a = Allocate(TestSettings.TypeBaseOffset + 8);
			Marshal.WriteIntPtr(a, TestSettings.TypeBaseOffset, b);

			Assert.AreEqual(8L, _V17assign_addition_1xxP1Ax_rx(3, 5, a, 2));
			Assert.AreEqual(66L + 2, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(33L + 2, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(99.99 + 2, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(b, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(-13L, _V20assign_subtraction_1xxP1Ax_rx(-3, 10, a, 2));
			Assert.AreEqual(66L, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(33L, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(99.99, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(b, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(143, _V23assign_multiplication_1xxP1Ax_rx(11, 13, a, -144));
			Assert.AreEqual(66L * -144, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(33L * -144, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(99.99 * -144, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(b, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(-17, _V17assign_division_1xxP1Ax_rx(493, -29, a, -48));
			Assert.AreEqual(66L * 3, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(33L * 3, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(99.99 * -144 / -48, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(b, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(2, _V18assign_remainder_1xxP1Ax_rx(11, 3, a, 10));
			Assert.AreEqual(198 % 10, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(99 % 10, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));

			Assert.AreEqual((1010L << 16) | 101L, _V19assign_bitwise_or_1xxP1Ax_rx(1010L << 16, 101L, a, 0x4992));
			Assert.AreEqual(8L | 0x4992, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(9L | 0x4992, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));

			Assert.AreEqual((123L << 32) & (321 << 16), _V20assign_bitwise_and_1xxP1Ax_rx(123L << 32, 321 << 16, a, 0x2112));
			Assert.AreEqual((8L | 0x4992) & 0x2112, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual((9L | 0x4992) & 0x2112, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));

			Assert.AreEqual(1, _V20assign_bitwise_xor_1xxP1Ax_rx(0x3E7C, 0x3E7D, a, 0x0112));
			Assert.AreEqual(0L, Marshal.ReadInt64(b, TestSettings.TypeBaseOffset));
			Assert.AreEqual(0L, Marshal.ReadInt16(b, TestSettings.TypeBaseOffset + 8));

			var ib = Allocate(TestSettings.TypeBaseOffset + 20);
			Marshal.WriteInt64(ib, TestSettings.TypeBaseOffset, 9);
			Marshal.WriteInt16(ib, TestSettings.TypeBaseOffset + 8, 16);
			Marshal.WriteInt64(ib, TestSettings.TypeBaseOffset + 10, BitConverter.DoubleToInt64Bits(9.16));

			var i = Allocate(TestSettings.TypeBaseOffset + 8);
			Marshal.WriteIntPtr(i, TestSettings.TypeBaseOffset, ib);

			var jb = Allocate(TestSettings.TypeBaseOffset + 20);
			Marshal.WriteInt64(jb, TestSettings.TypeBaseOffset, 36);
			Marshal.WriteInt16(jb, TestSettings.TypeBaseOffset + 8, 49);
			Marshal.WriteInt64(jb, TestSettings.TypeBaseOffset + 10, BitConverter.DoubleToInt64Bits(36.49));

			var j = Allocate(TestSettings.TypeBaseOffset + 8);
			Marshal.WriteIntPtr(j, TestSettings.TypeBaseOffset, jb);

			var kb = Allocate(TestSettings.TypeBaseOffset + 20);
			Marshal.WriteInt64(kb, TestSettings.TypeBaseOffset, 2809);
			Marshal.WriteInt16(kb, TestSettings.TypeBaseOffset + 8, 2916);
			Marshal.WriteInt64(kb, TestSettings.TypeBaseOffset + 10, BitConverter.DoubleToInt64Bits(2809.2916));

			var k = Allocate(TestSettings.TypeBaseOffset + 8);
			Marshal.WriteIntPtr(k, TestSettings.TypeBaseOffset, kb);

			var lb = Allocate(TestSettings.TypeBaseOffset + 20);
			Marshal.WriteInt64(lb, TestSettings.TypeBaseOffset, 49);
			Marshal.WriteInt16(lb, TestSettings.TypeBaseOffset + 8, 36);
			Marshal.WriteInt64(lb, TestSettings.TypeBaseOffset + 10, BitConverter.DoubleToInt64Bits(49.36));

			var l = Allocate(TestSettings.TypeBaseOffset + 8);
			Marshal.WriteIntPtr(l, TestSettings.TypeBaseOffset, lb);

			Assert.AreEqual(9L * 2 * 36L * 5 * 2809L * 51 * 49L * -8, _V23assign_multiplication_2xxxxP1AS0_S0_S0__rx(9, 36, 2809, 49, i, j, k, l));

			Assert.AreEqual(9L * 2, Marshal.ReadInt64(ib, TestSettings.TypeBaseOffset));
			Assert.AreEqual(16L * 2, Marshal.ReadInt16(ib, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(9.16 * 2, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ib, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(36L * 5, Marshal.ReadInt64(jb, TestSettings.TypeBaseOffset));
			Assert.AreEqual(49L * 5, Marshal.ReadInt16(jb, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(36.49 * 5, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(jb, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(2809L * 51, Marshal.ReadInt64(kb, TestSettings.TypeBaseOffset));
			Assert.AreEqual(unchecked((short)(2916L * 51)), Marshal.ReadInt16(kb, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(2809.2916 * 51, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(kb, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(49L * -8, Marshal.ReadInt64(lb, TestSettings.TypeBaseOffset));
			Assert.AreEqual(36L * -8, Marshal.ReadInt16(lb, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(49.36 * -8, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(lb, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(50L * 8L * 7L * -8L, _V17assign_division_2xxxxP1AS0_S0_S0__rx(100, 40, 357, 64, i, j, k, l));

			Assert.AreEqual(9L, Marshal.ReadInt64(ib, TestSettings.TypeBaseOffset));
			Assert.AreEqual(16L, Marshal.ReadInt16(ib, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(9.16, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(ib, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(36L, Marshal.ReadInt64(jb, TestSettings.TypeBaseOffset));
			Assert.AreEqual(49L, Marshal.ReadInt16(jb, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(36.49, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(jb, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(2809L, Marshal.ReadInt64(kb, TestSettings.TypeBaseOffset));
			Assert.AreEqual((short)(unchecked((short)(2916L * 51)) / 51), Marshal.ReadInt16(kb, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(2809.2916 * 51.0 / 51.0, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(kb, TestSettings.TypeBaseOffset + 10)));

			Assert.AreEqual(49L, Marshal.ReadInt64(lb, TestSettings.TypeBaseOffset));
			Assert.AreEqual(36L, Marshal.ReadInt16(lb, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual(49.36, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(lb, TestSettings.TypeBaseOffset + 10)));
		}

		public static void Conversions()
		{
			if (!Compile("Conversions", new[] { "Conversions.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Conversions_Test();
		}

		private static void Conditionals_Test()
		{
			var result = _V12conditionalsxx_rx(100, 999);
			Assert.AreEqual(999, result);

			result = _V12conditionalsxx_rx(1, -1);
			Assert.AreEqual(1, result);

			result = _V12conditionalsxx_rx(777, 777);
			Assert.AreEqual(777, result);
		}

		public static void Conditionals()
		{
			if (!Compile("Conditionals", new[] { "Conditionals.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Conditionals_Test();
		}

		[DllImport("Unit_Loops", ExactSpelling = true)]
		private static extern long _V5loopsxx_rx(long start, long count);

		[DllImport("Unit_Loops", ExactSpelling = true)]
		private static extern long _V16nested_for_loopsPhx_rx([MarshalAs(UnmanagedType.LPArray)] byte[] destination, long width);

		[DllImport("Unit_Loops", ExactSpelling = true)]
		private static extern long _V16conditional_loopx_rx(long start);

		[DllImport("Unit_Loops", ExactSpelling = true)]
		private static extern long _V23conditional_action_loopx_rx(long start);

		[DllImport("Unit_Loops", ExactSpelling = true)]
		private static extern long _V15normal_for_loopxx_rx(long start, long count);

		[DllImport("Unit_Loops", ExactSpelling = true)]
		private static extern long _V25normal_for_loop_with_stopxx_rx(long start, long count);

		private static void Loops_Test()
		{
			Assert.AreEqual(100, _V5loopsxx_rx(70, 5));

			Assert.AreEqual(10, _V16conditional_loopx_rx(3));
			Assert.AreEqual(1344, _V23conditional_action_loopx_rx(42));
			Assert.AreEqual(3169, _V15normal_for_loopxx_rx(3141, 8));

			Assert.AreEqual(220, _V25normal_for_loop_with_stopxx_rx(10, 20));
			Assert.AreEqual(3, _V25normal_for_loop_with_stopxx_rx(-3, 3));
			Assert.AreEqual(10, _V25normal_for_loop_with_stopxx_rx(10, -1));
			Assert.AreEqual(-1, _V25normal_for_loop_with_stopxx_rx(0, 999));

			var expected = new byte[] { 100, 0, 100, 0, 0, 0, 100, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 100, 0, 0, 0, 100, 0, 100 };

			var actual = new byte[27];
			var w = _V16nested_for_loopsPhx_rx(actual, 3);

			Assert.AreEqual(expected, actual);
			Assert.AreEqual(13, w);
		}

		public static void Loops()
		{
			if (!Compile("Loops", new[] { "Loops.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Loops_Test();
		}

		private static void Evacuation_Test()
		{
			Assert.AreEqual(570, _V10evacuationxx_rx(10, 50));
		}

		public static void Evacuation()
		{
			if (!Compile("Evacuation", new[] { "Evacuation.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Evacuation_Test();
		}

		[DllImport("Unit_Assignment", ExactSpelling = true)]
		private static extern void _V12assignment_1P6Holder(Holder instance);

		[DllImport("Unit_Assignment", ExactSpelling = true)]
		private static extern void _V12assignment_2P8Sequence(Sequence instance);

		private static void Assignment_Test()
		{
			var holder = new Holder();
			_V12assignment_1P6Holder(holder);

			Assert.AreEqual(64, holder.Tiny);
			Assert.AreEqual(12345, holder.Small);
			Assert.AreEqual(314159265, holder.Normal);
			Assert.AreEqual(-2718281828459045, holder.Large);
			Assert.AreEqual(1.414, holder.Double);

			var buffer = Allocate(sizeof(double) * 3);
			Marshal.WriteInt64(buffer, sizeof(double) * 0, BitConverter.DoubleToInt64Bits(0.0));
			Marshal.WriteInt64(buffer, sizeof(double) * 1, BitConverter.DoubleToInt64Bits(0.0));
			Marshal.WriteInt64(buffer, sizeof(double) * 2, BitConverter.DoubleToInt64Bits(0.0));

			var sequence = new Sequence() { Address = buffer };
			_V12assignment_2P8Sequence(sequence);

			Assert.AreEqual(BitConverter.DoubleToInt64Bits(-123.456), Marshal.ReadInt64(sequence.Address, sizeof(double) * 0));
			Assert.AreEqual(BitConverter.DoubleToInt64Bits(-987.654), Marshal.ReadInt64(sequence.Address, sizeof(double) * 1));
			Assert.AreEqual(BitConverter.DoubleToInt64Bits(101.010), Marshal.ReadInt64(sequence.Address, sizeof(double) * 2));
		}

		public static void Assignment()
		{
			if (!Compile("Assignment", new[] { "Assignment.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Assignment_Test();
		}

		private static void ConditionallyChangingConstant_Test()
		{
			var result = _V49conditionally_changing_constant_with_if_statementxx_rx(10, 20);
			Assert.AreEqual(17, result);

			result = _V49conditionally_changing_constant_with_if_statementxx_rx(10, 0);
			Assert.AreEqual(10 * 2, result);

			result = _V51conditionally_changing_constant_with_loop_statementxx_rx(3, 2);
			Assert.AreEqual(2 * 100, result);

			result = _V51conditionally_changing_constant_with_loop_statementxx_rx(2, 5);
			Assert.AreEqual(5 * 103, result);
		}

		public static void ConditionallyChangingConstant()
		{
			if (!Compile("ConditionallyChangingConstant", new[] { "ConditionallyChangingConstant.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			ConditionallyChangingConstant_Test();
		}

		private static void ConstantPermanenceAndArrayCopy_Test()
		{
			var source = new byte[] { 1, 3, 5, 7, 11, 13, 15, 17, 19, 23, 29, 31, 33, 37 };
			var destination = new byte[14];

			_V34constant_permanence_and_array_copyPhS_(source, destination);

			// Check whether the array copy with offset succeeded
			Assert.AreEqual(new byte[] { 0, 0, 0, 7, 11, 13, 15, 17, 19, 23, 29, 31, 33, 0 }, destination);

			var assembly = LoadAssemblyOutput("ConstantPermanence");
			Assert.True(Regex.IsMatch(assembly, "\\[\\w+\\+3\\]"));
		}

		public static void ConstantPermanence()
		{
			if (!Compile("ConstantPermanence", new[] { "ConstantPermanence.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			if (Assembler.IsArm64)
			{
				Assert.Pass("Constant permanence is not tested on arhitecture Arm64");
			}

			ConstantPermanenceAndArrayCopy_Test();
		}

		private static void Linkage_Test()
		{
			var b = 42;
			Assert.AreEqual(2 * b + 1, _V9linkage_1x_rx(b));
			Assert.AreEqual(2 * b + 2, _V9linkage_2x_rx(b));
			Assert.AreEqual(5, _V9linkage_3x_rx(b));
			Assert.AreEqual(4 * b + 75, _V9linkage_4x_rx(b));
		}

		public static void Linkage()
		{
			if (!Compile("Linkage", new[] { "Linkage.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Linkage_Test();
		}

		private static void Pi_Test()
		{
			string actual = Execute("PI");
			string expected = File.ReadAllText(GetProjectFile("Digits.txt", TESTS));

			Assert.AreEqual(expected, actual);
		}

		public static void Pi()
		{
			if (!CompileExecutable("PI", new[] { "PI.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Pi_Test();
		}

		private static void Fibonacci_Test()
		{
			string actual = Execute("Fibonacci");
			string expected = File.ReadAllText(GetProjectFile("Fibonacci_Output.txt", TESTS)).Replace("\r\n", "\n");

			Assert.AreEqual(expected, actual);
		}

		public static void Fibonacci()
		{
			if (!CompileExecutable("Fibonacci", new[] { "Fibonacci.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Fibonacci_Test();
		}

		private static void Stack_Test()
		{
			Assert.AreEqual(1, _V12multi_returnxx_rx(7, 1));
			Assert.AreEqual(0, _V12multi_returnxx_rx(-1, -1));
			Assert.AreEqual(-1, _V12multi_returnxx_rx(5, 20));

			var assembly = LoadAssemblyFunction("Stack.Stack", "_V12multi_returnxx_rx");
			var j = 0;

			// There should be five 'add rsp, 40' or 'ldp' instructions
			for (var i = 0; i < 4; i++)
			{
				j = assembly.IndexOf(Assembler.IsArm64 ? "ldp" : "add rsp, ", j, StringComparison.Ordinal);

				if (j++ == -1)
				{
					Assert.Fail("Warning: Assembly output did not contain five 'add rsp, 40' or 'ldp' instructions");
				}
			}
		}

		public static void Stack()
		{
			if (!Compile("Stack", new[] { "Stack.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Stack_Test();
		}

		private static void RegisterUtilization_Test()
		{
			Assert.AreEqual(-10799508, _V20register_utilizationxxxxxxx_rx(90, 7, 1, 1, 1, 1, 1));

			// Ensure the assembly function has exactly one memory address since otherwise the compiler would not be utilizing registers as much as it should
			Assert.AreEqual(1, GetMemoryAddressCount(LoadAssemblyFunction("RegisterUtilization.RegisterUtilization", "_V20register_utilizationxxxxxxx_rx")));
		}

		public static void RegisterUtilization()
		{
			if (!Compile("RegisterUtilization", new[] { "RegisterUtilization.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			if (Assembler.IsArm64)
			{
				Assert.Pass("TODO");
			}

			RegisterUtilization_Test();
		}

		public static void SpecialMultiplications()
		{
			if (!Compile("SpecialMultiplications", "SpecialMultiplications.v"))
			{
				Assert.Fail("Failed to compile");
			}

			Assert.AreEqual(1802, _V23special_multiplicationsxx_rx(7, 100));

			// The last part of this test is supposed to run when optimization is disabled
			if (OptimizationLevel > 0)
			{
				Assert.Pass("Special multiplications are not tested when optimization is enabled");
				return;
			}

			var assembly = LoadAssemblyOutput("SpecialMultiplications");

			if (Assembler.IsX64)
			{
				Assert.AreEqual(1, GetCountOf(assembly, "mul\\ [a-z]+"));
				Assert.AreEqual(1, GetCountOf(assembly, "sal\\ [a-z]+"));
				Assert.AreEqual(1, GetCountOf(assembly, "lea\\ [a-z]+"));
				Assert.AreEqual(1, GetCountOf(assembly, "sar\\ [a-z]+"));
			}
			else
			{
				Assert.AreEqual(1, GetCountOf(assembly, "lsl.+#1"));
				Assert.AreEqual(2, GetCountOf(assembly, "add.+lsl\\ #"));
				Assert.AreEqual(1, GetCountOf(assembly, "asr.+#2"));
			}
		}

		private static void LargeFunctions_Test()
		{
			Assert.AreEqual(197, _V1xxx_rx(26, 16));
			Assert.AreEqual(414.414, _V1yxx_rd(8, 13));
		}

		public static void LargeFunctions()
		{
			if (!Compile("LargeFunctions", new[] { "LargeFunctions.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			LargeFunctions_Test();
		}

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V19logical_operators_1xx_rx(long a, long b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern bool _V14single_booleanb_rb(bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V12two_booleansbb_rx(bool a, bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern bool _V20nested_if_statementsxxx_rb(long a, long b, long c);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V27logical_and_in_if_statementbb_rx(bool a, bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V26logical_or_in_if_statementbb_rx(bool a, bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V25nested_logical_statementsbbbb_rx(bool a, bool b, bool c, bool d);

		private static void LogicalOperators_Test()
		{
			// Single boolean as input
			Assert.False(_V14single_booleanb_rb(true));
			Assert.True(_V14single_booleanb_rb(false));

			// Two booleans as input
			Assert.AreEqual(1, _V12two_booleansbb_rx(true, false));
			Assert.AreEqual(2, _V12two_booleansbb_rx(false, true));
			Assert.AreEqual(3, _V12two_booleansbb_rx(false, false));

			// Nested if-statement:

			// All correct inputs
			Assert.True(_V20nested_if_statementsxxx_rb(1, 2, 3));
			Assert.True(_V20nested_if_statementsxxx_rb(1, 2, 4));
			Assert.True(_V20nested_if_statementsxxx_rb(1, 0, 1));
			Assert.True(_V20nested_if_statementsxxx_rb(1, 0, -1));

			Assert.True(_V20nested_if_statementsxxx_rb(2, 4, 8));
			Assert.True(_V20nested_if_statementsxxx_rb(2, 4, 6));
			Assert.True(_V20nested_if_statementsxxx_rb(2, 3, 4));
			Assert.True(_V20nested_if_statementsxxx_rb(2, 3, 5));

			// Most of the paths for returning false
			Assert.False(_V20nested_if_statementsxxx_rb(0, 0, 0));

			Assert.False(_V20nested_if_statementsxxx_rb(1, 1, 1));
			Assert.False(_V20nested_if_statementsxxx_rb(1, 2, 5));
			Assert.False(_V20nested_if_statementsxxx_rb(1, 0, 0));

			Assert.False(_V20nested_if_statementsxxx_rb(2, 0, 0));
			Assert.False(_V20nested_if_statementsxxx_rb(2, 4, 7));
			Assert.False(_V20nested_if_statementsxxx_rb(2, 3, 6));

			// Logical and
			Assert.AreEqual(10, _V27logical_and_in_if_statementbb_rx(true, true));
			Assert.AreEqual(0, _V27logical_and_in_if_statementbb_rx(true, false));
			Assert.AreEqual(0, _V27logical_and_in_if_statementbb_rx(false, true));
			Assert.AreEqual(0, _V27logical_and_in_if_statementbb_rx(false, false));

			// Logical or
			Assert.AreEqual(10, _V26logical_or_in_if_statementbb_rx(true, true));
			Assert.AreEqual(10, _V26logical_or_in_if_statementbb_rx(true, false));
			Assert.AreEqual(10, _V26logical_or_in_if_statementbb_rx(false, true));
			Assert.AreEqual(0, _V26logical_or_in_if_statementbb_rx(false, false));

			// Nested logical statements
			Assert.AreEqual(1, _V25nested_logical_statementsbbbb_rx(true, true, true, true));
			Assert.AreEqual(2, _V25nested_logical_statementsbbbb_rx(false, true, true, true));
			Assert.AreEqual(2, _V25nested_logical_statementsbbbb_rx(true, false, true, true));
			Assert.AreEqual(3, _V25nested_logical_statementsbbbb_rx(true, true, false, true));
			Assert.AreEqual(3, _V25nested_logical_statementsbbbb_rx(true, true, true, false));
			Assert.AreEqual(4, _V25nested_logical_statementsbbbb_rx(true, true, false, false));
			Assert.AreEqual(4, _V25nested_logical_statementsbbbb_rx(false, false, true, true));
			Assert.AreEqual(5, _V25nested_logical_statementsbbbb_rx(true, false, false, false));
			Assert.AreEqual(5, _V25nested_logical_statementsbbbb_rx(false, true, false, false));
			Assert.AreEqual(5, _V25nested_logical_statementsbbbb_rx(false, false, true, false));
			Assert.AreEqual(5, _V25nested_logical_statementsbbbb_rx(false, false, false, true));
			Assert.AreEqual(6, _V25nested_logical_statementsbbbb_rx(false, false, false, false));

			Assert.AreEqual(5, _V19logical_operators_1xx_rx(10, 5));
			Assert.AreEqual(7, _V19logical_operators_1xx_rx(0, 7));
			Assert.AreEqual(1, _V19logical_operators_1xx_rx(1, 1));
			Assert.AreEqual(0, _V19logical_operators_1xx_rx(3, 3));
		}

		public static void LogicalOperators()
		{
			if (!Compile("LogicalOperators", new[] { "LogicalOperators.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			LogicalOperators_Test();
		}


		[DllImport("Unit_Objects", ExactSpelling = true)]
		private static extern IntPtr _V12create_applev_rP5Apple();

		[DllImport("Unit_Objects", ExactSpelling = true)]
		private static extern IntPtr _V10create_card_rP3Car(double price);

		private static void Objects_Test()
		{
			var apple = (Apple)Marshal.PtrToStructure(_V12create_applev_rP5Apple(), typeof(Apple))!;

			Assert.AreEqual(100, apple.Weight);
			Assert.AreEqual(0.1, apple.Price);

			var car = (Car)Marshal.PtrToStructure(_V10create_card_rP3Car(20000), typeof(Car))!;

			Assert.AreEqual(2000000, car.Weight);
			Assert.AreEqual(20000.0, car.Price);

			var brand = (String)Marshal.PtrToStructure(car.Brand, typeof(String))!;

			brand.Assert("Flash");
		}

		public static void Objects()
		{
			if (!Compile("Objects", new[] { "Objects.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Objects_Test();
		}

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern IntPtr _V13create_bundlev_rP6BundleIP7ProductP5PriceE();

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern IntPtr _V11set_productP6BundleIP7ProductP5PriceExPhxc(IntPtr bundle, long index, IntPtr name, long value, byte currency);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern IntPtr _V16get_product_nameP6BundleIP7ProductP5PriceEx_rP6String(IntPtr bundle, long index);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern void _V15enchant_productP6BundleIP7ProductP5PriceEx(IntPtr bundle, long index);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern bool _V20is_product_enchantedP6BundleIP7ProductP5PriceEx_rb(IntPtr bundle, long index);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern double _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(IntPtr bundle, long index, byte currency);

		private static void Templates_Test()
		{
			var bundle = _V13create_bundlev_rP6BundleIP7ProductP5PriceE();

			var car = new String("Car");
			var banana = new String("Banana");
			var lawnmower = new String("Lawnmower");

			const int EUROS = 0;
			const int DOLLARS = 1;

			// fsetproduct_a_tproduct_a_tprice_tpack_tlarge_tlink_ttiny
			_V11set_productP6BundleIP7ProductP5PriceExPhxc(bundle, 0, car.Data, 700000, EUROS);
			_V11set_productP6BundleIP7ProductP5PriceExPhxc(bundle, 2, lawnmower.Data, 40000, DOLLARS);
			_V11set_productP6BundleIP7ProductP5PriceExPhxc(bundle, 1, banana.Data, 100, DOLLARS);

			String.From(_V16get_product_nameP6BundleIP7ProductP5PriceEx_rP6String(bundle, 0)).Assert("Car");
			String.From(_V16get_product_nameP6BundleIP7ProductP5PriceEx_rP6String(bundle, 1)).Assert("Banana");
			String.From(_V16get_product_nameP6BundleIP7ProductP5PriceEx_rP6String(bundle, 2)).Assert("Lawnmower");

			_V15enchant_productP6BundleIP7ProductP5PriceEx(bundle, 0);
			_V15enchant_productP6BundleIP7ProductP5PriceEx(bundle, 1);

			Assert.True(_V20is_product_enchantedP6BundleIP7ProductP5PriceEx_rb(bundle, 0));
			Assert.True(_V20is_product_enchantedP6BundleIP7ProductP5PriceEx_rb(bundle, 1));
			Assert.False(_V20is_product_enchantedP6BundleIP7ProductP5PriceEx_rb(bundle, 2));

			String.From(_V16get_product_nameP6BundleIP7ProductP5PriceEx_rP6String(bundle, 0)).Assert("iCar");
			String.From(_V16get_product_nameP6BundleIP7ProductP5PriceEx_rP6String(bundle, 1)).Assert("iBanana");

			Assert.AreEqual(700000.0, _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(bundle, 0, EUROS));
			Assert.AreEqual(100.0 * 0.8, _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(bundle, 1, EUROS));
			Assert.AreEqual(40000.0 * 0.8, _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(bundle, 2, EUROS));

			Assert.AreEqual(700000.0 * 1.25, _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(bundle, 0, DOLLARS));
			Assert.AreEqual(100.0, _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(bundle, 1, DOLLARS));
			Assert.AreEqual(40000.0, _V17get_product_priceP6BundleIP7ProductP5PriceExc_rd(bundle, 2, DOLLARS));
		}

		public static void Templates()
		{
			if (!Compile("Templates", new[] { "Templates.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Templates_Test();
		}

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V11bitwise_andcc_rc(byte a, byte b);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V11bitwise_xorcc_rc(byte a, byte b);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V10bitwise_orcc_rc(byte a, byte b);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V13synthetic_andcc_rc(byte a, byte b);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V13synthetic_xorcc_rc(byte a, byte b);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V12synthetic_orcc_rc(byte a, byte b);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V18assign_bitwise_andx_rx(long a);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V18assign_bitwise_xorx_rx(long a);

		[DllImport("Unit_BitwiseOperations", ExactSpelling = true)]
		private static extern long _V17assign_bitwise_orxx_rx(long a, long b);

		private static void BitwiseOperations_Test()
		{
			Assert.AreEqual(1, _V11bitwise_andcc_rc(1, 1));
			Assert.AreEqual(0, _V11bitwise_andcc_rc(1, 0));
			Assert.AreEqual(0, _V11bitwise_andcc_rc(0, 1));
			Assert.AreEqual(0, _V11bitwise_andcc_rc(0, 0));

			Assert.AreEqual(0, _V11bitwise_xorcc_rc(1, 1));
			Assert.AreEqual(1, _V11bitwise_xorcc_rc(1, 0));
			Assert.AreEqual(1, _V11bitwise_xorcc_rc(0, 1));
			Assert.AreEqual(0, _V11bitwise_xorcc_rc(0, 0));

			Assert.AreEqual(1, _V10bitwise_orcc_rc(1, 1));
			Assert.AreEqual(1, _V10bitwise_orcc_rc(1, 0));
			Assert.AreEqual(1, _V10bitwise_orcc_rc(0, 1));
			Assert.AreEqual(0, _V10bitwise_orcc_rc(0, 0));

			Assert.AreEqual(1, _V13synthetic_andcc_rc(1, 1));
			Assert.AreEqual(0, _V13synthetic_andcc_rc(1, 0));
			Assert.AreEqual(0, _V13synthetic_andcc_rc(0, 1));
			Assert.AreEqual(0, _V13synthetic_andcc_rc(0, 0));

			Assert.AreEqual(0, _V13synthetic_xorcc_rc(1, 1));
			Assert.AreEqual(1, _V13synthetic_xorcc_rc(1, 0));
			Assert.AreEqual(1, _V13synthetic_xorcc_rc(0, 1));
			Assert.AreEqual(0, _V13synthetic_xorcc_rc(0, 0));

			Assert.AreEqual(1, _V12synthetic_orcc_rc(1, 1));
			Assert.AreEqual(1, _V12synthetic_orcc_rc(1, 0));
			Assert.AreEqual(1, _V12synthetic_orcc_rc(0, 1));
			Assert.AreEqual(0, _V12synthetic_orcc_rc(0, 0));

			// 111 & 011 = 11
			Assert.AreEqual(3, _V18assign_bitwise_andx_rx(7));

			// 10101 ¤ 00001 = 10100
			Assert.AreEqual(20, _V18assign_bitwise_xorx_rx(21));

			// 10101 ¤ 00001 = 10100
			Assert.AreEqual(96, _V17assign_bitwise_orxx_rx(32, 64));
		}

		public static void BitwiseOperations()
		{
			if (!Compile("BitwiseOperations", new[] { "BitwiseOperations.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			BitwiseOperations_Test();
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Animal
		{
			[FieldOffset(TestSettings.TypeBaseOffset)] public short energy;
			[FieldOffset(TestSettings.TypeBaseOffset + 2)] public byte hunger;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Fish
		{
			[FieldOffset(TestSettings.TypeBaseOffset)] public short speed;
			[FieldOffset(TestSettings.TypeBaseOffset + 2)] public short velocity;
			[FieldOffset(TestSettings.TypeBaseOffset + 4)] public short weight;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Salmon
		{
			[FieldOffset(TestSettings.TypeBaseOffset)] public short energy;
			[FieldOffset(TestSettings.TypeBaseOffset + 2)] public byte hunger;
			[FieldOffset(TestSettings.TypeBaseOffset * 2 + 3)] public short speed;
			[FieldOffset(TestSettings.TypeBaseOffset * 2 + 5)] public short velocity;
			[FieldOffset(TestSettings.TypeBaseOffset * 2 + 7)] public short weight;
			[FieldOffset(TestSettings.TypeBaseOffset * 3 + 9)] public bool is_hiding;
		}

		private static Salmon GetSalmon(IntPtr pointer)
		{
			return Marshal.PtrToStructure<Salmon>(pointer);
		}

		private static IntPtr GetFishPointer(IntPtr salmon)
		{
			return salmon + TestSettings.TypeBaseOffset + 3;
		}

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V10get_animalv_rP6Animal();

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V8get_fishv_rP4Fish();

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V10get_salmonv_rP6Salmon();

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V12animal_movesP6Animal(IntPtr address);

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V10fish_movesP4Fish(IntPtr address);

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V10fish_swimsP6Animal(IntPtr address);

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V10fish_stopsP6Animal(IntPtr address);

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V10fish_hidesP6Salmon(IntPtr address);

		[DllImport("Unit_Inheritance", ExactSpelling = true)]
		private static extern IntPtr _V17fish_stops_hidingP6Salmon(IntPtr address);

		private static void Inheritance_Test()
		{
			var animal = Marshal.PtrToStructure<Animal>(_V10get_animalv_rP6Animal());
			Assert.AreEqual(100, animal.energy);
			Assert.AreEqual(0, animal.hunger);

			var fish = Marshal.PtrToStructure<Fish>(_V8get_fishv_rP4Fish());
			Assert.AreEqual(1, fish.speed);
			Assert.AreEqual(0, fish.velocity);
			Assert.AreEqual(1500, fish.weight);

			var salmon_pointer = _V10get_salmonv_rP6Salmon();
			var salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(false, salmon.is_hiding);
			Assert.AreEqual(5000, salmon.weight);

			_V12animal_movesP6Animal(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(99, salmon.energy);
			Assert.AreEqual(1, salmon.hunger);

			_V10fish_movesP4Fish(GetFishPointer(salmon_pointer));
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(5, salmon.velocity);
			Assert.AreEqual(98, salmon.energy);
			Assert.AreEqual(2, salmon.hunger);

			_V10fish_swimsP6Animal(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(5, salmon.velocity);
			Assert.AreEqual(97, salmon.energy);
			Assert.AreEqual(3, salmon.hunger);

			_V10fish_stopsP6Animal(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(0, salmon.velocity);

			_V10fish_hidesP6Salmon(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(0, salmon.velocity);
			Assert.AreEqual(96, salmon.energy);
			Assert.AreEqual(4, salmon.hunger);
			Assert.AreEqual(true, salmon.is_hiding);

			// The fish should not move since it is hiding
			_V10fish_movesP4Fish(GetFishPointer(salmon_pointer));
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(0, salmon.velocity);
			Assert.AreEqual(96, salmon.energy);
			Assert.AreEqual(4, salmon.hunger);
			Assert.AreEqual(true, salmon.is_hiding);

			_V17fish_stops_hidingP6Salmon(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(5, salmon.velocity);
			Assert.AreEqual(94, salmon.energy);
			Assert.AreEqual(6, salmon.hunger);
			Assert.AreEqual(false, salmon.is_hiding);
		}

		public static void Inheritance()
		{
			if (!Compile("Inheritance", new[] { "Inheritance.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Inheritance_Test();
		}

		[DllImport("Unit_Scopes", ExactSpelling = true)]
		private static extern long _V27scopes_nested_if_statementsxxxxxxxx_rx(long a, long b, long c, long d, long e, long f, long g, long h);

		[DllImport("Unit_Scopes", ExactSpelling = true)]
		private static extern long _V18scopes_single_loopxxxxxxxx_rx(long a, long b, long c, long d, long e, long f, long g, long h);

		[DllImport("Unit_Scopes", ExactSpelling = true)]
		private static extern long _V19scopes_nested_loopsxxxxxxxx_rx(long a, long b, long c, long d, long e, long f, long g, long h);

		private static long GetExpectedReturnValue(long a, long b, long c, long d, long e, long f, long g, long h)
		{
			var x = 2 * a;
			var y = 3 * b;
			var z = 5 * c;

			return (a + b + c + d + e + f + g + h) * x * y * z;
		}

		private static void Scopes_Test()
		{
			Assert.AreEqual(GetExpectedReturnValue(1, 2, 3, 4, 5, 6, 7, 8), _V27scopes_nested_if_statementsxxxxxxxx_rx(1, 2, 3, 4, 5, 6, 7, 8));
			Assert.AreEqual(GetExpectedReturnValue(10, 20, -30, 40, 50, 60, 70, 80), _V27scopes_nested_if_statementsxxxxxxxx_rx(10, 20, -30, 40, 50, 60, 70, 80));
			Assert.AreEqual(GetExpectedReturnValue(-2, 4, 6, 8, 10, 12, 14, 16), _V27scopes_nested_if_statementsxxxxxxxx_rx(-2, 4, 6, 8, 10, 12, 14, 16));
			Assert.AreEqual(GetExpectedReturnValue(-20, 40, 60, -80, 100, 120, 140, 160), _V27scopes_nested_if_statementsxxxxxxxx_rx(-20, 40, 60, -80, 100, 120, 140, 160));
			Assert.AreEqual(GetExpectedReturnValue(-3, -5, 9, 11, 13, 17, 19, 23), _V27scopes_nested_if_statementsxxxxxxxx_rx(-3, -5, 9, 11, 13, 17, 19, 23));
			Assert.AreEqual(GetExpectedReturnValue(-30, -50, 90, 110, -130, 170, 190, 230), _V27scopes_nested_if_statementsxxxxxxxx_rx(-30, -50, 90, 110, -130, 170, 190, 230));

			Assert.AreEqual(GetExpectedReturnValue(7, 8, 11, 16, 23, 32, 43, 56), _V18scopes_single_loopxxxxxxxx_rx(7, 8, 11, 16, 23, 32, 43, 56));

			Assert.AreEqual(GetExpectedReturnValue(7, 8, 11, 16, 23, 32, 43, 56), _V19scopes_nested_loopsxxxxxxxx_rx(7, 8, 11, 16, 23, 32, 43, 56));
		}

		public static void Scopes()
		{
			if (!Compile("Scopes", new[] { "Scopes.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Scopes_Test();
		}

		private static void Lambdas_Test()
		{
			string actual = Execute("Lambdas");
			string expected = File.ReadAllText(GetProjectFile("Lambdas.txt", TESTS));

			Assert.AreEqual(expected, actual);
		}

		public static void Lambdas()
		{
			if (!CompileExecutable("Lambdas", new[] { "Lambdas.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Lambdas_Test();
		}

		private static void Virtuals_Test()
		{
			string actual = Execute("Virtuals");
			string expected = File.ReadAllText(GetProjectFile("Virtuals.txt", TESTS));

			Assert.AreEqual(expected, actual);
		}

		public static void Virtuals()
		{
			if (!CompileExecutable("Virtuals", new[] { "Virtuals.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Virtuals_Test();
		}

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern long _V14numerical_whenx_rx(long x);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern IntPtr _V13create_stringPhx_rP6String(StringBuilder characters, long length);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern long _V11string_whenP6String_rx(IntPtr text);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern IntPtr _V10create_boov_rP3Boo();

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern IntPtr _V11create_babax_rP4Baba(long x);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern IntPtr _V10create_buix_rP3Bui(long x);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern IntPtr _V14create_bababuixx_rP7Bababui(long x, long y);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern long _V7is_whenP3Boo_rx(IntPtr boo);

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern long _V10range_whenx_rx(long x);

		private static void Whens_Test()
		{
			Assert.AreEqual(49, _V14numerical_whenx_rx(7));
			Assert.AreEqual(9, _V14numerical_whenx_rx(3));
			Assert.AreEqual(-1, _V14numerical_whenx_rx(1));

			Assert.AreEqual(42, _V14numerical_whenx_rx(42));
			Assert.AreEqual(-100, _V14numerical_whenx_rx(-100));
			Assert.AreEqual(0, _V14numerical_whenx_rx(0));

			Assert.AreEqual(0, _V11string_whenP6String_rx(_V13create_stringPhx_rP6String(new StringBuilder("Foo"), 3)));
			Assert.AreEqual(1, _V11string_whenP6String_rx(_V13create_stringPhx_rP6String(new StringBuilder("Bar"), 3)));
			Assert.AreEqual(2, _V11string_whenP6String_rx(_V13create_stringPhx_rP6String(new StringBuilder("Baz"), 3)));
			Assert.AreEqual(-1, _V11string_whenP6String_rx(_V13create_stringPhx_rP6String(new StringBuilder("Bababui"), 7)));
		
			var boo = _V10create_boov_rP3Boo();
			var baba = _V11create_babax_rP4Baba(42);
			var bui = _V10create_buix_rP3Bui(777);
			var bababui = _V14create_bababuixx_rP7Bababui(-123, 321);

			Assert.AreEqual(-1, _V7is_whenP3Boo_rx(boo));
			Assert.AreEqual(42 * 42, _V7is_whenP3Boo_rx(baba));
			Assert.AreEqual(777 + 777, _V7is_whenP3Boo_rx(bui));
			Assert.AreEqual(321 * (-123 * -123), _V7is_whenP3Boo_rx(bababui));

			Assert.AreEqual(10, _V10range_whenx_rx(10));
			Assert.AreEqual(11 * 11, _V10range_whenx_rx(11));
			Assert.AreEqual(100 * 100, _V10range_whenx_rx(100));
			Assert.AreEqual(-6, _V10range_whenx_rx(-6));
			Assert.AreEqual(2 * -7, _V10range_whenx_rx(-7));
			Assert.AreEqual(2 * -8, _V10range_whenx_rx(-8));
			Assert.AreEqual(2 * -42, _V10range_whenx_rx(-42));
			Assert.AreEqual(3, _V10range_whenx_rx(3));
		}

		public static void Whens()
		{
			if (!Compile("Whens", new[] { "Whens.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Whens_Test();
		}

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern bool _V7can_useP6EntityP6Usable_rb(IntPtr entity, IntPtr usable);

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE(IntPtr usables, long min_reliability);

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS3_(IntPtr entity, IntPtr vehicles, long distance);

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V10create_pigv_rP3Pig();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V10create_busv_rP3Bus();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V10create_carv_rP3Car();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V13create_bananav_rP6Banana();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V11create_johnv_rP6Person();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V10create_maxv_rP6Person();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V11create_gabev_rP6Person();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V12create_stevev_rP6Person();

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern IntPtr _V12create_arrayx_rP5ArrayIP6UsableE(long size);

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern void _V3setP5ArrayIP6UsableES1_x(IntPtr array, IntPtr usable, long i);

		[DllImport("Unit_Is", ExactSpelling = true)]
		private static extern bool _V6is_pigP7Vehicle_rb(IntPtr vehicle);

		private static void Is_Test()
		{
			var pig = _V10create_pigv_rP3Pig();
			var bus = _V10create_busv_rP3Bus();
			var car = _V10create_carv_rP3Car();
			var banana = _V13create_bananav_rP6Banana();

			var john = _V11create_johnv_rP6Person();
			var max = _V10create_maxv_rP6Person();
			var gabe = _V11create_gabev_rP6Person();
			var steve = _V12create_stevev_rP6Person();

			var array = _V12create_arrayx_rP5ArrayIP6UsableE(4);
			_V3setP5ArrayIP6UsableES1_x(array, pig + TestSettings.TypeBaseOffset, 0);
			_V3setP5ArrayIP6UsableES1_x(array, bus + TestSettings.TypeBaseOffset, 1);
			_V3setP5ArrayIP6UsableES1_x(array, car + TestSettings.TypeBaseOffset, 2);
			_V3setP5ArrayIP6UsableES1_x(array, banana, 3);

			Assert.False(_V7can_useP6EntityP6Usable_rb(john, pig + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(john, bus + TestSettings.TypeBaseOffset));
			Assert.True(_V7can_useP6EntityP6Usable_rb(john, car + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(john, banana));

			Assert.True(_V7can_useP6EntityP6Usable_rb(max, pig + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(max, bus + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(max, car + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(max, banana));

			Assert.False(_V7can_useP6EntityP6Usable_rb(gabe, pig + TestSettings.TypeBaseOffset));
			Assert.True(_V7can_useP6EntityP6Usable_rb(gabe, bus + TestSettings.TypeBaseOffset));
			Assert.True(_V7can_useP6EntityP6Usable_rb(gabe, car + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(gabe, banana));

			Assert.True(_V7can_useP6EntityP6Usable_rb(steve, pig + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(steve, bus + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(steve, car + TestSettings.TypeBaseOffset));
			Assert.False(_V7can_useP6EntityP6Usable_rb(steve, banana));

			_V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE(array, long.MinValue);

			var vehicles = _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE(array, 10);

			Assert.AreEqual(car + TestSettings.TypeBaseOffset, _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS3_(john, vehicles, 7000));
			Assert.AreEqual(car + TestSettings.TypeBaseOffset, _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS3_(max, vehicles, 1000));
			Assert.AreEqual(car + TestSettings.TypeBaseOffset, _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS3_(gabe, vehicles, 3000));

			var vehicle = _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS3_(steve, vehicles, 3000);

			Assert.True(_V6is_pigP7Vehicle_rb(vehicle));
		}

		public static void Is()
		{
			if (!Compile("Is", new[] { "Is.v", GetProjectFile("Math.v", STANDARD_LIBRARY_FOLDER) }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Is_Test();
		}

		public static void ExpressionVariables_Test()
		{
			string actual = Execute("ExpressionVariables");
			string expected = File.ReadAllText(GetProjectFile("ExpressionVariables_Output.txt", TESTS)).Replace("\r\n", "\n");

			Assert.AreEqual(expected, actual);
		}

		public static void ExpressionVariables()
		{
			if (!CompileExecutable("ExpressionVariables", new[] { "ExpressionVariables.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			ExpressionVariables_Test();
		}


		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern void _V11iteration_1P5ArrayIxEPx(IterationArray array, IntPtr destination);

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern void _V11iteration_2Px(IntPtr destination);

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern void _V11iteration_3P5RangePx(IterationRange range, IntPtr destination);

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern void _V11iteration_4P5ArrayIP6ObjectE(IterationArray objects);

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern void _V11iteration_5P5ArrayIP6ObjectE(IterationArray objects);

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern IntPtr _V7range_1v_rP5Range();

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern IntPtr _V7range_2v_rP5Range();

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern IntPtr _V7range_3xx_rP5Range(long a, long b);

		[DllImport("Unit_Iteration", ExactSpelling = true)]
		private static extern IntPtr _V7range_4xx_rP5Range(long a, long b);

		private static IntPtr GetUnmanagedObject<T>(T instance, int size)
		{
			var memory = Allocate(size);
			Marshal.StructureToPtr(instance!, memory, false);

			return memory;
		}

		public static void Iteration_Test()
		{
			var buffer = Allocate(sizeof(long) * 5);
			Marshal.WriteInt64(buffer, sizeof(long) * 0, -2);
			Marshal.WriteInt64(buffer, sizeof(long) * 1, 3);
			Marshal.WriteInt64(buffer, sizeof(long) * 2, -5);
			Marshal.WriteInt64(buffer, sizeof(long) * 3, 7);
			Marshal.WriteInt64(buffer, sizeof(long) * 4, -11);

			var destination = Allocate(sizeof(long) * 5);

			var array = new IterationArray
			{
				Count = 5,
				Data = buffer
			};

			_V11iteration_1P5ArrayIxEPx(array, destination);

			for (var i = 0; i < 5; i++)
			{
				Assert.AreEqual(Marshal.ReadInt64(buffer, sizeof(long) * i), Marshal.ReadInt64(destination, sizeof(long) * i));
			}

			Marshal.FreeHGlobal(destination);
			destination = Allocate(sizeof(long) * (10 + 1 + 10));

			_V11iteration_2Px(destination);

			for (var i = -10; i <= 10; i++)
			{
				Assert.AreEqual(i * i, Marshal.ReadInt64(destination, sizeof(long) * (i + 10)));
			}

			var range = new IterationRange() { Start = -7, End = -3 };

			Marshal.FreeHGlobal(destination);
			destination = Allocate(sizeof(long) * (7 - (-3) + 1));

			_V11iteration_3P5RangePx(range, destination);

			for (var i = -7; i <= -3; i++)
			{
				Assert.AreEqual(2 * i, Marshal.ReadInt64(destination, sizeof(long) * (i + 7)));
			}

			Marshal.FreeHGlobal(buffer);

			buffer = Allocate(8 * 3); // 3 x Object

			var first = GetUnmanagedObject(new IterationObject() { Value = -123.456, Flag = false }, TestSettings.TypeBaseOffset + 9);
			var second = GetUnmanagedObject(new IterationObject() { Value = -1.333333, Flag = false }, TestSettings.TypeBaseOffset + 9);
			var third = GetUnmanagedObject(new IterationObject() { Value = 1010, Flag = false }, TestSettings.TypeBaseOffset + 9);

			Marshal.WriteIntPtr(buffer, 0, first);
			Marshal.WriteIntPtr(buffer, 8, second);
			Marshal.WriteIntPtr(buffer, 16, third);

			array.Data = buffer;
			array.Count = 3;

			_V11iteration_4P5ArrayIP6ObjectE(array);

			Assert.AreEqual((byte)1, Marshal.ReadByte(first, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual((byte)1, Marshal.ReadByte(second, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual((byte)0, Marshal.ReadByte(third, TestSettings.TypeBaseOffset + 8));

			Marshal.WriteInt64(first, TestSettings.TypeBaseOffset, BitConverter.DoubleToInt64Bits(12.345)); // first.value = 12.345
			Marshal.WriteByte(first, TestSettings.TypeBaseOffset + 8, 0); // first.flag = false

			Marshal.WriteInt64(second, TestSettings.TypeBaseOffset, BitConverter.DoubleToInt64Bits(-12.34)); // second.value = -12.34
			Marshal.WriteByte(second, TestSettings.TypeBaseOffset + 8, 0); // second.flag = false

			Marshal.WriteInt64(third, TestSettings.TypeBaseOffset, BitConverter.DoubleToInt64Bits(101)); // third.value = 101
			Marshal.WriteByte(third, TestSettings.TypeBaseOffset + 8, 0); // third.flag = false

			_V11iteration_5P5ArrayIP6ObjectE(array);

			Assert.AreEqual((byte)1, Marshal.ReadByte(first, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual((byte)1, Marshal.ReadByte(second, TestSettings.TypeBaseOffset + 8));
			Assert.AreEqual((byte)0, Marshal.ReadByte(third, TestSettings.TypeBaseOffset + 8));

			range = Marshal.PtrToStructure<IterationRange>(_V7range_1v_rP5Range());

			Assert.AreEqual(1, range!.Start);
			Assert.AreEqual(10, range.End);

			range = Marshal.PtrToStructure<IterationRange>(_V7range_2v_rP5Range());

			Assert.AreEqual((long)-5e2, range!.Start);
			Assert.AreEqual((long)10e10, range.End);

			range = Marshal.PtrToStructure<IterationRange>(_V7range_3xx_rP5Range(314159, -42));

			Assert.AreEqual((long)314159, range!.Start);
			Assert.AreEqual((long)-42, range.End);

			range = Marshal.PtrToStructure<IterationRange>(_V7range_4xx_rP5Range(-12, -14));

			Assert.AreEqual(12 * 12, range!.Start);
			Assert.AreEqual(14 * 14, range.End);
		}

		public static void Iteration()
		{
			if (!Compile("Iteration", new[] { "Iteration.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Iteration_Test();
		}

		private static void Namespaces_Test()
		{
			var actual = Execute("Namespaces");
			var expected = "Apple\nBanana\nFactory Foo.Apple\nFactory Foo.Apple\nFactory Foo.Apple\n";

			Assert.AreEqual(expected, actual);
		}

		public static void Namespaces()
		{
			if (!CompileExecutable("Namespaces", new[] { "Namespaces.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Namespaces_Test();
		}

		private static void Extensions_Test()
		{
			var actual = Execute("Extensions");
			var expected = "Decimal seems to be larger than tiny\nFactory created new Foo.Bar.Counter\n7\n";

			Assert.AreEqual(expected, actual);
		}

		public static void Extensions()
		{
			if (!CompileExecutable("Extensions", new[] { "Extensions.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Extensions_Test();
		}

		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern int _V13memory_case_1P6Objecti_ri(ref MemoryObject instance, int value);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern byte _V13memory_case_2Phi_rh(IntPtr memory, int i);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V13memory_case_3P6Objectdd_rd(ref MemoryObject instance, double value);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern int _V13memory_case_4P6ObjectS0__ri(ref MemoryObject a, ref MemoryObject b);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V13memory_case_5P6ObjectPh_rd(IntPtr instance, IntPtr memory);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V13memory_case_6P6Object_rd(ref MemoryObject a);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V13memory_case_7P6ObjectS0__rd(ref MemoryObject a, ref MemoryObject b);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V13memory_case_8P6ObjectS0__rd(ref MemoryObject a, ref MemoryObject b);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V13memory_case_9P6ObjectS0__rd(ref MemoryObject a, IntPtr b);
		
		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern double _V14memory_case_10P6ObjectS0__rd(ref MemoryObject a, ref MemoryObject b);

		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern void _V14memory_case_11P6Objectx(ref MemoryObject a, int i);

		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern int _V14memory_case_12P6Objectx_ri(ref MemoryObject a, int i);

		[DllImport("Unit_Memory", ExactSpelling = true)]
		private static extern void _V14memory_case_13P6Objectx(ref MemoryObject a, int i);



		public static void Memory_Test()
		{
			var a = new MemoryObject();
			var x = Allocate(TestSettings.TypeBaseOffset + 20);
			a.Other = x;

			var b = new MemoryObject();
			var y = Allocate(TestSettings.TypeBaseOffset + 20);
			b.Other = y;

			Assert.AreEqual(10, _V13memory_case_1P6Objecti_ri(ref a, 10));
			Assert.AreEqual(10, a.X);

			var memory = Allocate(TestSettings.TypeBaseOffset + 20);
			Assert.AreEqual(7, _V13memory_case_2Phi_rh(memory, 6));
			Assert.AreEqual(7, Marshal.ReadByte(memory, 6));

			a.Y = 1.718281;
			var result = _V13memory_case_3P6Objectdd_rd(ref a, 8.8);
			if (result != 1.718281 + 1.0 + 8.0 && result != 10.718281) Assert.Fail("Values are not equal");

			Assert.AreEqual(8, a.X);
			Assert.AreEqual(1.718281 + 1.0, a.Y);

			Assert.AreEqual(2, _V13memory_case_4P6ObjectS0__ri(ref a, ref a));
			Assert.AreEqual(2, a.X);

			Assert.AreEqual(120.11225, _V13memory_case_5P6ObjectPh_rd(memory, memory + TestSettings.TypeBaseOffset + 4));

			Assert.AreEqual(-3.14159, _V13memory_case_6P6Object_rd(ref a));
			Assert.AreEqual(-3.14159, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(a.Other, TestSettings.TypeBaseOffset + 4)));
			
			a.Y = 1.0;
			b.Y = -1.0;
			var previous = a.Other;
			Assert.AreEqual(-1.0, _V13memory_case_7P6ObjectS0__rd(ref a, ref b));
			Assert.AreNotEqual(previous, a.Other);
			Assert.AreEqual(-3.14159, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(previous, TestSettings.TypeBaseOffset + 4)));
			a.Other = previous;

			Marshal.WriteInt64(b.Other, TestSettings.TypeBaseOffset + 4, BitConverter.DoubleToInt64Bits(101.1000));
			Assert.AreEqual(101.1000, _V13memory_case_8P6ObjectS0__rd(ref a, ref b));

			Assert.AreEqual(10.0, _V13memory_case_9P6ObjectS0__rd(ref a, a.Other));
			Assert.AreEqual(10.0, BitConverter.Int64BitsToDouble(Marshal.ReadInt64(a.Other, TestSettings.TypeBaseOffset + 4)));

			a.Y = 13579.2468;
			Assert.AreEqual(13579.2468, _V14memory_case_10P6ObjectS0__rd(ref a, ref a));
			
			a.X = 10;
			a.Other = x;
			Marshal.WriteInt32(a.Other, TestSettings.TypeBaseOffset, 20);
			_V14memory_case_11P6Objectx(ref a, 7);
			Assert.AreEqual(11, a.X);
			Assert.AreEqual(21, Marshal.ReadInt32(a.Other, TestSettings.TypeBaseOffset));
			
			_V14memory_case_11P6Objectx(ref a, -3);
			Assert.AreEqual(12, a.X);
			Assert.AreEqual(22, Marshal.ReadInt32(a.Other, TestSettings.TypeBaseOffset));

			a.Y = -2.25;
			Assert.AreEqual(-2, _V14memory_case_12P6Objectx_ri(ref a, 555));
			Assert.AreEqual(-2, a.X);
			Assert.AreEqual(-2.25, a.Y);

			a.X = 101;
			Assert.AreEqual(101, _V14memory_case_12P6Objectx_ri(ref a, -111));
			Assert.AreEqual(101, a.X);
			Assert.AreEqual(101.0, a.Y);

			a.X = 3;
			a.Y = 92.001;
			_V14memory_case_13P6Objectx(ref a, 7);
			Assert.AreEqual(10, a.X);
			Assert.AreEqual(100.001, a.Y);

			a.X = 7;
			a.Y = 6.0;
			_V14memory_case_13P6Objectx(ref a, -7);
			Assert.AreEqual(0, a.X);
			Assert.AreEqual(0.0, a.Y);

			if (OptimizationLevel < 1 || Analysis.IsGarbageCollectorEnabled) return;

			// Load the generated assembly
			var assembly = LoadAssemblyOutput("Memory");
			
			Assert.AreEqual(1, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_1P6Objecti_ri")));
			Assert.AreEqual(1, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_2Phi_rh")));
			Assert.AreEqual(3, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_3P6Objectdd_rd")));
			Assert.AreEqual(3, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_4P6ObjectS0__ri")));
			Assert.AreEqual(3, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_5P6ObjectPh_rd")));
			Assert.AreEqual(2, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_6P6Object_rd")));
			Assert.AreEqual(4, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_7P6ObjectS0__rd")));
			Assert.AreEqual(4, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_8P6ObjectS0__rd")));
			Assert.AreEqual(4, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V13memory_case_9P6ObjectS0__rd")));
			Assert.AreEqual(5, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V14memory_case_10P6ObjectS0__rd")));
			Assert.AreEqual(6, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V14memory_case_11P6Objectx")));
			Assert.AreEqual(4, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V14memory_case_12P6Objectx_ri")));
			Assert.AreEqual(6, GetMemoryAddressCount(GetFunctionFromAssembly(assembly, "_V14memory_case_13P6Objectx")));
		}

		public static void Memory()
		{
			if (!Compile("Memory", new[] { "Memory.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Memory_Test();
		}

		private static void Lists_Test()
		{
			var actual = Execute("Lists");
			var expected = "1, 2, 3, 5, 7, 11, 13, \n42, 69, \nFoo, Bar, Baz, Qux, Xyzzy, \nFoo, Bar, Baz x 3, Qux, Xyzzy x 7, \n";

			Assert.AreEqual(expected, actual);
		}

		public static void Lists()
		{
			if (!CompileExecutable("Lists", new[] { "Lists.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Lists_Test();
		}

		private static void Packs_Test()
		{
			var actual = Execute("Packs");
			var expected = "170\n2143\n20716\n3050\n4058\n3502\n354256\n";

			Assert.AreEqual(expected, actual);
		}

		public static void Packs()
		{
			if (!CompileExecutable("Packs", new[] { "Packs.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			Packs_Test();
		}

		private static void UnnamedPacks_Test()
		{
			var actual = Execute("UnnamedPacks");
			var expected = "420\n420\n2310\n";

			Assert.AreEqual(expected, actual);
		}

		public static void UnnamedPacks()
		{
			if (!CompileExecutable("UnnamedPacks", new[] { "UnnamedPacks.v" }.Concat(StandardLibraryUtility).ToArray()))
			{
				Assert.Fail("Failed to compile");
			}

			UnnamedPacks_Test();
		}
	}
}
