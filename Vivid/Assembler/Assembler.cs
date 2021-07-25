using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public static class Assembler
{
	public static Function? AllocationFunction { get; set; }
	public static Function? DeallocationFunction { get; set; }
	public static FunctionImplementation? InitializationFunction { get; set; }

	public static Size Size { get; set; } = Size.QWORD;
	public static Format Format => Size.ToFormat();
	public static OSPlatform Target { get; set; } = OSPlatform.Windows;
	public static Architecture Architecture { get; set; } = RuntimeInformation.ProcessArchitecture;

	public static bool Is32Bit => Size.Bits == 32;
	public static bool Is64Bit => Size.Bits == 64;

	public static bool IsTargetWindows => Target == OSPlatform.Windows;
	public static bool IsTargetLinux => Target == OSPlatform.Linux;

	public static bool IsArm64 => Architecture == Architecture.Arm64;
	public static bool IsX64 => Architecture == Architecture.X64;

	public static bool IsPositionIndependent { get; set; } = false;

	public static bool IsDebuggingEnabled { get; set; } = false;
	public static bool IsVerboseOutputEnabled { get; set; } = false;

	private const string SECTION_DIRECTIVE = ".section";
	private const string SECREL_DIRECTIVE = ".secrel";
	private const string EXPORT_DIRECTIVE = ".global";
	private const string TEXT_SECTION_DIRECTIVE = SECTION_DIRECTIVE + " .text";
	private const string SYNTAX_REQUIREMENT_DIRECTIVE = ".intel_syntax noprefix";
	private const string FILE_DIRECTIVE = ".file";
	private const string STRING_ALLOCATOR_DIRECTIVE = ".ascii";
	private const string BYTE_ALIGNMENT_DIRECTIVE = ".balign";
	private const string POWER_OF_TWO_ALIGNMENT = ".align";
	private const string BYTE_ZERO_ALLOCATOR = ".zero";
	private const string ARM64_COMMENT = "//";
	private const string X64_COMMENT = "#";
	public static string Comment { get; set; } = X64_COMMENT;

	private const string FORMAT_X64_LINUX_TEXT_SECTION_HEADER =
		".global _start" + "\n" +
		"_start:" + "\n" +
		"{0}" + "\n" +
		"mov rax, 60" + "\n" +
		"xor rdi, rdi" + "\n" +
		"syscall" + SEPARATOR;

	private const string FORMAT_X64_WINDOWS_TEXT_SECTION_HEADER =
		".global main" + "\n" +
		"main:" + "\n" +
		"{0}" + SEPARATOR;

	private const string FORMAT_ARM64_LINUX_TEXT_SECTION_HEADER =
		".global _start" + "\n" +
		"_start:" + "\n" +
		"{0}" + "\n" +
		"mov x8, #93" + "\n" +
		"mov x0, xzr" + "\n" +
		"svc #0" + SEPARATOR;

	private const string FORMAT_ARM64_WINDOWS_TEXT_SECTION_HEADER =
		".global main" + "\n" +
		"main:" + "\n" +
		"{0}" + SEPARATOR;

	private const string DATA_SECTION = SECTION_DIRECTIVE + " .data";
	private const string SEPARATOR = "\n\n";

	/// <summary>
	/// Creates a mangled text which describes the specified virtual function and appends it to the specified mangle object
	/// </summary>
	private static void ExportVirtualFunction(Mangle mangle, VirtualFunction function)
	{
		mangle += Mangle.START_MEMBER_VIRTUAL_FUNCTION_COMMAND;
		mangle += $"{function.Name}{function.Name}";

		/// NOTE: All parameters must have a type since that is a requirement for virtual functions
		mangle += function.Parameters.Select(i => i.Type!);
		
		if (!Primitives.IsPrimitive(function.ReturnType, Primitives.UNIT))
		{
			mangle += Mangle.START_RETURN_TYPE_COMMAND;
			mangle += function.ReturnType ?? throw new ApplicationException("Virtual function missing return type");
		}

		mangle += Mangle.END_COMMAND;
	}

	/// <summary>
	/// Creates a mangled text which describes the specified type and appends it to the specified builder
	/// </summary>
	private static string? ExportType(StringBuilder builder, Type type)
	{
		// Skip template types since they will be exported in a different way
		if (type.IsTemplateType)
		{
			return null;
		}

		var mangle = new Mangle(Mangle.EXPORT_TYPE_TAG);
		mangle.Add(type);

		var member_variables = type.Variables.Values.Where(i => !i.IsStatic && !i.IsHidden).ToArray();
		var virtual_functions = type.Virtuals.Values.ToArray();

		var public_member_variables = member_variables.Where(i => i.IsPublic).ToArray();
		var public_virtual_functions = virtual_functions.SelectMany(i => i.Overloads).Where(i => i.IsPublic).ToArray();

		var private_member_variables = member_variables.Where(i => i.IsPrivate).ToArray();
		var private_virtual_functions = virtual_functions.SelectMany(i => i.Overloads).Where(i => i.IsPrivate).ToArray();

		var protected_member_variables = member_variables.Where(i => i.IsProtected).ToArray();
		var protected_virtual_functions = virtual_functions.SelectMany(i => i.Overloads).Where(i => i.IsProtected).ToArray();

		// Export all public member variables
		foreach (var variable in public_member_variables)
		{
			mangle += Mangle.START_MEMBER_VARIABLE_COMMAND;
			mangle += $"{variable.Name.Length}{variable.Name}";
			mangle += variable.Type!;
		}

		// Export all public virtual functions
		foreach (var function in public_virtual_functions)
		{
			ExportVirtualFunction(mangle, (VirtualFunction)function);
		}

		var is_private_section_empty = !private_member_variables.Any() && !private_virtual_functions.Any();
		var is_protected_section_empty = !protected_member_variables.Any() && !protected_virtual_functions.Any();

		if (is_private_section_empty && is_protected_section_empty)
		{
			builder.AppendLine($"{EXPORT_DIRECTIVE} {mangle.Value}");
			builder.AppendLine($"{mangle.Value}:");
			return mangle.Value;
		}

		mangle += Mangle.END_COMMAND;

		// Export all private member variables
		foreach (var variable in private_member_variables)
		{
			mangle += Mangle.START_MEMBER_VARIABLE_COMMAND;
			mangle += $"{variable.Name.Length}{variable.Name}";
			mangle += variable.Type!;
		}

		// Export all private virtual functions
		foreach (var function in private_virtual_functions)
		{
			ExportVirtualFunction(mangle, (VirtualFunction)function);
		}

		if (is_protected_section_empty)
		{
			builder.AppendLine($"{EXPORT_DIRECTIVE} {mangle.Value}");
			builder.AppendLine($"{mangle.Value}:");
			return mangle.Value;
		}

		mangle += Mangle.END_COMMAND;

		// Export all protected member variables
		foreach (var variable in protected_member_variables)
		{
			mangle += Mangle.START_MEMBER_VARIABLE_COMMAND;
			mangle += $"{variable.Name.Length}{variable.Name}";
			mangle += variable.Type!;
		}

		// Export all protected virtual functions
		foreach (var function in protected_virtual_functions)
		{
			ExportVirtualFunction(mangle, (VirtualFunction)function);
		}
		
		builder.AppendLine($"{EXPORT_DIRECTIVE} {mangle.Value}");
		builder.AppendLine($"{mangle.Value}:");

		return mangle.Value;
	}

	/// <summary>
	/// Creates a template name by combining the specified name and the template argument names together
	/// </summary>
	private static string CreateTemplateName(string name, IEnumerable<string> template_argument_names)
	{
		return name + '<' + string.Join(", ", template_argument_names) + '>';
	}

	/// <summary>
	/// Converts the specified modifiers into source code
	/// </summary>
	private static string GetModifiers(int modifiers)
	{
		var result = new List<string>();
		if (Flag.Has(modifiers, Modifier.EXPORTED)) result.Add(Keywords.EXPORT.Identifier);
		if (Flag.Has(modifiers, Modifier.INLINE)) result.Add(Keywords.INLINE.Identifier);
		if (Flag.Has(modifiers, Modifier.IMPORTED)) result.Add(Keywords.IMPORT.Identifier);
		if (Flag.Has(modifiers, Modifier.OUTLINE)) result.Add(Keywords.OUTLINE.Identifier);
		if (Flag.Has(modifiers, Modifier.PRIVATE)) result.Add(Keywords.PRIVATE.Identifier);
		if (Flag.Has(modifiers, Modifier.PROTECTED)) result.Add(Keywords.PROTECTED.Identifier);
		if (Flag.Has(modifiers, Modifier.PUBLIC)) result.Add(Keywords.PUBLIC.Identifier);
		if (Flag.Has(modifiers, Modifier.READONLY)) result.Add(Keywords.READONLY.Identifier);
		if (Flag.Has(modifiers, Modifier.STATIC)) result.Add(Keywords.STATIC.Identifier);
		return string.Join(' ', result);
	}

	/// <summary>
	/// Exports the specified template function which may have the specified parent type
	/// </summary>
	private static void ExportTemplateFunction(StringBuilder builder, TemplateFunction function)
	{
		builder.Append(GetModifiers(function.Modifiers));
		builder.Append(' ');
		builder.Append(CreateTemplateName(function.Name, function.TemplateParameters));
		builder.Append(ParenthesisType.PARENTHESIS.Opening);
		builder.Append(string.Join(", ", function.Parameters.Select(i => i.Export())));
		builder.Append(ParenthesisType.PARENTHESIS.Closing);
		builder.Append(' ');
		builder.Append(string.Join(' ', function.Blueprint.Skip(1)) + Lexer.LINE_ENDING);
		builder.Append(Lexer.LINE_ENDING);
	}

	/// <summary>
	/// Exports the specified short template function which may have the specified parent type
	/// </summary>
	private static void ExportShortTemplateFunction(StringBuilder builder, Function function)
	{
		builder.Append(GetModifiers(function.Modifiers));
		builder.Append(' ');
		builder.Append(function.Name);
		builder.Append(ParenthesisType.PARENTHESIS.Opening);
		builder.Append(string.Join(", ", function.Parameters.Select(i => i.Export())));
		builder.Append(ParenthesisType.PARENTHESIS.Closing);
		builder.Append(' ');
		builder.Append(ParenthesisType.CURLY_BRACKETS.Opening);
		builder.Append(Lexer.LINE_ENDING);
		builder.Append(string.Join(' ', function.Blueprint) + Lexer.LINE_ENDING);
		builder.Append(ParenthesisType.CURLY_BRACKETS.Closing);
		builder.Append(Lexer.LINE_ENDING);
	}

	/// <summary>
	/// Exports the specified template type
	/// </summary>
	private static void ExportTemplateType(StringBuilder builder, TemplateType type)
	{
		builder.Append(CreateTemplateName(type.Name, type.TemplateParameters));
		builder.Append(string.Join(' ', type.Blueprint.Skip(1)) + Lexer.LINE_ENDING);
		builder.Append(Lexer.LINE_ENDING);
	}

	/// <summary>
	/// Returns true if the specified function represents an actual template function or if any of its parameter types is not defined
	/// </summary>
	private static bool IsTemplateFunction(Function function)
	{
		return function is TemplateFunction || function.Parameters.Any(i => i.Type == null);
	}
	
	/// <summary>
	/// Looks for template functions and types and exports them to string builders
	/// </summary>
	public static Dictionary<SourceFile, StringBuilder> GetTemplateExportFiles(Context context)
	{
		var files = new Dictionary<SourceFile, StringBuilder>();
		var functions = context.Functions.Values.SelectMany(i => i.Overloads).Where(i => IsTemplateFunction(i) && i.Start?.File != null).GroupBy(i => i.Start!.File!);

		foreach (var iterator in functions)
		{
			var builder = new StringBuilder();

			foreach (var function in iterator)
			{
				if (function is TemplateFunction template)
				{
					ExportTemplateFunction(builder, template);
				}
				else
				{
					ExportShortTemplateFunction(builder, function);
				}
			}

			files.Add(iterator.Key!, builder);
		}

		var types = Common.GetAllTypes(context).Where(i => i.Position?.File != null).GroupBy(i => i.Position!.File!).ToArray();

		foreach (var iterator in types)
		{
			foreach (var type in iterator)
			{
				var templates = type.Functions.Values.SelectMany(i => i.Overloads).Where(IsTemplateFunction).ToArray();

				if (!templates.Any())
				{
					continue;
				}

				if (!files.TryGetValue(iterator.Key!, out StringBuilder? builder))
				{
					builder = new StringBuilder();
					files.Add(iterator.Key!, builder);
				}

				builder.Append(type.Name);
				builder.Append(' ');
				builder.Append(ParenthesisType.CURLY_BRACKETS.Opening);

				foreach (var function in templates)
				{
					if (function is TemplateFunction template)
					{
						ExportTemplateFunction(builder, template);
					}
					else
					{
						ExportShortTemplateFunction(builder, function);
					}
				}

				builder.Append(ParenthesisType.CURLY_BRACKETS.Closing);
			}
		}

		foreach (var iterator in types)
		{
			foreach (var type in iterator)
			{
				if (!type.IsTemplateType)
				{
					continue;
				}

				if (!files.TryGetValue(iterator.Key!, out StringBuilder? builder))
				{
					builder = new StringBuilder();
					files.Add(iterator.Key!, builder);
				}

				ExportTemplateType(builder, type.To<TemplateType>());
			}
		}

		return files;
	}

	/// <summary>
	/// Returns all symbols which are exported from the specified context
	/// </summary>
	public static Dictionary<SourceFile, List<string>> GetExportedSymbols(Context context)
	{
		// Collect all the types which have a file registered
		var types = Common.GetAllTypes(context).Where(i => i.Position?.File != null).ToArray();

		// Collect all the type configuration table names and group them by their files
		var configurations = types.Where(i => i.Configuration != null).GroupBy(i => i.Position!.File!).ToDictionary(i => i.Key, i => i.Select(i => i.Configuration!.Entry.Name).ToList());
		
		// Collect all the static variable names and group them by their files
		var statics = types.SelectMany(i => i.Variables.Values).Where(i => i.IsStatic).GroupBy(i => i.Position!.File!).ToDictionary(i => i.Key, i => i.Select(i => i.GetStaticName()).ToList());
		
		// Collect all the function names and group them by their files
		var functions = Common.GetAllFunctionImplementations(context).Where(i => i.Metadata.Start?.File != null).GroupBy(i => i.Metadata.Start!.File!).ToDictionary(i => i.Key, i => i.Select(i => i.GetFullname()).ToList());

		// Finally, merge all the collected symbols
		return (Dictionary<SourceFile, List<string>>)configurations.Merge(statics).Merge(functions);
	}

	private static void AppendVirtualFunctionHeader(Unit unit, FunctionImplementation implementation, string fullname)
	{
		unit.Append(new LabelInstruction(unit, new Label(fullname + Mangle.VIRTUAL_FUNCTION_POSTFIX)));

		var from = implementation.VirtualFunction!.FindTypeParent() ?? throw new ApplicationException("Virtual function missing its parent type");
		var to = implementation.FindTypeParent() ?? throw new ApplicationException("Virtual function implementation missing its parent type");

		// NOTE: The type 'from' must be one of the subtypes that type 'to' has
		var alignment = to.GetSupertypeBaseOffset(from);

		if (alignment == null || alignment < 0) throw new ApplicationException("Could not add virtual function header");

		if (alignment != 0)
		{
			var self = References.GetVariable(unit, unit.Self ?? throw new ApplicationException("Missing self pointer"), AccessMode.READ);
			var offset = References.GetConstant(unit, new NumberNode(Format, (long)alignment));

			// Convert the self pointer to the type 'to' by offsetting it
			unit.Append(new SubtractionInstruction(unit, self, offset, Format, true));
		}
	}

	private static string GetTextSection(Function function, List<ConstantDataSectionHandle> constants)
	{
		var builder = new StringBuilder();

		foreach (var implementation in function.Implementations)
		{
			if (implementation.IsInlined) continue;

			var fullname = implementation.GetFullname();

			// Ensure this function is visible to other units
			builder.AppendLine($"{EXPORT_DIRECTIVE} {fullname}");

			var unit = new Unit(implementation);

			unit.Execute(UnitMode.APPEND, () =>
			{
				// Create the most outer scope where all instructions will be placed
				using var scope = new Scope(unit, implementation.Node!);

				if (implementation.VirtualFunction != null)
				{
					builder.AppendLine($"{EXPORT_DIRECTIVE} {fullname + Mangle.VIRTUAL_FUNCTION_POSTFIX}");
					AppendVirtualFunctionHeader(unit, implementation, fullname);
				}

				// Append the function name to the output as a label
				unit.Append(new LabelInstruction(unit, new Label(fullname)));

				// Initialize this function
				unit.Append(new InitializeInstruction(unit));

				// Parameters are active from the start of the function, so they must be required now otherwise they would become active at their first usage
				var parameters = unit.Function.Parameters;

				if ((unit.Function.Metadata.IsMember && !unit.Function.IsStatic) || implementation.IsLambdaImplementation)
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

			unit.Reindex();
			unit.Simulate(UnitMode.BUILD, instruction => { instruction.Build(); });

			builder.Append(Translator.Translate(unit, constants));
			builder.AppendLine();
		}

		return builder.Length == 0 ? string.Empty : builder.ToString();
	}

	private static Dictionary<SourceFile, string> GetTextSections(Context context, out Dictionary<SourceFile, List<ConstantDataSectionHandle>> constant_sections)
	{
		constant_sections = new Dictionary<SourceFile, List<ConstantDataSectionHandle>>();

		var files = Common.GetAllImplementedFunctions(context).Where(i => !i.IsImported && i.Start != null).GroupBy(i => i.Start!.File ?? throw new ApplicationException("Missing declaration file"));
		var text_sections = new Dictionary<SourceFile, string>();

		foreach (var iterator in files)
		{
			var constants = new List<ConstantDataSectionHandle>();
			var builder = new StringBuilder();
			var file = iterator.Key!;

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
			var size = variable.Type!.AllocationSize;

			builder.AppendLine(EXPORT_DIRECTIVE + ' ' + name);

			if (Assembler.IsArm64)
			{
				builder.AppendLine($"{POWER_OF_TWO_ALIGNMENT} 3");
			}

			builder.AppendLine($"{name}: {BYTE_ZERO_ALLOCATOR} {size}");
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
		return root.FindAll(NodeType.STRING).Cast<StringNode>();
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

			if (position >= text.Length) break;

			position++; // Skip character '\'

			var command = text[position++];
			var length = 0;
			var error = string.Empty;

			if (command == 'x')
			{
				length = 2;
				error = "Can not understand hexadecimal value in a string";
			}
			else if (command == 'u')
			{
				length = 4;
				error = "Can not understand Unicode character in a string";
			}
			else if (command == 'U')
			{
				length = 8;
				error = "Can not understand Unicode character in a string";
			}
			else if (command == '\\')
			{
				builder.AppendLine($"{Size.BYTE.Allocator} {(int)'\\'}");
				continue;
			}
			else
			{
				throw new ApplicationException($"Can not understand string command '{command}'");
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
		if (table.IsBuilt) return;

		table.IsBuilt = true;

		if (table.IsSection)
		{
			builder.AppendLine(SECTION_DIRECTIVE + ' ' + table.Name);
		}
		else
		{
			builder.AppendLine(EXPORT_DIRECTIVE + ' ' + table.Name);

			if (Assembler.IsArm64)
			{
				builder.AppendLine($"{POWER_OF_TWO_ALIGNMENT} 3");
			}

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
	private static Dictionary<SourceFile, string> GetDataSections(Context context, Dictionary<SourceFile, List<string>> exports)
	{
		var sections = new Dictionary<SourceFile, StringBuilder>();
		var types = Common.GetAllTypes(context).Where(i => i.Position != null).GroupBy(i => i.Position?.File ?? throw new ApplicationException("Missing type declaration file")).ToArray();

		// Append static variables
		foreach (var iterator in types)
		{
			var builder = new StringBuilder();

			foreach (var type in iterator)
			{
				builder.AppendLine(GetStaticVariables(type));
				builder.Append(SEPARATOR);
			}

			sections.Add(iterator.Key!, builder);
		}

		// Append runtime information about types
		foreach (var iterator in types)
		{
			var builder = sections[iterator.Key!];

			foreach (var type in iterator)
			{
				// 1. Skip if the runtime configuration is not created
				// 2. Imported types are already exported
				// 3. The template type must be a variant
				if (type.Configuration == null || type.IsImported || (type.IsTemplateType && !type.IsTemplateTypeVariant))
				{
					continue;
				}

				AppendTable(builder, type.Configuration.Entry);
			}
		}

		// Append all strings into the data section
		var functions = Common.GetAllFunctionImplementations(context).Where(i => !i.Metadata!.IsImported)
			.GroupBy(i => i.Metadata!.Start?.File ?? throw new ApplicationException("Missing type declaration file"));

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

				builder.AppendLine(Assembler.IsArm64 ? $"{POWER_OF_TWO_ALIGNMENT} 3" : $"{BYTE_ALIGNMENT_DIRECTIVE} 16");
				builder.AppendLine($"{name}:");
				builder.AppendLine(AllocateString(node.Text));
			}

			sections[iterator.Key!] = builder;
		}

		// Append type metadata
		foreach (var iterator in types)
		{
			var builder = sections[iterator.Key!];
			var symbols = new List<string>();

			foreach (var type in iterator)
			{
				var symbol = ExportType(builder, type);

				if (symbol != null)
				{
					symbols.Add(symbol);
				}
			}

			if (symbols.Any())
			{
				exports.Add(iterator.Key!, symbols);
			}
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

				text += $" {Comment} {value}";
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

				text += $" {Comment} {value}";
			}
			else
			{
				text = constant.Value.ToString()!.Replace(',', '.');
				allocator = constant.Size.Allocator;
			}

			builder.AppendLine(Assembler.IsArm64 ? $"{POWER_OF_TWO_ALIGNMENT} 3" : $"{BYTE_ALIGNMENT_DIRECTIVE} 16");
			builder.AppendLine($"{name}:");
			builder.AppendLine($"{allocator} {text}");
		}

		return builder.ToString();
	}

	/// <summary>
	/// Appends debug information about the specified function
	/// </summary>
	public static void AppendFunctionDebugInfo(Debug debug, FunctionImplementation implementation, HashSet<Type> types)
	{
		debug.AppendFunction(implementation, types);

		foreach (var iterator in implementation.Functions.Values.SelectMany(i => i.Overloads).SelectMany(i => i.Implementations))
		{
			if (iterator.Node == null || iterator.Metadata.IsImported) continue;

			AppendFunctionDebugInfo(debug, iterator, types);
		}
	}

	public static Dictionary<SourceFile, string> GetDebugSections(Context context)
	{
		var sections = new Dictionary<SourceFile, string>();

		if (!Assembler.IsDebuggingEnabled)
		{
			return sections;
		}

		var functions = Common.GetAllFunctionImplementations(context).Where(i => i.Metadata.Start != null).GroupBy(i => i.Metadata!.Start!.File ?? throw new ApplicationException("Missing declaration file"));

		foreach (var file in functions)
		{
			var debug = new Debug();
			var types = new HashSet<Type>();

			debug.BeginFile(file.Key!);

			foreach (var implementation in file)
			{
				if (implementation.Metadata!.IsImported) continue;

				AppendFunctionDebugInfo(debug, implementation, types);
			}

			var denylist = new HashSet<Type>();

			while (true)
			{
				var previous = new HashSet<Type>(types);

				foreach (var type in previous)
				{
					if (denylist.Contains(type)) continue;
					debug.AppendType(type, types);
				}

				// Stop if the types have not increased
				if (previous.Count == types.Count) break;

				previous.ForEach(i => denylist.Add(i));
			}

			debug.EndFile();

			sections.Add(file.Key!, debug.Export());
		}

		return sections;
	}

	public static Dictionary<SourceFile, string> Assemble(Context context, SourceFile[] files, Dictionary<SourceFile, List<string>> exports, BinaryType output_type)
	{
		if (Assembler.IsArm64)
		{
			Comment = ARM64_COMMENT;

			Instructions.Arm64.Initialize();
		}
		else
		{
			Instructions.X64.Initialize();
		}

		Assembler.IsPositionIndependent = output_type == BinaryType.SHARED_LIBRARY;

		var result = new Dictionary<SourceFile, string>();

		var entry_function = context.GetFunction(Keywords.INIT.Identifier)?.Overloads.FirstOrDefault()?.Implementations.FirstOrDefault();
		var entry_function_file = (SourceFile?)null;

		if (entry_function != null)
		{
			entry_function_file = entry_function.Metadata.Start?.File ?? throw new ApplicationException("Entry function declaration file missing");
		}

		var text_sections = GetTextSections(context, out Dictionary<SourceFile, List<ConstantDataSectionHandle>> constant_sections);
		var data_sections = GetDataSections(context, exports);
		var debug_sections = GetDebugSections(context);

		foreach (var file in files)
		{
			var builder = new StringBuilder();
			var is_data_section = false;

			builder.AppendLine(TEXT_SECTION_DIRECTIVE);

			if (Assembler.IsX64)
			{
				builder.AppendLine(SYNTAX_REQUIREMENT_DIRECTIVE);
			}

			if (Assembler.IsDebuggingEnabled)
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, Debug.FORMAT_COMPILATION_UNIT_START, file.Index);
				builder.AppendLine(":");

				var fullname = file.Fullname;
				var current_folder = Environment.CurrentDirectory.Replace('\\', '/');
				if (!current_folder.EndsWith('/')) { current_folder += '/'; }

				if (fullname.StartsWith(current_folder))
				{
					fullname = fullname.Remove(0, current_folder.Length);
					fullname = fullname.Insert(0, ".");
				}

				builder.AppendLine(FILE_DIRECTIVE + " 1 " + $"\"{fullname.Replace('\\', '/')}\"");
			}

			// Append the text section header only if the output type represents executable
			if (output_type != BinaryType.STATIC_LIBRARY && entry_function_file == file)
			{
				if (entry_function == null) throw new ApplicationException("Missing entry function");

				var template = IsTargetWindows ? FORMAT_X64_WINDOWS_TEXT_SECTION_HEADER : FORMAT_X64_LINUX_TEXT_SECTION_HEADER;
				var instructions = string.Empty;

				if (Assembler.IsArm64)
				{
					template = IsTargetWindows ? FORMAT_ARM64_WINDOWS_TEXT_SECTION_HEADER : FORMAT_ARM64_LINUX_TEXT_SECTION_HEADER;
				}

				var function = entry_function;

				// Load the stack pointer as the first parameter
				if (Assembler.InitializationFunction != null)
				{
					if (Assembler.IsX64)
					{
						if (IsTargetWindows) { instructions = "mov rcx, rsp\n"; }
						else { instructions = "mov rdi, rsp\n"; }
					}
					else { instructions = "mov x0, sp\n"; }

					function = Assembler.InitializationFunction;
				}

				// Now determine the instruction, which will call the first function
				if (Assembler.IsTargetWindows)
				{
					if (Assembler.IsX64) { instructions += $"jmp {function.GetFullname()}"; }
					else { instructions += $"b {function.GetFullname()}"; }
				}
				else
				{
					if (Assembler.IsX64) { instructions += $"call {function.GetFullname()}"; }
					else { instructions += $"bl {function.GetFullname()}"; }
				}

				builder.AppendLine(string.Format(CultureInfo.InvariantCulture, template, instructions));
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