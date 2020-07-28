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
      private const string INCLUDE_PATH = "C:\\Users\\Lehto\\Intuitive\\Zigzag\\Tests\\";
      private const string LIBZ = "C:\\Users\\Lehto\\Intuitive\\Zigzag\\libz\\";
      private const string Prefix = "NUnit_";

      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_basic_math(Int64 a, Int64 b, Int64 c);

      [DllImport("NUnit_BasicIfStatement", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_basic_if_statement(Int64 a, Int64 b);

      [DllImport("NUnit_BasicCallEvacuation", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_basic_call_evacuation(Int64 a, Int64 b);

      [DllImport("NUnit_BasicCallEvacuation", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_basic_call_evacuation_with_memory(Int64 a, Int64 b);

      

      [DllImport("NUnit_BasicDataFieldAssign", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern void function_basic_data_field_assign(ref BasicDataType target);

      [DllImport("NUnit_ConditionallyChangingConstant", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_conditionally_changing_constant_with_if_statement(Int64 a, Int64 b);

      [DllImport("NUnit_ConditionallyChangingConstant", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_conditionally_changing_constant_with_loop_statement(Int64 a, Int64 b);

      [DllImport("NUnit_ConstantPermanence", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern void function_constant_permanence_and_array_copy([MarshalAs(UnmanagedType.LPArray)] byte[] source, [MarshalAs(UnmanagedType.LPArray)] byte[] destination);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_reference_decoy_1(Int64 b);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_reference_decoy_2(Int64 b);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_reference_decoy_3(Int64 b);

      [DllImport("NUnit_ReferenceDecoys", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_reference_decoy_4(Int64 b);

      [DllImport("NUnit_Stack", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_multi_return(Int64 a, Int64 b);

      [DllImport("NUnit_RegisterUtilization", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_register_utilization(Int64 a, Int64 b, Int64 c, Int64 d, Int64 e);

      [DllImport("NUnit_SpecialMultiplications", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_special_multiplications(Int64 a, Int64 b);

      [DllImport("NUnit_LargeFunctions", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_g(Int64 a, Int64 b);
      
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

         var files = source_files.Select(f => Path.IsPathRooted(f) ? f : INCLUDE_PATH + f).ToArray();

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

         var files = source_files.Select(f => Path.IsPathRooted(f) ? f : INCLUDE_PATH + f).ToArray();

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
            FileName = Prefix + name + ".exe",
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

      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_addition(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_subtraction(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_multiplication(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_division(Int64 a, Int64 b);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_addition_with_constant(Int64 a);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_subtraction_with_constant(Int64 a);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_multiplication_with_constant(Int64 a);
      [DllImport("NUnit_BasicMath", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_division_with_constant(Int64 a);

      [TestCase]
      public void Assembler_BasicMath()
      {
         if (!Compile("BasicMath", "BasicMath.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var result = function_basic_math(6, 7, 9);

         Assert.AreEqual(42069, result);

         Assert.AreEqual(3, function_addition(1, 2));
         Assert.AreEqual(-90, function_subtraction(10, 100));
         Assert.AreEqual(49, function_multiplication(7, 7));
         Assert.AreEqual(7, function_division(42, 6));

         Assert.AreEqual(64, function_addition_with_constant(44));
         Assert.AreEqual(-1, function_subtraction_with_constant(19));
         Assert.AreEqual(1300, function_multiplication_with_constant(13));
         Assert.AreEqual(1, function_division_with_constant(10));
      }

      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_addition(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_subtraction(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_multiplication(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_division(Double a, Double b);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_addition_with_constant(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_subtraction_with_constant(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_multiplication_with_constant(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_division_with_constant(Double a);
      [DllImport("NUnit_Decimals", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_decimal_operator_order(Double a, Double b);

      [TestCase]
      public void Assembler_DecimalArithmetics()
      {
         if (!Compile("Decimals", "Decimals.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(3.141 + 2.718, function_decimal_addition(3.141, 2.718));
         Assert.AreEqual(3.141 - 2.718, function_decimal_subtraction(3.141, 2.718));
         Assert.AreEqual(3.141 * 2.718, function_decimal_multiplication(3.141, 2.718));
         Assert.AreEqual(3.141 / 2.718, function_decimal_division(3.141, 2.718));

         Assert.AreEqual(1.414 + 4.474 + 1.414, function_decimal_addition_with_constant(4.474));
         Assert.AreEqual(-1.414 + 3.363 - 1.414, function_decimal_subtraction_with_constant(3.363));
         Assert.AreEqual(1.414 * 2.252 * 1.414, function_decimal_multiplication_with_constant(2.252));
         Assert.AreEqual(2.0 / 1.414 / 1.414, function_decimal_division_with_constant(1.414));

         Assert.AreEqual(9.870 + 7.389 * 9.870 - 7.389 / 9.870, function_decimal_operator_order(9.870, 7.389));
      }

      [TestCase]
      public void Assembler_BasicIfStatement()
      {
         if (!Compile("BasicIfStatement", "BasicIfStatement.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var result = function_basic_if_statement(100, 999);
         Assert.AreEqual(999, result);

         result = function_basic_if_statement(1, -1);
         Assert.AreEqual(1, result);

         result = function_basic_if_statement(777, 777);
         Assert.AreEqual(777, result);
      }

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_basic_for_loop(Int64 start, Int64 count);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_nested_for_loops([MarshalAs(UnmanagedType.LPArray)] byte[] destination, Int64 width);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_conditional_loop(Int64 start);
      
      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_conditional_action_loop(Int64 start);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_normal_for_loop(Int64 start, Int64 count);

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_normal_for_loop_with_stop(Int64 start, Int64 count);

      [TestCase]
      public void Assembler_BasicForLoop()
      {
         if (!Compile("BasicForLoop", "BasicForLoop.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(100, function_basic_for_loop(70, 5));

         Assert.AreEqual(10, function_conditional_loop(3));
         Assert.AreEqual(1344, function_conditional_action_loop(42));
         Assert.AreEqual(3169, function_normal_for_loop(3141, 8));

         Assert.AreEqual(220, function_normal_for_loop_with_stop(10, 20));
         Assert.AreEqual(3, function_normal_for_loop_with_stop(-3, 3));
         Assert.AreEqual(10, function_normal_for_loop_with_stop(10, -1));
         Assert.AreEqual(-1, function_normal_for_loop_with_stop(0, 999));

         var expected = new byte[] { 100, 0, 100, 0, 0, 0, 100, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 0, 100, 0, 0, 0, 100, 0, 100 };

         var actual = new byte[27];
         var w = function_nested_for_loops(actual, 3);

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

         Assert.AreEqual(570, function_basic_call_evacuation(10, 50));
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
         function_basic_data_field_assign(ref target);

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

         var result = function_conditionally_changing_constant_with_if_statement(10, 20);
         Assert.AreEqual(17, result);

         result = function_conditionally_changing_constant_with_if_statement(10, 0);
         Assert.AreEqual(10 * 2, result);

         result = function_conditionally_changing_constant_with_loop_statement(3, 2);
         Assert.AreEqual(2 * 100, result);

         result = function_conditionally_changing_constant_with_loop_statement(2, 5);
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

         function_constant_permanence_and_array_copy(source, destination);

         // Check whether the array copy with offset succeeded
         Assert.AreEqual(new byte[] { 0, 0, 0, 7, 11, 13, 15, 17, 19, 23, 29, 31, 33, 0 }, destination);

         var assembly = LoadAssemblyOutput("ConstantPermanence");
         Console.WriteLine(assembly);
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
         Assert.AreEqual(2 * b + 1, function_reference_decoy_1(b));
         Assert.AreEqual(2 * b + 2, function_reference_decoy_2(b));
         Assert.AreEqual(5, function_reference_decoy_3(b));
         Assert.AreEqual(4 * b + 75, function_reference_decoy_4(b));

         // Make sure there aren't any stack memory operations since they aren't needed
         AssertNoMemoryAddress(LoadAssemblyOutput("ReferenceDecoys"));
      }

      [TestCase]
      public void Assembler_PI()
      {
         if (!CompileExecutable("PI", "PI.z", LIBZ + "String.z", LIBZ + "Console.z"))
         {
            Assert.Fail("Failed to compile");
         }

         string actual = Execute("PI");
         string expected = File.ReadAllText(INCLUDE_PATH + "Digits.txt");

         Assert.AreEqual(expected, actual);
      }

      [TestCase]
      public void Assembler_Fibonacci()
      {
         if (!CompileExecutable("Fibonacci", "Fibonacci.z", LIBZ + "String.z", LIBZ + "Console.z"))
         {
            Assert.Fail("Failed to compile");
         }

         string actual = Execute("Fibonacci");
         string expected = File.ReadAllText(INCLUDE_PATH + "Fibonacci_Output.txt").Replace("\r\n", "\n");

         Assert.AreEqual(expected, actual);
      }

      [TestCase]
      public void Assembler_StackSymmetryWithMultipleReturns()
      {
         if (!Compile("Stack", "Stack.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(1, function_multi_return(7, 1));
         Assert.AreEqual(0, function_multi_return(-1, -1));
         Assert.AreEqual(-1, function_multi_return(5, 20));

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

         Assert.AreEqual(-10799508, function_register_utilization(90, 7, 1, 1, 1));

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

         Assert.AreEqual(1802, function_special_multiplications(7, 100));

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

         Assert.AreEqual(197, function_g(26, 16));
      }
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_logical_operators_1(Int64 a, Int64 b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern bool function_single_boolean(bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_two_booleans(bool a, bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern bool function_nested_if_statements(Int64 a, Int64 b, Int64 c);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_logical_and_in_if_statement(bool a, bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_logical_or_in_if_statement(bool a, bool b);
      
      [DllImport("NUnit_LogicalOperators", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_nested_logical_statements(bool a, bool b, bool c, bool d);
      
      [TestCase]
      public void Assembler_LogicalOperators()
      {
         if (!Compile("LogicalOperators", "LogicalOperators.z"))
         {
            Assert.Fail("Failed to compile");
         }
         
         // Single boolean as input
         Assert.IsFalse(function_single_boolean(true));
         Assert.IsTrue(function_single_boolean(false));
         
         // Two booleans as input
         Assert.AreEqual(1, function_two_booleans(true, false));
         Assert.AreEqual(2, function_two_booleans(false, true));
         Assert.AreEqual(3, function_two_booleans(false, false));
         
         // Nested if-statement:
         
         // All correct inputs
         Assert.IsTrue(function_nested_if_statements(1, 2, 3));
         Assert.IsTrue(function_nested_if_statements(1, 2, 4));
         Assert.IsTrue(function_nested_if_statements(1, 0, 1));
         Assert.IsTrue(function_nested_if_statements(1, 0, -1));
         
         Assert.IsTrue(function_nested_if_statements(2, 4, 8));
         Assert.IsTrue(function_nested_if_statements(2, 4, 6));
         Assert.IsTrue(function_nested_if_statements(2, 3, 4));
         Assert.IsTrue(function_nested_if_statements(2, 3, 5));

         // Most of the paths for returning false
         Assert.IsFalse(function_nested_if_statements(0, 0, 0));
         
         Assert.IsFalse(function_nested_if_statements(1, 1, 1));
         Assert.IsFalse(function_nested_if_statements(1, 2, 5));
         Assert.IsFalse(function_nested_if_statements(1, 0, 0));
         
         Assert.IsFalse(function_nested_if_statements(2, 0, 0));
         Assert.IsFalse(function_nested_if_statements(2, 4, 7));
         Assert.IsFalse(function_nested_if_statements(2, 3, 6));
         
         // Logical and
         Assert.AreEqual(10, function_logical_and_in_if_statement(true, true));
         Assert.AreEqual(0, function_logical_and_in_if_statement(true, false));
         Assert.AreEqual(0, function_logical_and_in_if_statement(false, true));
         Assert.AreEqual(0, function_logical_and_in_if_statement(false, false));
         
         // Logical or
         Assert.AreEqual(10, function_logical_or_in_if_statement(true, true));
         Assert.AreEqual(10, function_logical_or_in_if_statement(true, false));
         Assert.AreEqual(10, function_logical_or_in_if_statement(false, true));
         Assert.AreEqual(0, function_logical_or_in_if_statement(false, false));
         
         // Nested logical statements
         Assert.AreEqual(1, function_nested_logical_statements(true, true, true, true));
         Assert.AreEqual(2, function_nested_logical_statements(false, true, true, true));
         Assert.AreEqual(2, function_nested_logical_statements(true, false, true, true));
         Assert.AreEqual(3, function_nested_logical_statements(true, true, false, true));
         Assert.AreEqual(3, function_nested_logical_statements(true, true, true, false));
         Assert.AreEqual(4, function_nested_logical_statements(true, true, false, false));
         Assert.AreEqual(4, function_nested_logical_statements(false, false, true, true));
         Assert.AreEqual(5, function_nested_logical_statements(true, false, false, false));
         Assert.AreEqual(5, function_nested_logical_statements(false, true, false, false));
         Assert.AreEqual(5, function_nested_logical_statements(false, false, true, false));
         Assert.AreEqual(5, function_nested_logical_statements(false, false, false, true));
         Assert.AreEqual(6, function_nested_logical_statements(false, false, false, false));
         
         Assert.AreEqual(5, function_logical_operators_1(10, 5));
         Assert.AreEqual(7, function_logical_operators_1(0, 7));
         Assert.AreEqual(1, function_logical_operators_1(1, 1));
         Assert.AreEqual(0, function_logical_operators_1(3, 3));
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

      [DllImport("NUnit_ObjectCreation", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern IntPtr function_create_apple();

      [DllImport("NUnit_ObjectCreation", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern IntPtr function_create_car(Double price);

      [TestCase]
      public void Assembler_ObjectCreation()
      {
         if (!Compile("ObjectCreation", "ObjectCreation.z", LIBZ + "String.z", LIBZ + "Console.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var apple = (Apple)Marshal.PtrToStructure(function_create_apple(), typeof(Apple))!;

         Assert.AreEqual(100, apple.Weight);
         Assert.AreEqual(0.1, apple.Price);

         var car = (Car)Marshal.PtrToStructure(function_create_car(20000), typeof(Car))!;

         Assert.AreEqual(2000000, car.Weight);
         Assert.AreEqual(20000, car.Price);

         var brand = (String)Marshal.PtrToStructure(car.Brand, typeof(String))!;

         brand.Assert("Flash");
      }

      [DllImport("NUnit_Templates", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern IntPtr function_create_pack();

      [DllImport("NUnit_Templates", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern IntPtr function_set_product(IntPtr pack, Int64 index, IntPtr name, Int64 value, byte currency);
      
      [DllImport("NUnit_Templates", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern IntPtr function_get_product_name(IntPtr pack, Int64 index);

      [DllImport("NUnit_Templates", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern void function_enchant_product(IntPtr pack, Int64 index);

      [DllImport("NUnit_Templates", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern bool function_is_product_enchanted(IntPtr pack, Int64 index);

      [DllImport("NUnit_Templates", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Double function_get_product_price(IntPtr pack, Int64 index, byte currency);

      [TestCase]
      public void Assembler_Templates()
      {
         if (!Compile("Templates", "Templates.z", LIBZ + "String.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var pack = function_create_pack();

         var car = new String("Car");
         var banana = new String("Banana");
         var lawnmower = new String("Lawnmower");

         const int EUROS = 0;
         const int DOLLARS = 1;

         function_set_product(pack, 0, car.Data, 700000, EUROS);
         function_set_product(pack, 2, lawnmower.Data, 40000, DOLLARS);
         function_set_product(pack, 1, banana.Data, 100, DOLLARS);

         String.From(function_get_product_name(pack, 0)).Assert("Car");
         String.From(function_get_product_name(pack, 1)).Assert("Banana");
         String.From(function_get_product_name(pack, 2)).Assert("Lawnmower");

         function_enchant_product(pack, 0);
         function_enchant_product(pack, 1);

         Assert.IsTrue(function_is_product_enchanted(pack, 0));
         Assert.IsTrue(function_is_product_enchanted(pack, 1));
         Assert.IsFalse(function_is_product_enchanted(pack, 2));

         String.From(function_get_product_name(pack, 0)).Assert("iCar");
         String.From(function_get_product_name(pack, 1)).Assert("iBanana");

         Assert.AreEqual(700000.0, function_get_product_price(pack, 0, EUROS));
         Assert.AreEqual(100.0 * 0.8, function_get_product_price(pack, 1, EUROS));
         Assert.AreEqual(40000.0 * 0.8, function_get_product_price(pack, 2, EUROS));

         Assert.AreEqual(700000.0 * 1.25, function_get_product_price(pack, 0, DOLLARS));
         Assert.AreEqual(100.0, function_get_product_price(pack, 1, DOLLARS));
         Assert.AreEqual(40000.0, function_get_product_price(pack, 2, DOLLARS));
      }

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_bitwise_and(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_bitwise_xor(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_bitwise_or(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_synthetic_and(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_synthetic_xor(byte a, byte b);

      [DllImport("NUnit_BitwiseOperations", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_synthetic_or(byte a, byte b);

      [TestCase]
      public void Assembler_BitwiseOperations()
      {
         if (!Compile("BitwiseOperations", "BitwiseOperations.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(1, function_bitwise_and(1, 1));
         Assert.AreEqual(0, function_bitwise_and(1, 0));
         Assert.AreEqual(0, function_bitwise_and(0, 1));
         Assert.AreEqual(0, function_bitwise_and(0, 0));

         Assert.AreEqual(0, function_bitwise_xor(1, 1));
         Assert.AreEqual(1, function_bitwise_xor(1, 0));
         Assert.AreEqual(1, function_bitwise_xor(0, 1));
         Assert.AreEqual(0, function_bitwise_xor(0, 0));

         Assert.AreEqual(1, function_bitwise_or(1, 1));
         Assert.AreEqual(1, function_bitwise_or(1, 0));
         Assert.AreEqual(1, function_bitwise_or(0, 1));
         Assert.AreEqual(0, function_bitwise_or(0, 0));

         Assert.AreEqual(1, function_synthetic_and(1, 1));
         Assert.AreEqual(0, function_synthetic_and(1, 0));
         Assert.AreEqual(0, function_synthetic_and(0, 1));
         Assert.AreEqual(0, function_synthetic_and(0, 0));

         Assert.AreEqual(0, function_synthetic_xor(1, 1));
         Assert.AreEqual(1, function_synthetic_xor(1, 0));
         Assert.AreEqual(1, function_synthetic_xor(0, 1));
         Assert.AreEqual(0, function_synthetic_xor(0, 0));

         Assert.AreEqual(1, function_synthetic_or(1, 1));
         Assert.AreEqual(1, function_synthetic_or(1, 0));
         Assert.AreEqual(1, function_synthetic_or(0, 1));
         Assert.AreEqual(0, function_synthetic_or(0, 0));
      }
   }
}
