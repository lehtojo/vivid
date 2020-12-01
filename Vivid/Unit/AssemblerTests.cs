using NUnit.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Vivid.Unit
{
	[TestFixture]
	class AssemblerTests
	{
		private const bool IsOptimizationEnabled = true;

		private const string Prefix = "Unit_";

		private const string LIBV = "libv";
		private const string TESTS = "Tests";

		[DllImport("Unit_Arithmetic", ExactSpelling = true)]
		private static extern long _V10arithmeticxxx_rx(long a, long b, long c);

		[DllImport("Unit_Conditionals", ExactSpelling = true)]
		private static extern long _V12conditionalsxx_rx(long a, long b);

		[DllImport("Unit_Evacuation", ExactSpelling = true)]
		private static extern long _V10evacuationxx_rx(long a, long b);

		[DllImport("Unit_Evacuation", ExactSpelling = true)]
		private static extern long _V33basic_call_evacuation_with_memoryxx_rx(long a, long b);

		[DllImport("Unit_Assignment", ExactSpelling = true)]
		private static extern void _V10assignmentP6Holder(ref Holder target);

		[DllImport("Unit_ConditionallyChangingConstant", ExactSpelling = true)]
		private static extern long _V49conditionally_changing_constant_with_if_statementxx_rx(long a, long b);

		[DllImport("Unit_ConditionallyChangingConstant", ExactSpelling = true)]
		private static extern long _V51conditionally_changing_constant_with_loop_statementxx_rx(long a, long b);

		[DllImport("Unit_ConstantPermanence", ExactSpelling = true)]
		private static extern void _V34constant_permanence_and_array_copyPhPS_([MarshalAs(UnmanagedType.LPArray)] byte[] source, [MarshalAs(UnmanagedType.LPArray)] byte[] destination);

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
			var current = Directory.GetCurrentDirectory();
			var separator = GetSeparator();

			// The current directory should be in one of the following forms (Examples):
			// X:\...\$PROJECT_ROOT\bin\Debug\netcoreapp3.1
			// /home/.../$PROJECT_ROOT/bin/Debug/netcore3.1
			for (var i = 0; i < 3; i++)
			{
				var x = current.LastIndexOf(separator);

				if (x == -1)
				{
					throw new ApplicationException("Could not find project root folder");
				}

				current = current[0..x];
			}

			return current;
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
				typeof(AssemblerPhase)
			);

			var files = source_files.Select(f => Path.IsPathRooted(f) ? f : GetProjectFile(f, TESTS)).ToArray();
			var arguments = new List<string>() { "--shared", "--asm", "-o", Prefix + output };

			if (IsOptimizationEnabled)
			{
				arguments.Add("-O1");
			}

			// Pack the program arguments in the chain
			var bundle = new Bundle();
			bundle.Put("arguments", arguments.Concat(files).Concat(new[] { GetProjectFile("Core.v", LIBV) }).ToArray());

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
				typeof(AssemblerPhase)
			);

			var files = source_files.Select(f => Path.IsPathRooted(f) ? f : GetProjectFile(f, TESTS)).ToArray();
			var arguments = new List<string>() { "--asm", "--debug", "-o", Prefix + output };

			#pragma warning disable 162
			if (IsOptimizationEnabled)
			{
				// The condition depends on the constant boolean which is used manually to control the optimization of the tests
				// NOTE: This is practically redundant since this could be automated
				arguments.Add("-O1");
			}
			#pragma warning restore 162

			// Pack the program arguments in the chain
			var bundle = new Bundle();
			bundle.Put("arguments", arguments.Concat(files).Concat(new[] { GetProjectFile("Core.v", LIBV) }).ToArray());

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
				var process = Process.Start(configuration);
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

		private static string LoadAssemblyOutput(string output)
		{
			return File.ReadAllText(Prefix + output + ".asm");
		}

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
				if (Regex.IsMatch(line, "\\[.*\\]") && !line.Contains("lea"))
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
				if (Regex.IsMatch(line, "\\[.*\\]") && !line.Contains("lea"))
				{
					Assert.Fail("Assembly contained memory address(es)");
				}
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool FreeLibrary(IntPtr module);

		public static void UnloadModule(string name)
		{
			foreach (ProcessModule? module in Process.GetCurrentProcess().Modules)
			{
				if (module?.ModuleName.Contains(name) ?? false)
				{
					FreeLibrary(module.BaseAddress);
				}
			}
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

		[TestCase]
		public void Arithmetic()
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

		[TestCase]
		public void Decimals()
		{
			if (!Compile("Decimals", new[] { "Decimals.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Decimals_Test();
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

		[TestCase]
		public void Conditionals()
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

		[TestCase]
		public void Loops()
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

		[TestCase]
		public void Evacuation()
		{
			if (!Compile("Evacuation", new[] { "Evacuation.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Evacuation_Test();
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Holder
		{
			[FieldOffset(8)] public int Normal;
			[FieldOffset(12)] public byte Tiny;
			[FieldOffset(13)] public double Double;
			[FieldOffset(21)] public long Large;
			[FieldOffset(29)] public short Small;
		}

		private static void Assignment_Test()
		{
			var target = new Holder();
			_V10assignmentP6Holder(ref target);

			Assert.AreEqual(64, target.Tiny);
			Assert.AreEqual(12345, target.Small);
			Assert.AreEqual(314159265, target.Normal);
			Assert.AreEqual(-2718281828459045, target.Large);
			Assert.AreEqual(1.414, target.Double);
		}

		[TestCase]
		public void Assignment()
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

		[TestCase]
		public void ConditionallyChangingConstant()
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

			_V34constant_permanence_and_array_copyPhPS_(source, destination);

			// Check whether the array copy with offset succeeded
			Assert.AreEqual(new byte[] { 0, 0, 0, 7, 11, 13, 15, 17, 19, 23, 29, 31, 33, 0 }, destination);

			var assembly = LoadAssemblyOutput("ConstantPermanence");
			Assert.IsTrue(Regex.IsMatch(assembly, "\\[3\\+[a-z0-9]*\\]"));
		}

		[TestCase]
		public void ConstantPermanenceAndArrayCopy()
		{
			if (!Compile("ConstantPermanence", new[] { "ConstantPermanence.v" }))
			{
				Assert.Fail("Failed to compile");
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

		[TestCase]
		public void Linkage()
		{
			if (!Compile("Linkage", new[] { "Linkage.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			Linkage_Test();
		}

		private static void PI_Test()
		{
			string actual = Execute("PI");
			string expected = File.ReadAllText(GetProjectFile("Digits.txt", TESTS));

			Assert.AreEqual(expected, actual);
		}

		[TestCase]
		public void PI()
		{
			if (!CompileExecutable("PI", new[] { "PI.v", GetProjectFile("String.v", LIBV), GetProjectFile("Console.v", LIBV) }))
			{
				Assert.Fail("Failed to compile");
			}

			PI_Test();
		}

		private static void Fibonacci_Test()
		{
			string actual = Execute("Fibonacci");
			string expected = File.ReadAllText(GetProjectFile("Fibonacci_Output.txt", TESTS)).Replace("\r\n", "\n");

			Assert.AreEqual(expected, actual);
		}

		[TestCase]
		public void Fibonacci()
		{
			if (!CompileExecutable("Fibonacci", new[] { "Fibonacci.v", GetProjectFile("String.v", LIBV), GetProjectFile("Console.v", LIBV) }))
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

			var assembly = LoadAssemblyOutput("Stack");
			var j = 0;

			// There should be five 'add rsp, 40' instructions
			for (var i = 0; i < 5; i++)
			{
				j = assembly.IndexOf("add rsp, ", j, StringComparison.Ordinal);

				if (j++ == -1)
				{
					Assert.Fail("Warning: Assembly output did not contain five 'add rsp, 40' instructions");
				}
			}
		}

		[TestCase]
		public void Stack()
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

			// Ensure the assembly function has exactly one memory address since otherwise the compiler wouldn't be utilizing registers as much as it should
			Assert.AreEqual(1, GetMemoryAddressCount(LoadAssemblyFunction("RegisterUtilization", "_V20register_utilizationxxxxxxx_rx")));
		}

		[TestCase]
		public void RegisterUtilization()
		{
			if (!Compile("RegisterUtilization", new[] { "RegisterUtilization.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			RegisterUtilization_Test();
		}

		[TestCase]
		public void SpecialMultiplications()
		{
			if (!Compile("SpecialMultiplications", "SpecialMultiplications.v"))
			{
				Assert.Fail("Failed to compile");
			}

			Assert.AreEqual(1802, _V23special_multiplicationsxx_rx(7, 100));

			// The last part of this test is supposed to run when optimization is disabled
			#pragma warning disable 162
			
			if (IsOptimizationEnabled)
			{
				Assert.Pass();
				return;
			}

			var assembly = LoadAssemblyOutput("SpecialMultiplications");
			Assert.AreEqual(1, GetCountOf(assembly, "mul\\ [a-z]+"));
			Assert.AreEqual(1, GetCountOf(assembly, "sal\\ [a-z]+"));
			Assert.AreEqual(1, GetCountOf(assembly, "lea\\ [a-z]+"));
			Assert.AreEqual(1, GetCountOf(assembly, "sar\\ [a-z]+"));

			#pragma warning restore 162
		}

		private static void LargeFunctions_Test()
		{
			Assert.AreEqual(197, _V1xxx_rx(26, 16));
			Assert.AreEqual(414.414, _V1yxx_rd(8, 13));
		}

		[TestCase]
		public void LargeFunctions()
		{
			if (!Compile("LargeFunctions", new[] { "LargeFunctions.v", GetProjectFile("Core.v", LIBV) }))
			{
				Assert.Fail("Failed to compile");
			}

			LargeFunctions_Test();
		}

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V19logical_operators_1xx_rx(long a, long b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern bool _V14single_booleanb_rx(bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V12two_booleansbb_rx(bool a, bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern bool _V20nested_if_statementsxxx_rx(long a, long b, long c);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V27logical_and_in_if_statementbb_rx(bool a, bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V26logical_or_in_if_statementbb_rx(bool a, bool b);

		[DllImport("Unit_LogicalOperators", ExactSpelling = true)]
		private static extern long _V25nested_logical_statementsbbbb_rx(bool a, bool b, bool c, bool d);

		private static void LogicalOperators_Test()
		{
			// Single boolean as input
			Assert.IsFalse(_V14single_booleanb_rx(true));
			Assert.IsTrue(_V14single_booleanb_rx(false));

			// Two booleans as input
			Assert.AreEqual(1, _V12two_booleansbb_rx(true, false));
			Assert.AreEqual(2, _V12two_booleansbb_rx(false, true));
			Assert.AreEqual(3, _V12two_booleansbb_rx(false, false));

			// Nested if-statement:

			// All correct inputs
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(1, 2, 3));
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(1, 2, 4));
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(1, 0, 1));
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(1, 0, -1));

			Assert.IsTrue(_V20nested_if_statementsxxx_rx(2, 4, 8));
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(2, 4, 6));
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(2, 3, 4));
			Assert.IsTrue(_V20nested_if_statementsxxx_rx(2, 3, 5));

			// Most of the paths for returning false
			Assert.IsFalse(_V20nested_if_statementsxxx_rx(0, 0, 0));

			Assert.IsFalse(_V20nested_if_statementsxxx_rx(1, 1, 1));
			Assert.IsFalse(_V20nested_if_statementsxxx_rx(1, 2, 5));
			Assert.IsFalse(_V20nested_if_statementsxxx_rx(1, 0, 0));

			Assert.IsFalse(_V20nested_if_statementsxxx_rx(2, 0, 0));
			Assert.IsFalse(_V20nested_if_statementsxxx_rx(2, 4, 7));
			Assert.IsFalse(_V20nested_if_statementsxxx_rx(2, 3, 6));

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

		[TestCase]
		public void LogicalOperators()
		{
			if (!Compile("LogicalOperators", new[] { "LogicalOperators.v" }))
			{
				Assert.Fail("Failed to compile");
			}

			LogicalOperators_Test();
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Apple
		{
			[FieldOffset(8)] public long Weight;
			[FieldOffset(16)] public double Price;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct Car
		{
			[FieldOffset(8)] public double Price;
			[FieldOffset(16)] public long Weight;
			[FieldOffset(24)] public IntPtr Brand;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct String
		{
			[FieldOffset(8)] public IntPtr Data;

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

			public void Assert(string expected)
			{
				var expected_bytes = Encoding.UTF8.GetBytes(expected);
				var actual_bytes = new byte[expected_bytes.Length];
				Marshal.Copy(Data, actual_bytes, 0, actual_bytes.Length);

				NUnit.Framework.Assert.AreEqual(expected_bytes, actual_bytes);
				NUnit.Framework.Assert.AreEqual((byte)0, Marshal.ReadByte(Data, expected_bytes.Length));
			}
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
			Assert.AreEqual(20000, car.Price);

			var brand = (String)Marshal.PtrToStructure(car.Brand, typeof(String))!;

			brand.Assert("Flash");
		}

		[TestCase]
		public void Objects()
		{
			if (!Compile("Objects", new[] { "Objects.v", GetProjectFile("String.v", LIBV), GetProjectFile("Console.v", LIBV) }))
			{
				Assert.Fail("Failed to compile");
			}

			Objects_Test();
		}

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern IntPtr _V11create_packv_rP4PackIP7ProductP5PriceE();

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern IntPtr _V11set_productP4PackIP7ProductP5PriceExPhxc(IntPtr pack, long index, IntPtr name, long value, byte currency);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern IntPtr _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(IntPtr pack, long index);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern void _V15enchant_productP4PackIP7ProductP5PriceEx(IntPtr pack, long index);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern bool _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx(IntPtr pack, long index);

		[DllImport("Unit_Templates", ExactSpelling = true)]
		private static extern double _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(IntPtr pack, long index, byte currency);

		private static void Templates_Test()
		{
			var pack = _V11create_packv_rP4PackIP7ProductP5PriceE();

			var car = new String("Car");
			var banana = new String("Banana");
			var lawnmower = new String("Lawnmower");

			const int EUROS = 0;
			const int DOLLARS = 1;

			// fsetproduct_a_tproduct_a_tprice_tpack_tlarge_tlink_ttiny
			_V11set_productP4PackIP7ProductP5PriceExPhxc(pack, 0, car.Data, 700000, EUROS);
			_V11set_productP4PackIP7ProductP5PriceExPhxc(pack, 2, lawnmower.Data, 40000, DOLLARS);
			_V11set_productP4PackIP7ProductP5PriceExPhxc(pack, 1, banana.Data, 100, DOLLARS);

			String.From(_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(pack, 0)).Assert("Car");
			String.From(_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(pack, 1)).Assert("Banana");
			String.From(_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(pack, 2)).Assert("Lawnmower");

			_V15enchant_productP4PackIP7ProductP5PriceEx(pack, 0);
			_V15enchant_productP4PackIP7ProductP5PriceEx(pack, 1);

			Assert.IsTrue(_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx(pack, 0));
			Assert.IsTrue(_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx(pack, 1));
			Assert.IsFalse(_V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx(pack, 2));

			String.From(_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(pack, 0)).Assert("iCar");
			String.From(_V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(pack, 1)).Assert("iBanana");

			Assert.AreEqual(700000.0, _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(pack, 0, EUROS));
			Assert.AreEqual(100.0 * 0.8, _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(pack, 1, EUROS));
			Assert.AreEqual(40000.0 * 0.8, _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(pack, 2, EUROS));

			Assert.AreEqual(700000.0 * 1.25, _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(pack, 0, DOLLARS));
			Assert.AreEqual(100.0, _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(pack, 1, DOLLARS));
			Assert.AreEqual(40000.0, _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(pack, 2, DOLLARS));
		}

		[TestCase]
		public void Templates()
		{
			if (!Compile("Templates", new[] { "Templates.v", GetProjectFile("String.v", LIBV) }))
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

		[TestCase]
		public void BitwiseOperations()
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
			[FieldOffset(8)] public short energy;
			[FieldOffset(10)] public byte hunger;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Fish
		{
			[FieldOffset(8)] public short speed;
			[FieldOffset(10)] public short velocity;
			[FieldOffset(12)] public short weight;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Salmon
		{
			[FieldOffset(8)] public short energy;
			[FieldOffset(10)] public byte hunger;
			[FieldOffset(19)] public short speed;
			[FieldOffset(21)] public short velocity;
			[FieldOffset(23)] public short weight;
			[FieldOffset(33)] public bool is_hiding;
		}

		private static Salmon GetSalmon(IntPtr pointer)
		{
			return Marshal.PtrToStructure<Salmon>(pointer);
		}

		private static IntPtr GetFishPointer(IntPtr salmon)
		{
			return salmon + 11;
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
			Assert.AreEqual(-1, salmon.energy);
			Assert.AreEqual(1, salmon.hunger);

			_V10fish_movesP4Fish(GetFishPointer(salmon_pointer));
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(5, salmon.velocity);
			Assert.AreEqual(-2, salmon.energy);
			Assert.AreEqual(2, salmon.hunger);

			_V10fish_swimsP6Animal(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(5, salmon.velocity);
			Assert.AreEqual(-3, salmon.energy);
			Assert.AreEqual(3, salmon.hunger);

			_V10fish_stopsP6Animal(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(0, salmon.velocity);

			_V10fish_hidesP6Salmon(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(0, salmon.velocity);
			Assert.AreEqual(-4, salmon.energy);
			Assert.AreEqual(4, salmon.hunger);
			Assert.AreEqual(true, salmon.is_hiding);

			// The fish should not move since it's hiding
			_V10fish_movesP4Fish(GetFishPointer(salmon_pointer));
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(0, salmon.velocity);
			Assert.AreEqual(-4, salmon.energy);
			Assert.AreEqual(4, salmon.hunger);
			Assert.AreEqual(true, salmon.is_hiding);

			_V17fish_stops_hidingP6Salmon(salmon_pointer);
			salmon = GetSalmon(salmon_pointer);
			Assert.AreEqual(5, salmon.speed);
			Assert.AreEqual(5, salmon.velocity);
			Assert.AreEqual(-6, salmon.energy);
			Assert.AreEqual(6, salmon.hunger);
			Assert.AreEqual(false, salmon.is_hiding);
		}

		[TestCase]
		public void Inheritance()
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

		[TestCase]
		public void Scopes()
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

		[TestCase]
		public void Lambdas()
		{
			if (!CompileExecutable("Lambdas", new[] { "Lambdas.v", GetProjectFile("String.v", LIBV), GetProjectFile("Console.v", LIBV) }))
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

		[TestCase]
		public void Virtuals()
		{
			if (!CompileExecutable("Virtuals", new[] { "Virtuals.v", GetProjectFile("String.v", LIBV), GetProjectFile("Console.v", LIBV) }))
			{
				Assert.Fail("Failed to compile");
			}

			Virtuals_Test();
		}

		[DllImport("Unit_Whens", ExactSpelling = true)]
		private static extern long _V14numerical_whenx_rx(long x);

		private static void Whens_Test()
		{
			Assert.AreEqual(49, _V14numerical_whenx_rx(7));
			Assert.AreEqual(9, _V14numerical_whenx_rx(3));
			Assert.AreEqual(-1, _V14numerical_whenx_rx(1));

			Assert.AreEqual(42, _V14numerical_whenx_rx(42));
			Assert.AreEqual(-100, _V14numerical_whenx_rx(-100));
			Assert.AreEqual(0, _V14numerical_whenx_rx(0));
		}

		[TestCase]
		public void Whens()
		{
			if (!Compile("Whens", new[] { "Whens.v" }))
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
		private static extern IntPtr _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_(IntPtr entity, IntPtr vehicles, long distance);

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
		private static extern void _V3setP5ArrayIP6UsableES0_x(IntPtr array, IntPtr usable, long i);

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
			_V3setP5ArrayIP6UsableES0_x(array, pig + 8, 0);
			_V3setP5ArrayIP6UsableES0_x(array, bus + 8, 1);
			_V3setP5ArrayIP6UsableES0_x(array, car + 8, 2);
			_V3setP5ArrayIP6UsableES0_x(array, banana, 3);

			Assert.False(_V7can_useP6EntityP6Usable_rb(john, pig + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(john, bus + 8));
			Assert.True(_V7can_useP6EntityP6Usable_rb(john, car + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(john, banana));

			Assert.True(_V7can_useP6EntityP6Usable_rb(max, pig + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(max, bus + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(max, car + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(max, banana));

			Assert.False(_V7can_useP6EntityP6Usable_rb(gabe, pig + 8));
			Assert.True(_V7can_useP6EntityP6Usable_rb(gabe, bus + 8));
			Assert.True(_V7can_useP6EntityP6Usable_rb(gabe, car + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(gabe, banana));

			Assert.True(_V7can_useP6EntityP6Usable_rb(steve, pig + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(steve, bus + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(steve, car + 8));
			Assert.False(_V7can_useP6EntityP6Usable_rb(steve, banana));

			var all = _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE(array, long.MinValue);

			var vehicles = _V21get_reliable_vehiclesP5ArrayIP6UsableEx_rP4ListIP7VehicleE(array, 10);

			Assert.AreEqual(car + 8, _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_(john, vehicles, 7000));
			Assert.AreEqual(car + 8, _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_(max, vehicles, 1000));
			Assert.AreEqual(car + 8, _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_(gabe, vehicles, 3000));
			
			var vehicle = _V14choose_vehicleP6EntityP4ListIP7VehicleEx_rS2_(steve, vehicles, 3000);

			Assert.True(_V6is_pigP7Vehicle_rb(vehicle));
		}

		[TestCase]
		public void Is()
		{
			if (!Compile("Is", new[] { "Is.v", GetProjectFile("Core.v", LIBV), GetProjectFile("String.v", LIBV), GetProjectFile("Array.v", LIBV), GetProjectFile("List.v", LIBV), GetProjectFile("Math.v", LIBV), GetProjectFile("Console.v", LIBV) }))
			{
				Assert.Fail("Failed to compile");
			}

			Is_Test();
		}
	}
}
