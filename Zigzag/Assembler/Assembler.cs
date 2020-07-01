using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Text;
using System.Linq;
using System;

public static class Assembler 
{
	private const string AllocateFunctionIdentifier = "allocate";

	public static Function? AllocationFunction { get; private set; }
	public static Size Size { get; set; } = Size.QWORD;
	public static OSPlatform Target { get; set; } = OSPlatform.Windows;
	public static bool IsTargetWindows => Target == OSPlatform.Windows;
	public static bool IsTargetLinux => Target == OSPlatform.Linux;
	
	public const string TEXT_SECTION = "section .text";
	public const string LINUX_TEXT_SECTION_HEADER = "global _start" + "\n" +
													"_start:" + "\n" +
													"call function_run" + "\n" +
													"mov rax, 60" + "\n" +
													"xor rdi, rdi" + "\n" +
													"syscall" + SEPARATOR;   

	//public const string WINDOWS_TEXT_SECTION_HEADER = 	"global main" + "\n" +
	//																	"main:" + "\n" +
	//																	"jmp function_run" + SEPARATOR;
	public const string WINDOWS_TEXT_SECTION_HEADER = "global function_run";

	public const string DATA_SECTION = "section .data";
	public const string SEPARATOR = "\n\n";

	private static string GetExternalFunctions(Context context)
	{
		var builder = new StringBuilder();

		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				if (overload.IsImported)
				{
					builder.AppendLine($"extern {overload.GetFullname()}");
				}
			}
		}

		builder.Append(SEPARATOR);

		return builder.ToString();
	}

	private static string GetText(Function function, out List<(string, double)> decimals)
	{
		var builder = new StringBuilder();
		var decimal_constants = new List<(string, double)>();

		foreach (var implementation in function.Implementations)
		{
			if (implementation.Node != null && !implementation.IsEmpty)
			{
				var header = string.Empty;

				// Export this function if necessary
				if (function.IsExported)
				{
					builder.AppendLine($"global {function.GetFullname()}");

					if (IsTargetWindows)
					{
						builder.AppendLine($"export {function.GetFullname()}");
					}
				}

				// Append the function label
				builder.AppendLine(function.GetFullname() + ':');

				var unit = new Unit(implementation);

				unit.Execute(UnitPhase.APPEND_MODE, () => 
				{
               // Create the most outer scope where all instructions will be placed
               using var scope = new Scope(unit);

               // Initialize this function
               unit.Append(new InitializeInstruction(unit));

               // Parameters are active from the start of the function, so they must be required now otherwise they would become active at their first usage
               var variables = unit.Function.Parameters;

               if (unit.Function.Metadata!.IsMember && !unit.Function.Metadata!.IsConstructor)
               {
                  variables.Add(unit.Function.GetVariable(Function.THIS_POINTER_IDENTIFIER) ?? throw new ApplicationException("This pointer was missing in member function"));
               }

               unit.Append(new RequireVariablesInstruction(unit, variables));

               if (function is Constructor constructor)
               {
                  Constructors.CreateHeader(unit, constructor.GetTypeParent() ?? throw new ApplicationException("Couldn't get constructor owner type"));
               }

               Builders.Build(unit, implementation.Node);
            });

				// Sprinkle a little intelligence into the output code
				Oracle.Channel(unit);

				var previous = 0;
				var current = unit.Instructions.Count;

				do
				{
					previous = current;

					Oracle.SimulateLifetimes(unit);

					unit.Simulate(UnitPhase.BUILD_MODE, instruction => 
					{
						instruction.Build();
					});

					current = unit.Instructions.Count;
				}
				while (previous != current);

				builder.Append(Translator.Translate(unit));
				builder.AppendLine();

				unit.Decimals.ToList().ForEach(p => decimal_constants.Add((p.Value, p.Key)));
			}
		}

		decimals = decimal_constants;

		if (builder.Length == 0)
		{
			return string.Empty;
		}

		return builder.ToString();
	}

	private static string GetText(Context context, out List<(string, double)> decimals)
	{
		var builder = new StringBuilder();

		decimals = new List<(string, double)>();

		foreach (var function in context.Functions.Values)
		{
			foreach (var overload in function.Overloads)
			{
				if (!overload.IsImported)
				{
					builder.Append(Assembler.GetText(overload, out List<(string, double)> function_decimals));
					builder.Append(SEPARATOR);

					decimals.AddRange(function_decimals);
				}
			}
		}

		foreach (var type in context.Types.Values)
		{
			foreach (var overload in type.Constructors.Overloads)
			{
				builder.Append(Assembler.GetText(overload, out List<(string, double)> constructor_decimals));
				builder.Append(SEPARATOR);

				decimals.AddRange(constructor_decimals);
			}

			builder.Append(GetText(type, out List<(string, double)> type_decimals));

			decimals.AddRange(type_decimals);
		}

		return Regex.Replace(builder.ToString().Replace("\r\n", "\n"), "\n{3,}", "\n\n");
	}

	private static string GetStaticVariables(Type type)
	{
		var builder = new StringBuilder();
		
		foreach (var variable in type.Variables.Values)
		{
			if (variable.IsStatic)
			{
				var name = variable.GetStaticName();
				var allocator = Size.FromBytes(variable.Type!.ReferenceSize).Allocator;

				builder.AppendLine($"{name} {allocator} 0");
			}
		}

		foreach (var subtype in type.Supertypes)
		{
			builder.Append(SEPARATOR);
			builder.AppendLine(GetStaticVariables(subtype));
		}

		return builder.ToString();
	}

	private static IEnumerable<StringNode> GetFunctionStringNodes(FunctionImplementation implementation)
	{
		return implementation.Node?.FindAll(n => n is StringNode).Select(n => (StringNode)n) ?? new List<StringNode>();
	}

	public static IEnumerable<StringNode> GetStringNodes(Context context)
	{
		var nodes = new List<StringNode>();

		foreach (var implementation in context.GetImplementedFunctions())
		{
			nodes.AddRange(GetFunctionStringNodes(implementation));
		}

		foreach (var type in context.Types.Values)
		{
			nodes.AddRange(GetStringNodes(type));
		}

		return nodes;
	}

	private static string GetData(Context context)
	{
		var builder = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			builder.AppendLine(GetStaticVariables(type));
			builder.Append(SEPARATOR);
		}

		var nodes = GetStringNodes(context);

		foreach (var node in nodes)
		{
			var name = node.GetIdentifier(null);
			var allocator = Size.BYTE.Allocator;
			var text = node.Text;

			builder.AppendLine($"{name} {allocator} '{text}', 0");
		}

		return builder.ToString();
	}

	private static string GetDecimalData(List<(string Identifier, double Value)> decimals)
	{
		var builder = new StringBuilder();

		foreach (var (name, value) in decimals)
		{
			var allocator = Size.FromFormat(Types.DECIMAL.Format).Allocator;
			var text = BitConverter.DoubleToInt64Bits(value).ToString(CultureInfo.InvariantCulture);

			builder.AppendLine($"{name} {allocator} {text}");
		}

		return builder.ToString();
	}

	public static string Assemble(Context context)
	{
		AllocationFunction = context.GetFunction(AllocateFunctionIdentifier)?.Overloads[0] ?? throw new ApplicationException("Allocation function was missing");
		
		var builder = new StringBuilder();

		builder.AppendLine(TEXT_SECTION);
		builder.AppendLine(IsTargetWindows ? WINDOWS_TEXT_SECTION_HEADER : LINUX_TEXT_SECTION_HEADER);
		builder.Append(GetExternalFunctions(context));
		builder.Append(GetText(context, out List<(string, double)> decimals));
		builder.Append(SEPARATOR);

		builder.AppendLine(DATA_SECTION);
		builder.Append(GetData(context));
		builder.Append(GetDecimalData(decimals));
		builder.Append(SEPARATOR);

		return Regex.Replace(builder.Replace("\r\n", "\n").ToString(), "\n{3,}", "\n\n");
	}
}