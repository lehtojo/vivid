using NUnit.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace Zigzag.Unit
{
   [TestFixture]
   class AssemblerTests
   {
      private const string Prefix = "NUnit_";
      private const string LIBV = "libz";
      private const string TESTS = "Tests";

      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V10basic_mathxxx_rx(Int64 a, Int64 b, Int64 c);

      [DllImport("NUnit_BasicIfStatement", ExactSpelling = true)]
      private static extern Int64 _V18basic_if_statementxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_BasicCallEvacuation", ExactSpelling = true)]
      private static extern Int64 _V21basic_call_evacuationxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_BasicCallEvacuation", ExactSpelling = true)]
      private static extern Int64 _V33basic_call_evacuation_with_memoryxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_BasicDataFieldAssign", ExactSpelling = true)]
      private static extern void _V23basic_data_field_assignP13BasicDataType(ref BasicDataType target);

      [DllImport("NUnit_ConditionallyChangingConstant", ExactSpelling = true)]
      private static extern Int64 _V49conditionally_changing_constant_with_if_statementxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_ConditionallyChangingConstant", ExactSpelling = true)]
      private static extern Int64 _V51conditionally_changing_constant_with_loop_statementxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_ConstantPermanence", ExactSpelling = true)]
      private static extern void _V34constant_permanence_and_array_copyPhPS_([MarshalAs(UnmanagedType.LPArray)] byte[] source, [MarshalAs(UnmanagedType.LPArray)] byte[] destination);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true)]
      private static extern Int64 _V17reference_decoy_1x_rx(Int64 b);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true)]
      private static extern Int64 _V17reference_decoy_2x_rx(Int64 b);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true)]
      private static extern Int64 _V17reference_decoy_3x_rx(Int64 b);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true)]
      private static extern Int64 _V17reference_decoy_4x_rx(Int64 b);

      [DllImport("NUnit_Stack", ExactSpelling = true)]
      private static extern Int64 _V12multi_returnxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_RegisterUtilization", ExactSpelling = true)]
      private static extern Int64 _V20register_utilizationxxxxxxx_rx(Int64 a, Int64 b, Int64 c, Int64 d, Int64 e, Int64 f, Int64 g);

      [DllImport("NUnit_SpecialMultiplications", ExactSpelling = true)]
      private static extern Int64 _V23special_multiplicationsxx_rx(Int64 a, Int64 b);

      [DllImport("NUnit_LargeFunctions", ExactSpelling = true)]
      private static extern Int64 _V1gxx_rx(Int64 a, Int64 b);

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

         // Pack the program arguments in the chain
         var bundle = new Bundle();
         bundle.Put("arguments", new string[] { "--shared", "--asm", "-o", Prefix + output }.Concat(files).ToArray());

         // Execute the chain
         return chain.Execute(bundle);
      }

      private static bool CompileExecutable(string output, params string[] source_files)
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

         // Pack the program arguments in the chain
         var bundle = new Bundle();
         bundle.Put("arguments", new string[] { "--asm", "--debug", "-o", Prefix + output }.Concat(files).ToArray());

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
         return File.ReadAllText("NUnit_" + output + ".asm");
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

      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V8additionxx_rx(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V11subtractionxx_rx(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V14multiplicationxx_rx(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V8divisionxx_rx(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V22addition_with_constantx_rx(Int64 a);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V25subtraction_with_constantx_rx(Int64 a);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V28multiplication_with_constantx_rx(Int64 a);
      [DllImport("NUnit_BasicMath", ExactSpelling = true)]
      private static extern Int64 _V22division_with_constantx_rx(Int64 a);

      [TestCase]
      public void Assembler_BasicMath()
      {
         if (!Compile("BasicMath", "BasicMath.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var result = _V10basic_mathxxx_rx(6, 7, 9);

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

      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V16decimal_additiondd_rd(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V19decimal_subtractiondd_rd(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V22decimal_multiplicationdd_rd(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V16decimal_divisiondd_rd(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V30decimal_addition_with_constantd_rd(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V33decimal_subtraction_with_constantd_rd(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V36decimal_multiplication_with_constantd_rd(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V30decimal_division_with_constantd_rd(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true)]
      private static extern Double _V22decimal_operator_orderdd_rd(Double a, Double b);

      [TestCase]
      public void Assembler_DecimalArithmetics()
      {
         if (!Compile("Decimals", "Decimals.z"))
         {
            Assert.Fail("Failed to compile");
         }

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
      public void Assembler_BasicIfStatement()
      {
         if (!Compile("BasicIfStatement", "BasicIfStatement.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var result = _V18basic_if_statementxx_rx(100, 999);
         Assert.AreEqual(999, result);

         result = _V18basic_if_statementxx_rx(1, -1);
         Assert.AreEqual(1, result);

         result = _V18basic_if_statementxx_rx(777, 777);
         Assert.AreEqual(777, result);
      }

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true)]
      private static extern Int64 _V14basic_for_loopxx_rx(Int64 start, Int64 count);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true)]
      private static extern Int64 _V16nested_for_loopsPhx_rx([MarshalAs(UnmanagedType.LPArray)] byte[] destination, Int64 width);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true)]
      private static extern Int64 _V16conditional_loopx_rx(Int64 start);
      
      [DllImport("NUnit_BasicForLoop", ExactSpelling = true)]
      private static extern Int64 _V23conditional_action_loopx_rx(Int64 start);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true)]
      private static extern Int64 _V15normal_for_loopxx_rx(Int64 start, Int64 count);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true)]
      private static extern Int64 _V25normal_for_loop_with_stopxx_rx(Int64 start, Int64 count);

      [TestCase]
      public void Assembler_BasicForLoop()
      {
         if (!Compile("BasicForLoop", "BasicForLoop.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(100, _V14basic_for_loopxx_rx(70, 5));

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
      public void Assembler_BasicCallEvacuation()
      {
         if (!Compile("BasicCallEvacuation", "BasicCallEvacuation.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(570, _V21basic_call_evacuationxx_rx(10, 50));
      }

      [StructLayout(LayoutKind.Explicit)]
      public struct BasicDataType
      {
         [FieldOffset(0)] public int Normal;
         [FieldOffset(4)] public byte Tiny;
         [FieldOffset(5)] public double Double;
         [FieldOffset(13)] public long Large;
         [FieldOffset(21)] public short Small;
      }

      [TestCase]
      public void Assembler_BasicDataFieldAssign()
      {
         if (!Compile("BasicDataFieldAssign", "BasicDataFieldAssign.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var target = new BasicDataType();
         _V23basic_data_field_assignP13BasicDataType(ref target);

         Assert.AreEqual(64, target.Tiny);
         Assert.AreEqual(12345, target.Small);
         Assert.AreEqual(314159265, target.Normal);
         Assert.AreEqual(-2718281828459045, target.Large);
         Assert.AreEqual(1.414, target.Double);
      }

      [TestCase]
      public void Assembler_ConditionallyChangingConstant()
      {
         if (!Compile("ConditionallyChangingConstant", "ConditionallyChangingConstant.z"))
         {
            Assert.Fail("Failed to compile");
         }

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
      public void Assembler_ConstantPermanenceAndArrayCopy()
      {
         if (!Compile("ConstantPermanence", "ConstantPermanence.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var source = new byte[] { 1, 3, 5, 7, 11, 13, 15, 17, 19, 23, 29, 31, 33, 37 };
         var destination = new byte[14];

         _V34constant_permanence_and_array_copyPhPS_(source, destination);

         // Check whether the array copy with offset succeeded
         Assert.AreEqual(new byte[] { 0, 0, 0, 7, 11, 13, 15, 17, 19, 23, 29, 31, 33, 0 }, destination);

         var assembly = LoadAssemblyOutput("ConstantPermanence");
         Assert.IsTrue(Regex.IsMatch(assembly, "\\[3\\+[a-z0-9]*\\]"));
      }

      [TestCase]
      public void Assembler_ReferenceDecoys()
      {
         if (!Compile("ReferenceDecoys", "ReferenceDecoys.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var b = 42;
         Assert.AreEqual(2 * b + 1, _V17reference_decoy_1x_rx(b));
         Assert.AreEqual(2 * b + 2, _V17reference_decoy_2x_rx(b));
         Assert.AreEqual(5, _V17reference_decoy_3x_rx(b));
         Assert.AreEqual(4 * b + 75, _V17reference_decoy_4x_rx(b));

         // Make sure there aren't any stack memory operations since they aren't needed
         AssertNoMemoryAddress(LoadAssemblyOutput("ReferenceDecoys"));
      }

      [TestCase]
      public void Assembler_PI()
      {
         if (!CompileExecutable("PI", "PI.z", GetProjectFile("String.z", LIBV), GetProjectFile("Console.z", LIBV)))
         {
            Assert.Fail("Failed to compile");
         }

         string actual = Execute("PI");
         string expected = File.ReadAllText(GetProjectFile("Digits.txt", TESTS));

         Assert.AreEqual(expected, actual);
      }

      [TestCase]
      public void Assembler_Fibonacci()
      {
         if (!CompileExecutable("Fibonacci", "Fibonacci.z", GetProjectFile("String.z", LIBV), GetProjectFile("Console.z", LIBV)))
         {
            Assert.Fail("Failed to compile");
         }

         string actual = Execute("Fibonacci");
         string expected = File.ReadAllText(GetProjectFile("Fibonacci_Output.txt", TESTS)).Replace("\r\n", "\n");

         Assert.AreEqual(expected, actual);
      }

      [TestCase]
      public void Assembler_StackSymmetryWithMultipleReturns()
      {
         if (!Compile("Stack", "Stack.z"))
         {
            Assert.Fail("Failed to compile");
         }

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
               Assert.Fail("Warning: Assembly output didn't contain five 'add rsp, 40' instructions");
            }
         }  
      }

      [TestCase]
      public void Assembler_RegisterUtilization()
      {
         if (!Compile("RegisterUtilization", "RegisterUtilization.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(-10799508, _V20register_utilizationxxxxxxx_rx(90, 7, 1, 1, 1, 1, 1));

         // Ensure the assembly output has only two memory addresses since otherwise the compiler wouldn't be utilizing registers as much as it should
         Assert.AreEqual(2, GetMemoryAddressCount(LoadAssemblyOutput("RegisterUtilization")));
      }

      [TestCase]
      public void Assembler_SpecialMultiplications()
      {
         if (!Compile("SpecialMultiplications", "SpecialMultiplications.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(1802, _V23special_multiplicationsxx_rx(7, 100));

         var assembly = LoadAssemblyOutput("SpecialMultiplications");
         Assert.AreEqual(1, GetCountOf(assembly, "mul\\ [a-z]+"));
         Assert.AreEqual(1, GetCountOf(assembly, "sal\\ [a-z]+"));
         Assert.AreEqual(1, GetCountOf(assembly, "lea\\ [a-z]+"));
         Assert.AreEqual(1, GetCountOf(assembly, "sar\\ [a-z]+"));
      }

      [TestCase]
      public void Assembler_LargeFunctions()
      {
         if (!Compile("LargeFunctions", "LargeFunctions.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(197, _V1gxx_rx(26, 16));
      }
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern Int64 _V19logical_operators_1xx_rx(Int64 a, Int64 b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern bool _V14single_booleanb_rx(bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern Int64 _V12two_booleansbb_rx(bool a, bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern bool _V20nested_if_statementsxxx_rx(Int64 a, Int64 b, Int64 c);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern Int64 _V27logical_and_in_if_statementbb_rx(bool a, bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern Int64 _V26logical_or_in_if_statementbb_rx(bool a, bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true)]
      private static extern Int64 _V25nested_logical_statementsbbbb_rx(bool a, bool b, bool c, bool d);
      
      [TestCase]
      public void Assembler_LogicalOperators()
      {
         if (!Compile("LogicalOperators", "LogicalOperators.z"))
         {
            Assert.Fail("Failed to compile");
         }
         
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

      [StructLayout(LayoutKind.Sequential)]
      public struct Apple
      {
         public long Weight;
         public double Price;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct Car
      {
         public double Price;
         public long Weight;
         public IntPtr Brand;
      }

      [StructLayout(LayoutKind.Sequential)]
      public struct String
      {
         public IntPtr Data;

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

      [DllImport("NUnit_ObjectCreation", ExactSpelling = true)]
      private static extern IntPtr _V12create_applev_rP5Apple();

      [DllImport("NUnit_ObjectCreation", ExactSpelling = true)]
      private static extern IntPtr _V10create_card_rP3Car(Double price);

      [TestCase]
      public void Assembler_ObjectCreation()
      {
         if (!Compile("ObjectCreation", "ObjectCreation.z", GetProjectFile("String.z", LIBV), GetProjectFile("Console.z", LIBV)))
         {
            Assert.Fail("Failed to compile");
         }

         var apple = (Apple)Marshal.PtrToStructure(_V12create_applev_rP5Apple(), typeof(Apple))!;

         Assert.AreEqual(100, apple.Weight);
         Assert.AreEqual(0.1, apple.Price);

         var car = (Car)Marshal.PtrToStructure(_V10create_card_rP3Car(20000), typeof(Car))!;

         Assert.AreEqual(2000000, car.Weight);
         Assert.AreEqual(20000, car.Price);

         var brand = (String)Marshal.PtrToStructure(car.Brand, typeof(String))!;

         brand.Assert("Flash");
      }

      [DllImport("NUnit_Templates", ExactSpelling = true)]
      private static extern IntPtr _V11create_packv_rP4PackIP7ProductP5PriceE();

      [DllImport("NUnit_Templates", ExactSpelling = true)]
      private static extern IntPtr _V11set_productP4PackIP7ProductP5PriceExPhxc(IntPtr pack, Int64 index, IntPtr name, Int64 value, byte currency);
      
      [DllImport("NUnit_Templates", ExactSpelling = true)]
      private static extern IntPtr _V16get_product_nameP4PackIP7ProductP5PriceEx_rP6String(IntPtr pack, Int64 index);

      [DllImport("NUnit_Templates", ExactSpelling = true)]
      private static extern void _V15enchant_productP4PackIP7ProductP5PriceEx(IntPtr pack, Int64 index);

      [DllImport("NUnit_Templates", ExactSpelling = true)]
      private static extern bool _V20is_product_enchantedP4PackIP7ProductP5PriceEx_rx(IntPtr pack, Int64 index);

      [DllImport("NUnit_Templates", ExactSpelling = true)]
      private static extern Double _V17get_product_priceP4PackIP7ProductP5PriceExc_rd(IntPtr pack, Int64 index, byte currency);

      [TestCase]
      public void Assembler_Templates()
      {
         if (!Compile("Templates", "Templates.z", GetProjectFile("String.z", LIBV)))
         {
            Assert.Fail("Failed to compile");
         }

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

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V11bitwise_andcc_rc(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V11bitwise_xorcc_rc(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V10bitwise_orcc_rc(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V13synthetic_andcc_rc(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V13synthetic_xorcc_rc(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V12synthetic_orcc_rc(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V18assign_bitwise_andx_rx(long a);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V18assign_bitwise_xorx_rx(long a);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true)]
      private static extern Int64 _V17assign_bitwise_orxx_rx(long a, long b);

      [TestCase]
      public void Assembler_BitwiseOperations()
      {
         if (!Compile("BitwiseOperations", "BitwiseOperations.z"))
         {
            Assert.Fail("Failed to compile");
         }

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

      [StructLayout(LayoutKind.Explicit)]
      struct Animal
      {
         [FieldOffset(0)] public short energy;
         [FieldOffset(2)] public byte hunger;
      }

      [StructLayout(LayoutKind.Explicit)]
      struct Fish
      {
         [FieldOffset(0)] public short speed;
         [FieldOffset(2)] public short velocity;
         [FieldOffset(4)] public short weight;
      }

      [StructLayout(LayoutKind.Explicit)]
      struct Salmon
      {
         [FieldOffset(0)] public short energy;
         [FieldOffset(2)] public byte hunger;
         [FieldOffset(3)] public short speed;
         [FieldOffset(5)] public short velocity;
         [FieldOffset(7)] public short weight;
         [FieldOffset(9)] public bool is_hiding;
      }

      private static Salmon GetSalmon(IntPtr pointer)
      {
         return Marshal.PtrToStructure<Salmon>(pointer);
      }

      private static IntPtr GetFishPointer(IntPtr salmon)
      {
         return salmon + 3;
      }

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V10get_animalv_rP6Animal();

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V8get_fishv_rP4Fish();

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V10get_salmonv_rP6Salmon();

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V12animal_movesP6Animal(IntPtr address);

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V10fish_movesP4Fish(IntPtr address);

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V10fish_swimsP6Animal(IntPtr address);

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V10fish_stopsP6Animal(IntPtr address);

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V10fish_hidesP6Salmon(IntPtr address);

      [DllImport("NUnit_Inheritance", ExactSpelling = true)]
      private static extern IntPtr _V17fish_stops_hidingP6Salmon(IntPtr address);

      [TestCase]
      public void Assembler_Inheritance()
      {
         if (!Compile("Inheritance", "Inheritance.z"))
         {
            Assert.Fail("Failed to compile");
         }

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

      [DllImport("NUnit_Scopes", ExactSpelling = true)]
      private static extern Int64 _V27scopes_nested_if_statementsxxxxxxxx_rx(Int64 a, Int64 b, Int64 c, Int64 d, Int64 e, Int64 f, Int64 g, Int64 h);

      [DllImport("NUnit_Scopes", ExactSpelling = true)]
      private static extern Int64 _V18scopes_single_loopxxxxxxxx_rx(Int64 a, Int64 b, Int64 c, Int64 d, Int64 e, Int64 f, Int64 g, Int64 h);

      [DllImport("NUnit_Scopes", ExactSpelling = true)]
      private static extern Int64 _V19scopes_nested_loopsxxxxxxxx_rx(Int64 a, Int64 b, Int64 c, Int64 d, Int64 e, Int64 f, Int64 g, Int64 h);

      private Int64 GetExpectedReturnValue(Int64 a, Int64 b, Int64 c, Int64 d, Int64 e, Int64 f, Int64 g, Int64 h)
      {
         var x = 2 * a;
         var y = 3 * b;
         var z = 5 * c;

         return (a + b + c + d + e + f + g + h) * x * y * z;
      }

      [TestCase]
      public void Assembler_Scopes()
      {
         if (!Compile("Scopes", "Scopes.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(GetExpectedReturnValue(1, 2, 3, 4, 5, 6, 7, 8), _V27scopes_nested_if_statementsxxxxxxxx_rx(1, 2, 3, 4, 5, 6, 7, 8));
         Assert.AreEqual(GetExpectedReturnValue(10, 20, -30, 40, 50, 60, 70, 80), _V27scopes_nested_if_statementsxxxxxxxx_rx(10, 20, -30, 40, 50, 60, 70, 80));
         Assert.AreEqual(GetExpectedReturnValue(-2, 4, 6, 8, 10, 12, 14, 16), _V27scopes_nested_if_statementsxxxxxxxx_rx(-2, 4, 6, 8, 10, 12, 14, 16));
         Assert.AreEqual(GetExpectedReturnValue(-20, 40, 60, -80, 100, 120, 140, 160), _V27scopes_nested_if_statementsxxxxxxxx_rx(-20, 40, 60, -80, 100, 120, 140, 160));
         Assert.AreEqual(GetExpectedReturnValue(-3, -5, 9, 11, 13, 17, 19, 23), _V27scopes_nested_if_statementsxxxxxxxx_rx(-3, -5, 9, 11, 13, 17, 19, 23));
         Assert.AreEqual(GetExpectedReturnValue(-30, -50, 90, 110, -130, 170, 190, 230), _V27scopes_nested_if_statementsxxxxxxxx_rx(-30, -50, 90, 110, -130, 170, 190, 230));

         Assert.AreEqual(GetExpectedReturnValue(7, 8, 11, 16, 23, 32, 43, 56), _V18scopes_single_loopxxxxxxxx_rx(7, 8, 11, 16, 23, 32, 43, 56));
         
         Assert.AreEqual(GetExpectedReturnValue(7, 8, 11, 16, 23, 32, 43, 56), _V19scopes_nested_loopsxxxxxxxx_rx(7, 8, 11, 16, 23, 32, 43, 56));
      }
   }
}
