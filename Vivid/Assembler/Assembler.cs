using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System;
using System.Globalization;

public static class Assembler
{
	private const string ALLOCATE_FUNCTION_IDENTIFIER = "allocate";

	public static Function? AllocationFunction { get; set; }
	public static Size Size { get; set; } = Size.QWORD;
	public static Format Format => Size.ToFormat();
	public static OSPlatform Target { get; set; } = OSPlatform.Windows;
	public static bool IsTargetWindows => Target == OSPlatform.Windows;
	public static bool IsTargetLinux => Target == OSPlatform.Linux;
	public static bool IsTargetX86 => Size.Bits == 32;
	public static bool IsTargetX64 => Size.Bits == 64;
	public static bool IsDebuggingEnabled { get; set; } = false;

	private const string SECTION_DIRECTIVE = ".section";
	private const string SECREL_DIRECTIVE = ".secrel";
	private const string TEXT_SECTION = SECTION_DIRECTIVE + " .text";
	private const string STRING_ALLOCATOR = ".ascii";
	private const string BYTE_ALIGNMENT = ".balign";
	private const string COMMENT = "#";

	private const string LINUX_TEXT_SECTION_HEADER = 
		".intel_syntax noprefix" + "\n" +
		".file 1 \"Sandbox.v\"" + "\n" +
		".global _start" + "\n" +
		"_start:" + "\n" +
		"call {0}" + "\n" +
		"mov rax, 60" + "\n" +
		"xor rdi, rdi" + "\n" +
		"syscall" + SEPARATOR;

	private const string WINDOWS_TEXT_SECTION_HEADER = 
		".intel_syntax noprefix" + "\n" +
		".file 1 \"Sandbox.v\"" + "\n" +
		".global main" + "\n" +
		"main:" + "\n" +
		"jmp {0}" + SEPARATOR;

	private const string DATA_SECTION = SECTION_DIRECTIVE + " .data";
	private const string SEPARATOR = "\n\n";

	private static string GetExternalFunctions(Context context)
	{
		var builder = new StringBuilder();

		foreach (var function in context.Functions.Values.SelectMany(l => l.Overloads))
		{
			if (!function.IsImported)
			{
				continue;
			}

			foreach (var implementation in function.Implementations)
			{
				if (!implementation.References.Any() && function != AllocationFunction)
				{
					continue;
				}

				builder.AppendLine($".extern {implementation.GetFullname()}");
			}
		}

		builder.Append(SEPARATOR);

		return builder.ToString();
	}

