using NUnit.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Zigzag.Unit
{
   [TestFixture]
   class AssemblerTests
   {
      private const string INCLUDE_PATH = "C:\\Users\\Lehto\\Documents\\Intuitive\\Zigzag\\Tests\\";
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

      [DllImport("NUnit_BasicForLoop", ExactSpelling = true, CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
      private static extern Int64 function_basic_for_loop(Int64 start, Int64 count);

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

      private bool Compile(string output, params string[] source_files)
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

      private bool CompileExecutable(string output, params string[] source_files)
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

      private string Execute(string name)
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

      private string LoadAssemblyOutput(string output)
      {
         return File.ReadAllText("NUnit_" + output + ".asm");
      }

      private int GetCountOf(string assembly, string pattern)
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

      private int GetMemoryAddressCount(string assembly)
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

      private void AssertNoMemoryAddress(string assembly)
      {
         foreach (var line in assembly.Split('\n'))
         {
            if (Regex.IsMatch(line, "\\[.*\\]") && !line.Contains("lea"))
            {
               Assert.Fail("Assembly contained memory address(es)");
            }
         }
      }

      [TestCase]
      public void Assembler_BasicMath()
      {
         if (!Compile("BasicMath", "BasicMath.z"))
         {
            Assert.Fail("Failed to compile");
         }

         var result = function_basic_math(6, 7, 9);

         Assert.AreEqual(42069, result);
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

      [TestCase]
      public void Assembler_BasicForLoop()
      {
         if (!Compile("BasicForLoop", "BasicForLoop.z"))
         {
            Assert.Fail("Failed to compile");
         }

         Assert.AreEqual(100, function_basic_for_loop(70, 5));
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
   }
}
