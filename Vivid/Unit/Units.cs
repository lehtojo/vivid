using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Vivid.Unit;
using System;

[TestClass]
public class Units
{
	private const string RED = "\x1B[1;31m";
	private const string GREEN = "\x1B[1;32m";
	private const string RESET = "\x1B[0m";
	private const string CYAN = "\x1B[1;36m";

	private static void PrintSuccess(string name, string description)
	{
		Console.WriteLine($"{name} {GREEN}OK{RESET}: {description}");
	}

	private static void PrintSuccess(string name)
	{
		Console.WriteLine($"{name} {GREEN}OK{RESET}");
	}

	private static void PrintFailure(string name, string description)
	{
		Console.WriteLine();
		Console.WriteLine("--------------------------------------------------");
		Console.WriteLine();
		Console.WriteLine($"{name} {RED}FAIL{RESET}:");
		Console.WriteLine(description);
		Console.WriteLine();
		Console.WriteLine("--------------------------------------------------");
		Console.WriteLine();
	}

	private static void Run(string name, Action unit)
	{
		try
		{
			unit();
			PrintSuccess(name);
		}
		catch (AssertionException assertion)
		{
			if (assertion.IsProblematic)
			{
				PrintFailure(name, assertion.ToString());
			}
			else
			{
				PrintSuccess(name, assertion.Message);
			}
		}
		catch (Exception exception)
		{
			PrintFailure(name, exception.ToString());
		}
	}

	private static void StartSection(string section)
	{
		Console.WriteLine($"{CYAN}{section}{RESET}:");
		Console.WriteLine();
	}

	private static void EndSection()
	{
		Console.WriteLine();
	}

	private static void Setup(string[] arguments)
	{
		if (arguments.Any(i => i == "-O1"))
		{
			AssemblerTests.OptimizationLevel = 1;
		}
		else if (arguments.Any(i => i == "-O2"))
		{
			AssemblerTests.OptimizationLevel = 2;
		}

		AssemblerTests.Initialize();
	}

	public static void Main(string[] arguments)
	{
		Setup(arguments);

		StartSection("Encoding");

		Run("Data", new DataEncoderTests().Run);
		Run("Instructions", new InstructionEncoderCoreTest().Run);
		Run("Jumps", new InstructionEncoderJumpTests().Run);

		EndSection();

		StartSection("Assembler");

		Instructions.X64.Initialize();
		Keywords.Definitions.Clear();
		Operators.Initialize();

		Run("Arithmetic", AssemblerTests.Arithmetic);
		Run("Assignment", AssemblerTests.Assignment);
		Run("BitwiseOperations", AssemblerTests.BitwiseOperations);
		Run("ConditionallyChangingConstant", AssemblerTests.ConditionallyChangingConstant);
		Run("Conditionals", AssemblerTests.Conditionals);
		Run("ConstantPermanenceAndArrayCopy", AssemblerTests.ConstantPermanenceAndArrayCopy);
		Run("Decimals", AssemblerTests.Decimals);
		Run("Evacuation", AssemblerTests.Evacuation);
		Run("LargeFunctions", AssemblerTests.LargeFunctions);
		Run("Linkage", AssemblerTests.Linkage);
		Run("LogicalOperators", AssemblerTests.LogicalOperators);
		Run("Loops", AssemblerTests.Loops);
		Run("Objects", AssemblerTests.Objects);
		Run("RegisterUtilization", AssemblerTests.RegisterUtilization);
		Run("Scopes", AssemblerTests.Scopes);
		Run("SpecialMultiplications", AssemblerTests.SpecialMultiplications);
		Run("Stack", AssemblerTests.Stack);
		Run("Memory", AssemblerTests.Memory);
		Run("Templates", AssemblerTests.Templates);
		Run("Fibonacci", AssemblerTests.Fibonacci);
		Run("PI", AssemblerTests.PI);
		Run("Inheritance", AssemblerTests.Inheritance);
		Run("Namespaces", AssemblerTests.Namespaces);
		Run("Extensions", AssemblerTests.Extensions);
		Run("Packs", AssemblerTests.Packs);
		Run("Virtuals", AssemblerTests.Virtuals);
		Run("Lists", AssemblerTests.Lists);
		Run("Conversions", AssemblerTests.Conversions);
		Run("ExpressionVariables", AssemblerTests.ExpressionVariables);
		Run("Iteration", AssemblerTests.Iteration);
		Run("Lambdas", AssemblerTests.Lambdas);
		Run("Is", AssemblerTests.Is);
		Run("Whens", AssemblerTests.Whens);

		EndSection();

		StartSection("Lexer");

		Run("StandardMath", LexerTests.StandardMath);
		Run("StandardMathAssignment", LexerTests.StandardMathAssignment);
		Run("Math", LexerTests.Math);
		Run("Assignment", LexerTests.Assignment);
		Run("NestedParenthesis", LexerTests.NestedParenthesis);
		Run("UndefinedOperator", LexerTests.UndefinedOperator);

		EndSection();
	}