	private static string GetText(Function function, out List<ConstantDataSectionHandle> out_constants)
	{
		var builder = new StringBuilder();
		var constants = new List<ConstantDataSectionHandle>();

		foreach (var implementation in function.Implementations)
		{
			// Build all lambdas defined in the current implementation
			builder.Append(GetText(implementation, out List<ConstantDataSectionHandle> lambda_constants));
			builder.Append(SEPARATOR);

			constants.AddRange(lambda_constants);

			if (implementation.IsInlined || implementation.IsEmpty)
			{
				continue;
			}

			// Export this function if necessary
			if (function.IsExported)
			{
				builder.AppendLine($".global {implementation.GetFullname()}");
			}

			var fullname = implementation.GetFullname();
			var unit = new Unit(implementation);

			unit.Execute(UnitPhase.APPEND_MODE, () =>
			{
				// Create the most outer scope where all instructions will be placed
				using var scope = new Scope(unit);

				if (implementation.VirtualFunction != null)
				{
					unit.Append(new LabelInstruction(unit, new Label(fullname + "_v")));

					var from = implementation.VirtualFunction.GetTypeParent() ?? throw new ApplicationException("Virtual function missing its parent type");
					var to = implementation.GetTypeParent() ?? throw new ApplicationException("Virtual function implementation missing its parent type");

					// NOTE: The type 'from' must be one of the subtypes that type 'to' has
					var alignment = to.GetSupertypeBaseOffset(from);

					if (alignment == null || alignment < 0)
					{
						throw new ApplicationException("Could not add virtual function header");
					}

					if (alignment != 0)
					{
						var self = References.GetVariable(unit, unit.Self ?? throw new ApplicationException("Missing self pointer"));
						var offset = References.GetConstant(unit, new NumberNode(Format, (long)alignment));

						// Convert the self pointer to the type 'to' by offsetting it
						unit.Append(new SubtractionInstruction(unit, self, offset, Format, true));
					}
				}

				unit.Append(new LabelInstruction(unit, new Label(fullname)));

				// Initialize this function
				unit.Append(new InitializeInstruction(unit));

				// Parameters are active from the start of the function, so they must be required now otherwise they would become active at their first usage
				var variables = unit.Function.Parameters;

				if ((unit.Function.Metadata!.IsMember || implementation.IsLambda) && !unit.Function.Metadata!.IsConstructor)
				{
					variables.Add(unit.Self ?? throw new ApplicationException("Missing self pointer in a member function"));
				}

				unit.Append(new RequireVariablesInstruction(unit, variables));

				Builders.Build(unit, implementation.Node!);
			});

			// Sprinkle a little intelligence into the output code
			Oracle.Channel(unit);

			Oracle.SimulateLifetimes(unit);
			unit.Simulate(UnitPhase.BUILD_MODE, instruction => { instruction.Build(); });

			builder.Append(Translator.Translate(unit, out List<ConstantDataSectionHandle> constant_handles));
			builder.AppendLine();

			constants.AddRange(constant_handles);
		}

		out_constants = constants;

		return builder.Length == 0 ? string.Empty : builder.ToString();
	}

	private static string GetText(Context context, out List<ConstantDataSectionHandle> constants)
	{
		var builder = new StringBuilder();

		constants = new List<ConstantDataSectionHandle>();

		foreach (var function in context.Functions.Values.SelectMany(l => l.Overloads).Where(f => !f.IsImported))
		{
			builder.Append(GetText(function, out List<ConstantDataSectionHandle> function_constants));
			builder.Append(SEPARATOR);

			constants.AddRange(function_constants);
		}

		foreach (var type in context.Types.Values)
		{
			foreach (var constructor in type.Constructors.Overloads)
			{
				builder.Append(GetText(constructor, out List<ConstantDataSectionHandle> constructor_constants));
				builder.Append(SEPARATOR);

				constants.AddRange(constructor_constants);
			}

			builder.Append(GetText(type, out List<ConstantDataSectionHandle> type_constants));

			constants.AddRange(type_constants);
		}

		return Regex.Replace(builder.ToString().Replace("\r\n", "\n"), "\n{3,}", "\n\n");
	}

	private static string GetStaticVariables(Type type)
	{
		var builder = new StringBuilder();

		foreach (var variable in type.Variables.Values)
		{
			if (!variable.IsStatic)
			{
				continue;
			}

			var name = variable.GetStaticName();
			var allocator = Size.FromBytes(variable.Type!.ReferenceSize).Allocator;

			builder.AppendLine($"{name}:");
			builder.AppendLine($"{allocator} 0");
		}

		foreach (var subtype in type.Supertypes)
		{
			builder.Append(SEPARATOR);
			builder.AppendLine(GetStaticVariables(subtype));
		}

		return builder.ToString();
	}

	private static IEnumerable<StringNode> GetStringNodes(Node root)
	{
		return root.FindAll(n => n is StringNode).Select(n => (StringNode)n) ?? new List<StringNode>();
	}

	private static IEnumerable<StringNode> GetStringNodes(Context context)
	{
		var nodes = new List<StringNode>();

		foreach (var implementation in context.GetImplementedFunctions())
		{
			nodes.AddRange(GetStringNodes(implementation));
			nodes.AddRange(GetStringNodes(implementation.Node!));
		}

		foreach (var type in context.Types.Values)
		{
			nodes.AddRange(GetStringNodes(type));
		}

		return nodes;
	}

