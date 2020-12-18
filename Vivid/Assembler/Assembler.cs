using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System;
using System.Globalization;

public static class Assembler
{
	public static Function? AllocationFunction { get; set; }
	public static Size Size { get; set; } = Size.QWORD;
	public static Format Format => Size.ToFormat();
	public static OSPlatform Target { get; set; } = OSPlatform.Windows;
	public static bool IsTargetWindows => Target == OSPlatform.Windows;
	public static bool IsTargetLinux => Target == OSPlatform.Linux;
	public static bool IsTargetX86 => Size.Bits == 32;
	public static bool IsTargetX64 => Size.Bits == 64;
	public static bool IsDebuggingEnabled { get; set; } = false;
	public static bool IsVerboseOutputEnabled { get; set; } = false;

	private const string SECTION_DIRECTIVE = ".section";
	private const string SECREL_DIRECTIVE = ".secrel";
	private const string TEXT_SECTION_DIRECTIVE = SECTION_DIRECTIVE + " .text";
	private const string SYNTAX_REQUIREMENT_DIRECTIVE = ".intel_syntax noprefix";
	private const string FILE_DIRECTIVE = ".file";
	private const string STRING_ALLOCATOR_DIRECTIVE = ".ascii";
	private const string BYTE_ALIGNMENT_DIRECTIVE = ".balign";
	private const string COMMENT = "#";

	private const string FORMAT_LINUX_TEXT_SECTION_HEADER = 
		".global _start" + "\n" +
		"_start:" + "\n" +
		"call {0}" + "\n" +
		"mov rax, 60" + "\n" +
		"xor rdi, rdi" + "\n" +
		"syscall" + SEPARATOR;

	private const string FORMAT_WINDOWS_TEXT_SECTION_HEADER =
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

	private static string GetTextSection(Function function, List<ConstantDataSectionHandle> constants)
	{
		var builder = new StringBuilder();

		foreach (var implementation in function.Implementations)
		{
			if (implementation.IsInlined || implementation.IsEmpty)
			{
				continue;
			}

			// Ensure this function is visible to other units
			builder.AppendLine($".global {implementation.GetFullname()}");

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
						var self = References.GetVariable(unit, unit.Self ?? throw new ApplicationException("Missing self pointer"), AccessMode.READ);
						var offset = References.GetConstant(unit, new NumberNode(Format, (long)alignment));

						// Convert the self pointer to the type 'to' by offsetting it
						unit.Append(new SubtractionInstruction(unit, self, offset, Format, true));
					}
				}

				unit.Append(new LabelInstruction(unit, new Label(fullname)));

				// Initialize this function
				unit.Append(new InitializeInstruction(unit));

				// Parameters are active from the start of the function, so they must be required now otherwise they would become active at their first usage
				var parameters = unit.Function.Parameters;

				if ((unit.Function.Metadata!.IsMember || implementation.IsLambda) && !unit.Function.Metadata!.IsConstructor)
				{
					parameters.Add(unit.Self ?? throw new ApplicationException("Missing self pointer in a member function"));
				}

				unit.Append(new RequireVariablesInstruction(unit, parameters));

				if (Assembler.IsDebuggingEnabled)
				{
					Calls.MoveParametersToStack(unit);
				}