	[TestMethod]
	public void Run()
	{
		// Support custom working folder for testing
		if (Environment.GetEnvironmentVariable("UNIT_TEST_FOLDER") != null)
		{
			Environment.CurrentDirectory = Environment.GetEnvironmentVariable("UNIT_TEST_FOLDER")!;
		}

		Setup(new string[] { "-O2" });

		StartSection("Assembler");

		Run("Arithmetic", AssemblerTests.Arithmetic);
		Run("Assignment", AssemblerTests.Assignment);
		Run("BitwiseOperations", AssemblerTests.BitwiseOperations);
		Run("ConditionallyChangingConstant", AssemblerTests.ConditionallyChangingConstant);
		Run("Conditionals", AssemblerTests.Conditionals);
		Run("ConstantPermanenceAndArrayCopy", AssemblerTests.ConstantPermanenceAndArrayCopy);
		Run("Decimals", AssemblerTests.Decimals);
		Run("Evacuation", AssemblerTests.Evacuation);
		Run("LargeFunctions", AssemblerTests.LargeFunctions);
		Run("Linkage", AssemblerTests.Linkage);
		Run("LogicalOperators", AssemblerTests.LogicalOperators);
		Run("Loops", AssemblerTests.Loops);
		Run("Objects", AssemblerTests.Objects);
		Run("RegisterUtilization", AssemblerTests.RegisterUtilization);
		Run("Scopes", AssemblerTests.Scopes);
		Run("SpecialMultiplications", AssemblerTests.SpecialMultiplications);
		Run("Stack", AssemblerTests.Stack);
		Run("Memory", AssemblerTests.Memory);
		Run("Templates", AssemblerTests.Templates);
		Run("Fibonacci", AssemblerTests.Fibonacci);
		Run("PI", AssemblerTests.PI);
		Run("Inheritance", AssemblerTests.Inheritance);
		Run("Namespaces", AssemblerTests.Namespaces);
		Run("Extensions", AssemblerTests.Extensions);
		Run("Packs", AssemblerTests.Packs);
		Run("Virtuals", AssemblerTests.Virtuals);
		Run("Lists", AssemblerTests.Lists);
		Run("Conversions", AssemblerTests.Conversions);
		Run("ExpressionVariables", AssemblerTests.ExpressionVariables);
		Run("Iteration", AssemblerTests.Iteration);
		Run("Lambdas", AssemblerTests.Lambdas);
		Run("Is", AssemblerTests.Is);
		Run("Whens", AssemblerTests.Whens);

		EndSection();

		StartSection("Lexer");

		Run("StandardMath", LexerTests.StandardMath);
		Run("StandardMathAssignment", LexerTests.StandardMathAssignment);
		Run("Math", LexerTests.Math);
		Run("Assignment", LexerTests.Assignment);
		Run("NestedParenthesis", LexerTests.NestedParenthesis);
		Run("UndefinedOperator", LexerTests.UndefinedOperator);

		EndSection();
	}
}