	private static string AllocateString(string text)
	{
		var builder = new StringBuilder();
		var position = 0;

		while (position < text.Length)
		{
			var buffer = new string(text.Skip(position).TakeWhile(i => i != '\\').ToArray());
			position += buffer.Length;

			if (buffer.Length > 0)
			{
				builder.AppendLine($"{STRING_ALLOCATOR} \"{buffer}\"");
			}

			if (position >= text.Length)
			{
				break;
			}

			position++; // Skip character '\'

			var command = text[position++];
			var length = 0;
			var error = string.Empty;

			if (command == 'x')
			{
				length = 2;
				error = "Could not understand hexadecimal value in a string";
			}
			else if (command == 'u')
			{
				length = 4;
				error = "Could not understand unicode character in a string";
			}
			else if (command == 'U')
			{
				length = 8;
				error = "Could not understand unicode character in a string";
			}
			else
			{
				Errors.Abort("Could not understand string command");
			}

			var hexadecimal = text.Substring(position, length);

			if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value))
			{
				Errors.Abort(error);
			}

			var bytes = BitConverter.GetBytes(value).Take(length / 2).ToArray();
			bytes.ForEach(i => builder.AppendLine($"{Size.BYTE.Allocator} {i}"));

			position += length;
		}

		return builder.Append($"{Size.BYTE.Allocator} 0").ToString();
	}

	private static string AppendTableLable(TableLabel label)
	{
		if (label.Declare)
		{
			return $"{label.Name}:";
		}

		if (label.IsSecrel)
		{
			return SECREL_DIRECTIVE + label.Size.Bits.ToString(CultureInfo.InvariantCulture) + ' ' + label.Name;
		}

		return $"{label.Size.Allocator} {label.Name}";
	}

	public static void AppendTable(StringBuilder builder, Table table)
	{
		if (table.IsBuilt)
		{
			return;
		}

		table.IsBuilt = true;

		if (table.IsSection)
		{
			builder.AppendLine(SECTION_DIRECTIVE + ' ' + table.Name);
		}
		else
		{
			builder.AppendLine(table.Name + ':');
		}

		var subtables = new List<Table>();

		foreach (var item in table.Items)
		{
			var result = item switch
			{
				string a => AllocateString(a),
				long b => $"{Size.QWORD.Allocator} {b}",
				int c => $"{Size.DWORD.Allocator} {c}",
				short d => $"{Size.WORD.Allocator} {d}",
				byte e => $"{Size.BYTE.Allocator} {e}",
				Table f => $"{Size.Allocator} {f.Name}",
				Label g => $"{Size.Allocator} {g.GetName()}",
				Offset h => $"{Size.DWORD.Allocator} {h.To.Name} - {h.From.Name}",
				TableLabel i => AppendTableLable(i),
				_ => throw new ApplicationException("Invalid table item")
			};

			builder.Append(result);
			builder.AppendLine();

			if (item is Table subtable && !subtable.IsBuilt)
			{
				subtables.Add(subtable);
			}
		}

		builder.Append(SEPARATOR);

		subtables.ForEach(i => AppendTable(builder, i));
	}

	private static string GetData(Context context)
	{
		var builder = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			builder.AppendLine(GetStaticVariables(type));
			builder.Append(SEPARATOR);
		}

		foreach (var type in context.Types.Values)
		{
			if (type.Configuration != null)
			{
				AppendTable(builder, type.Configuration.Entry);
			}
		}

		var nodes = GetStringNodes(context);

		foreach (var node in nodes)
		{
			if (node.Identifier == null)
			{
				continue;
			}

			var name = node.Identifier;
			var allocator = Size.BYTE.Allocator;

			// Align every data label for now since some instructions need them to be that way
			builder.AppendLine($"{BYTE_ALIGNMENT} 16");
			builder.AppendLine($"{name}:");
			builder.AppendLine(AllocateString(node.Text));
		}

		return builder.ToString();
	}

	private static string GetConstantData(List<ConstantDataSectionHandle> constants)
	{
		var builder = new StringBuilder();

		foreach (var constant in constants)
		{
			var name = constant.Identifier;

			string? allocator = null;
			string? text = null;

			if (constant.Value is byte[] x)
			{
				text = string.Join(", ", x.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray());
				allocator = Size.BYTE.Allocator;
			}
			else if (constant.Value is double y)
			{
				var bytes = BitConverter.GetBytes(y);
				allocator = Size.BYTE.Allocator;

				text = string.Join(", ", bytes.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray());

				var value = y.ToString(CultureInfo.InvariantCulture);

				if (!value.Contains('.') && !value.Contains('E'))
				{
					value += ".0";
				}

				text += $" {COMMENT} {value}";
			}
			else if (constant.Value is float z)
			{
				var bytes = BitConverter.GetBytes(z);
				allocator = Size.BYTE.Allocator;

				text = string.Join(", ", bytes.Select(i => i.ToString(CultureInfo.InvariantCulture)).ToArray());

				var value = z.ToString(CultureInfo.InvariantCulture);

				if (!value.Contains('.') && !value.Contains('E'))
				{
					value += ".0";
				}

				text += $" {COMMENT} {value}";
			}
			else
			{
				text = constant.Value.ToString()!.Replace(',', '.');
				allocator = constant.Size.Allocator;
			}

			// Align every data label for now since some instructions need them to be that way
			builder.AppendLine($"{BYTE_ALIGNMENT} 16");
			builder.AppendLine($"{name}:");
			builder.AppendLine($"{allocator} {text}");
		}

		return builder.ToString();
	}

	public static void AppendFunctionDebugInfo(Debug debug, FunctionImplementation implementation)
	{
		debug.AppendFunction(implementation);

		foreach (var iterator in implementation.GetImplementedFunctions())
		{
			if (iterator.Metadata.IsImported)
			{
				continue;
			}

			AppendFunctionDebugInfo(debug, iterator);
		}
	}

	public static void AppendTypeDebugInfo(Debug debug, Type type)
	{
		debug.AppendType(type);

		foreach (var iterator in type.Types.Values)
		{
			AppendTypeDebugInfo(debug, iterator);
		}
	}

	public static void AppendDebugInfo(StringBuilder builder, Context context)
	{
		var debug = new Debug("Sandbox.v", "C:/Users/joona/vivid/Vivid/Examples");

		foreach (var iterator in context.GetImplementedFunctions())
		{
			if (iterator.Metadata.IsImported)
			{
				continue;
			}
			
			AppendFunctionDebugInfo(debug, iterator);
		}

		foreach (var iterator in context.Types.Values.Distinct())
		{
			AppendTypeDebugInfo(debug, iterator);
		}

		debug.Export(builder);
	}

	public static string Assemble(Context context)
	{
		AllocationFunction = context.GetFunction(ALLOCATE_FUNCTION_IDENTIFIER)?.Overloads[0] ??
							 throw new ApplicationException("Allocation function was missing");

		var builder = new StringBuilder();

		builder.AppendLine(TEXT_SECTION);

		var entry = context.GetFunction(Keywords.INIT.Identifier)!.Overloads.First().Implementations.First();
		var header = IsTargetWindows ? WINDOWS_TEXT_SECTION_HEADER : LINUX_TEXT_SECTION_HEADER;

		builder.AppendLine(string.Format(CultureInfo.InvariantCulture, header, entry.GetFullname()));

		builder.Append(GetExternalFunctions(context));
		builder.Append(GetText(context, out List<ConstantDataSectionHandle> constants));
		builder.Append(SEPARATOR);

		if (Assembler.IsDebuggingEnabled)
		{
			builder.AppendLine($"{Debug.COMPILATION_UNIT_END}:");
		}

		var data = GetData(context) + GetConstantData(constants);

		if (Regex.IsMatch(data, "[a-zA-z0-9]"))
		{
			builder.AppendLine(DATA_SECTION);
			builder.Append(data);
			builder.Append(SEPARATOR);
		}

		if (Assembler.IsDebuggingEnabled)
		{
			AppendDebugInfo(builder, context);
		}
		
		return Regex.Replace(builder.Replace("\r\n", "\n").ToString(), "\n{3,}", "\n\n");
	}
}