				Builders.Build(unit, implementation.Node!);
			});

			Oracle.Channel(unit);

			Oracle.SimulateLifetimes(unit);
			unit.Simulate(UnitPhase.BUILD_MODE, instruction => { instruction.Build(); });

			builder.Append(Translator.Translate(unit, constants));
			builder.AppendLine();
		}

		return builder.Length == 0 ? string.Empty : builder.ToString();
	}

	private static Dictionary<File, string> GetTextSections(Context context, out Dictionary<File, List<ConstantDataSectionHandle>> constant_sections)
	{
		constant_sections = new Dictionary<File, List<ConstantDataSectionHandle>>();

		var files = GetAllFunctions(context).Where(i => !i.IsImported && i.Position != null).GroupBy(i => i.Position!.File ?? throw new ApplicationException("Missing declaration file"));
		var text_sections = new Dictionary<File, string>();

		foreach (var iterator in files)
		{
			var constants = new List<ConstantDataSectionHandle>();
			var builder = new StringBuilder();
			var file = iterator.Key;

			foreach (var function in iterator)
			{
				builder.Append(GetTextSection(function, constants));
				builder.Append(SEPARATOR);
			}

			text_sections.Add(file, builder.ToString());
			constant_sections.Add(file, constants);
		}

		return text_sections;
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

			builder.AppendLine($"{name}: {allocator} 0");
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
		return root.FindAll(n => n.Is(NodeType.STRING)).Cast<StringNode>();
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
				builder.AppendLine($"{STRING_ALLOCATOR_DIRECTIVE} \"{buffer}\"");
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
				throw new ApplicationException($"Could not understand string command '{command}'");
			}

			var hexadecimal = text.Substring(position, length);

			if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value))
			{
				throw new ApplicationException(error);
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

	/// <summary>
	/// Constructs file specific data sections based on the specified context
	/// </summary>
	private static Dictionary<File, string> GetDataSections(Context context)
	{
		var sections = new Dictionary<File, StringBuilder>();
		var types = context.Types.Values.Where(i => i.Position != null).GroupBy(i => i.Position?.File ?? throw new ApplicationException("Missing type declaration file"));

		// Append static variables
		foreach (var iterator in types)
		{
			var builder = new StringBuilder();

			foreach (var type in iterator)
			{
				builder.AppendLine(GetStaticVariables(type));
				builder.Append(SEPARATOR);
			}

			sections.Add(iterator.Key, builder);
		}

		// Append runtime information about types
		foreach (var iterator in types)
		{
			var builder = sections[iterator.Key];

			foreach (var type in iterator)
			{
				if (type.Configuration == null)
				{
					continue;
				}

				AppendTable(builder, type.Configuration.Entry);
			}
		}

		// Append all strings into the data section
		var functions = GetAllFunctionImplementations(context).Where(i => !i.Metadata!.IsImported)
			.GroupBy(i => i.Metadata!.Position?.File ?? throw new ApplicationException("Missing type declaration file"));

		foreach (var iterator in functions)
		{
			var nodes = new List<StringNode>();

			foreach (var implementation in iterator.Where(i => i.Node != null))
			{
				nodes.AddRange(GetStringNodes(implementation.Node!));
			}

			var builder = sections.GetValueOrDefault(iterator.Key, new StringBuilder())!;

			foreach (var node in nodes)
			{
				if (node.Identifier == null)
				{
					continue;
				}

				var name = node.Identifier;
				var allocator = Size.BYTE.Allocator;

				// Align every data label for now since some instructions need them to be that way
				builder.AppendLine($"{BYTE_ALIGNMENT_DIRECTIVE} 16");
				builder.AppendLine($"{name}:");
				builder.AppendLine(AllocateString(node.Text));
			}

			sections[iterator.Key] = builder;
		}

		return sections.ToDictionary(i => i.Key, i => i.Value.ToString());
	}

	/// <summary>
	/// Constructs data section for the specified constants
	/// </summary>
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
			builder.AppendLine($"{BYTE_ALIGNMENT_DIRECTIVE} 16");
			builder.AppendLine($"{name}:");
			builder.AppendLine($"{allocator} {text}");
		}

		return builder.ToString();
	}

	/// <summary>
	/// Appends debug information about the specified function
	/// </summary>
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

	/// <summary>
	/// Appends debug information about the specified type
	/// </summary>
	public static void AppendTypeDebugInfo(Debug debug, Type type)
	{
		debug.AppendType(type);

		foreach (var iterator in type.Types.Values)
		{
			AppendTypeDebugInfo(debug, iterator);
		}

		foreach (var iterator in type.GetImplementedFunctions())
		{
			if (iterator.Metadata.IsImported)
			{
				continue;
			}
			
			AppendFunctionDebugInfo(debug, iterator);
		}
	}

	/// <summary>
	/// Collects all types and subtypes from the specified context
	/// </summary>
	public static List<Type> GetAllTypes(Context context)
	{
		var result = context.Types.Values.ToList();
		result.AddRange(result.SelectMany(i => GetAllTypes(i)));

		return result;
	}
	
	/// <summary>
	/// Collects all function implementations from the specified context
	/// </summary>
	public static FunctionImplementation[] GetAllFunctionImplementations(Context context)
	{
		var types = GetAllTypes(context);
		
		// Collect all functions, constructors, destructors and virtual functions
		var type_functions = types.SelectMany(i => i.Functions.Values.SelectMany(j => j.Overloads));
		var type_constructors = types.SelectMany(i => i.Constructors.Overloads);
		var type_destructors = types.SelectMany(i => i.Destructors.Overloads);
		var type_virtual_functions = types.SelectMany(i => i.Virtuals.Values.SelectMany(j => j.Overloads));
		var context_functions = context.Functions.Values.SelectMany(i => i.Overloads);

		var implementations = type_functions.Concat(type_constructors).Concat(type_destructors).Concat(type_virtual_functions).Concat(context_functions).SelectMany(i => i.Implementations).ToArray();

		// Concat all functions with lambdas, which can be found inside the collected functions
		return implementations.Concat(implementations.SelectMany(i => GetAllFunctionImplementations(i))).Distinct().ToArray();
	}

	/// <summary>
	/// Collects all function implementations from the specified context
	/// </summary>
	public static Function[] GetAllFunctions(Context context)
	{
		return GetAllFunctionImplementations(context).Select(i => i.Metadata!).Distinct().ToArray();
	}

	public static Dictionary<File, string> GetDebugSections(Context context)
	{
		var sections = new Dictionary<File, string>();

		if (!Assembler.IsDebuggingEnabled)
		{
			return sections;
		}

		var all_types = GetAllTypes(context).Distinct();
		var base_types = all_types.Where(i => i.Position == null).ToArray();

		var types = all_types.Where(i => i.Position != null)
			.GroupBy(i => i.Position!.File ?? throw new ApplicationException("Missing declaration file")).Cast<IGrouping<File, object>>();

		var functions = GetAllFunctionImplementations(context).Where(i => i.Metadata!.Position != null)
			.GroupBy(i => i.Metadata!.Position!.File ?? throw new ApplicationException("Missing declaration file")).Cast<IGrouping<File, object>>();

		var files = types.Concat(functions).GroupBy(i => i.Key, i => (IEnumerable<object>)i).ToArray();

		foreach (var file in files)
		{
			var debug = new Debug();

			debug.BeginFile(file.Key);

			foreach (var iterator in file.SelectMany(i => i))
			{
				if (iterator is Type type)
				{
					AppendTypeDebugInfo(debug, type);
				}
				else if (iterator is FunctionImplementation implementation)
				{
					if (implementation.Metadata!.IsImported)
					{
						continue;
					}

					AppendFunctionDebugInfo(debug, implementation);
				}
				else
				{
					throw new ApplicationException("Unknown debug information element");
				}
			}

			foreach (var base_type in base_types)
			{
				AppendTypeDebugInfo(debug, base_type);
			}

			debug.EndFile();

			sections.Add(file.Key, debug.Export());
		}

		return sections;
	}

	public static Dictionary<File, string> Assemble(Context context, File[] files)
	{
		var result = new Dictionary<File, string>();

		var entry_function = context.GetFunction(Keywords.INIT.Identifier)!.Overloads.First().Implementations.First();
		var entry_function_file = entry_function.Metadata.Position?.File ?? throw new ApplicationException("Entry function declaration file missing");

		var text_sections = GetTextSections(context, out Dictionary<File, List<ConstantDataSectionHandle>> constant_sections);
		var data_sections = GetDataSections(context);
		var debug_sections = GetDebugSections(context);

		foreach (var file in files)
		{
			var builder = new StringBuilder();
			var is_data_section = false;

			builder.AppendLine(TEXT_SECTION_DIRECTIVE);
			builder.AppendLine(SYNTAX_REQUIREMENT_DIRECTIVE);

			if (Assembler.IsDebuggingEnabled)
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, Debug.FORMAT_COMPILATION_UNIT_START, file.Index);
				builder.AppendLine(":");

				var fullname = file.Fullname;

				if (fullname.StartsWith(Environment.CurrentDirectory))
				{
					fullname = fullname.Remove(0, Environment.CurrentDirectory.Length);
					fullname = fullname.Insert(0, ".");
				}

				builder.AppendLine(FILE_DIRECTIVE + " 1 " + $"\"{fullname.Replace('\\', '/')}\"");
			}

			if (entry_function_file == file)
			{
				var header = IsTargetWindows ? FORMAT_WINDOWS_TEXT_SECTION_HEADER : FORMAT_LINUX_TEXT_SECTION_HEADER;

				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, header, entry_function.GetFullname()));
				builder.Append(GetExternalFunctions(context));
			}

			if (text_sections.TryGetValue(file, out string? text_section))
			{
				builder.Append(text_section);
				builder.Append(SEPARATOR);
			}

			if (Assembler.IsDebuggingEnabled)
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, Debug.FORMAT_COMPILATION_UNIT_END, file.Index);
				builder.AppendLine(":");
			}

			if (data_sections.TryGetValue(file, out string? data_section))
			{
				if (!is_data_section)
				{
					builder.AppendLine(DATA_SECTION);
					is_data_section = true;
				}

				builder.Append(data_section);
				builder.Append(SEPARATOR);
			}

			if (constant_sections.TryGetValue(file, out List<ConstantDataSectionHandle>? constant_section))
			{
				if (!is_data_section)
				{
					builder.AppendLine(DATA_SECTION);
					is_data_section = true;
				}

				builder.Append(GetConstantData(constant_section));
				builder.Append(SEPARATOR);
			}

			if (Assembler.IsDebuggingEnabled && debug_sections.TryGetValue(file, out string? debug_section))
			{
				builder.Append(debug_section);
				builder.Append(SEPARATOR);
			}
			
			result.Add(file, Regex.Replace(builder.ToString().Replace("\r\n", "\n"), "\n{3,}", "\n\n"));
		}
		
		return result;
	}
}