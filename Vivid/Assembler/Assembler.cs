using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System;

public class AssemblyBuilder
{
	public Dictionary<SourceFile, List<Instruction>> Instructions { get; } = new Dictionary<SourceFile, List<Instruction>>();
	public Dictionary<SourceFile, List<ConstantDataSectionHandle>> Constants { get; } = new Dictionary<SourceFile, List<ConstantDataSectionHandle>>();
	public Dictionary<SourceFile, List<DataEncoderModule>> Modules { get; } = new Dictionary<SourceFile, List<DataEncoderModule>>();
	public HashSet<string> Exports { get; } = new HashSet<string>();
	public StringBuilder? Text { get; }

	public AssemblyBuilder()
	{
		if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled) { Text = new StringBuilder(); }
	}

	public AssemblyBuilder(string text)
	{
		if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled) { Text = new StringBuilder(text); }
	}

	public void Add(SourceFile file, List<Instruction> instructions)
	{
		if (Instructions.ContainsKey(file))
		{
			Instructions[file].AddRange(instructions);
			return;
		}

		Instructions[file] = instructions;
	}

	public void Add(SourceFile file, Instruction instruction)
	{
		if (Instructions.ContainsKey(file))
		{
			Instructions[file].Add(instruction);
			return;
		}

		Instructions[file] = new List<Instruction> { instruction };
	}

	public void Add(SourceFile file, List<ConstantDataSectionHandle> constants)
	{
		if (Constants.ContainsKey(file))
		{
			Constants[file].AddRange(constants);
			return;
		}

		Constants[file] = constants;
	}

	public void Add(SourceFile file, List<DataEncoderModule> modules)
	{
		if (Modules.ContainsKey(file))
		{
			Modules[file].AddRange(modules);
			return;
		}

		Modules[file] = modules;
	}

	public void Add(AssemblyBuilder builder)
	{
		foreach (var iterator in builder.Instructions) Add(iterator.Key, iterator.Value);
		foreach (var iterator in builder.Constants) Add(iterator.Key, iterator.Value);
		foreach (var iterator in builder.Modules) Add(iterator.Key, iterator.Value);
		foreach (var export in builder.Exports) Export(export);

		if (builder.Text != null) Write(builder.Text.ToString());
	}

	public DataEncoderModule GetDataSection(SourceFile file, string section)
	{
		if (section.Length > 0 && section[0] != '.') { section = '.' + section; }

		if (Modules.TryGetValue(file, out var modules))
		{
			for (var i = 0; i < modules.Count; i++)
			{
				if (modules[i].Name == section) return modules[i];
			}
		}
		else
		{
			modules = new List<DataEncoderModule>();
			Modules[file] = modules;
		}
		
		var module = new DataEncoderModule();
		module.Name = section;
		modules.Add(module);
		return module;
	}

	public void Export(IEnumerable<string> symbols)
	{
		foreach (var symbol in symbols) Export(symbol);
	}

	public void Export(string symbol) => Exports.Add(symbol);
	public void Write(string text) => Text?.Append(text);
	public void WriteLine(string text) => Text?.AppendLine(text);

	public override string ToString() => Text?.ToString() ?? string.Empty;
}

public static class Assembler
{
	public const string LEGACY_ASSEMBLY_SYNTAX_SPECIFIER = ".intel_syntax noprefix";
	public const string ARM64_COMMENT = "//";
	public const string X64_COMMENT = "#";
	public const string SEPARATOR = "\n\n";

	public static string DefaultEntryPoint => Settings.IsTargetWindows ? "main" : "_start";

	public static string SectionDirective { get; set; } = ".section";
	public static string SectionRelativeDirective { get; set; } = ".section_relative";
	public static string ExportDirective { get; set; } = ".export";
	public static string TextSectionIdentifier { get; set; } = "text";
	public static string DataSectionIdentifier { get; set; } = "data";
	public static string DebugFileDirective { get; set; } = ".debug_file";
	public static string CharactersAllocator { get; set; } = ".characters";
	public static string ByteAlignmentDirective { get; set; } = "?";
	public static string PowerOfTwoAlignment { get; set; } = ".align";
	public static string ZeroAllocator { get; set; } = ".zero";
	public static string CommentSpecifier { get; set; } = X64_COMMENT;
	public static string DebugFunctionStartDirective { get; set; } = '.' + AssemblyParser.DEBUG_START_DIRECTIVE;
	public static string DebugFrameOffsetDirective { get; set; } = '.' + AssemblyParser.DEBUG_FRAME_OFFSET_DIRECTIVE;
	public static string DebugFunctionEndDirective { get; set; } = '.' + AssemblyParser.DEBUG_END_DIRECTIVE;
	public static string MemoryAddressExtension { get; set; } = " ";
	public static string RelativeSymbolSpecifier { get; set; } = string.Empty;

	private const string FORMAT_X64_LINUX_TEXT_SECTION_HEADER =
		"{0} _start" + "\n" +
		"_start:" + "\n" +
		"{1}" + "\n" +
		"mov rdi, rax" + "\n" +
		"mov rax, 60" + "\n" +
		"syscall" + SEPARATOR;

	private const string FORMAT_ARM64_LINUX_TEXT_SECTION_HEADER =
		"{0} _start" + "\n" +
		"_start:" + "\n" +
		"{1}" + "\n" +
		"mov x8, #93" + "\n" +
		"svc #0" + SEPARATOR;

	private const string FORMAT_X64_LINUX_TEXT_SECTION_HEADER_WITHOUT_SYSTEM_CALL =
		"{0} _start" + "\n" +
		"_start:" + "\n" +
		"{1}" + SEPARATOR;

	private const string FORMAT_WINDOWS_TEXT_SECTION_HEADER =
		"{0} main" + "\n" +
		"main:" + "\n" +
		"{1}" + SEPARATOR;

	private static void AddVirtualFunctionHeader(Unit unit, FunctionImplementation implementation, string fullname)
	{
		unit.Add(new LabelInstruction(unit, new Label(fullname + Mangle.VIRTUAL_FUNCTION_POSTFIX)));

		// Do not try to convert the self pointer, if it is not used
		if (unit.Self!.Usages.Count == 0) return;

		var from = implementation.VirtualFunction!.FindTypeParent() ?? throw new ApplicationException("Virtual function missing its parent type");
		var to = implementation.FindTypeParent() ?? throw new ApplicationException("Virtual function implementation missing its parent type");

		// NOTE: The type 'from' must be one of the subtypes that type 'to' has
		var alignment = to.GetSupertypeBaseOffset(from);

		if (alignment == null || alignment < 0) throw new ApplicationException("Could not add virtual function header");

		if (alignment != 0)
		{
			var self = References.GetVariable(unit, unit.Self ?? throw new ApplicationException("Missing self pointer"), AccessMode.WRITE);
			var offset = References.GetConstant(unit, new NumberNode(Settings.Signed, (long)alignment));

			// Convert the self pointer to the type 'to' by offsetting it by the alignment
			unit.Add(new SubtractionInstruction(unit, self, offset, Settings.Signed, true));
		}
	}

	/// <summary>
	/// Connects the specified scope to the destination scope.
	/// </summary>
	public static void ConnectBackwardsJump(Unit unit, Scope from, Scope to)
	{
		// Require the input variables of the destination scope in the arrival scope
		foreach (var iterator in to.Inputs)
		{
			unit.RequireVariable(iterator.Key, from);
		}
	}

	/// <summary>
	/// Finds all scopes that arrive to scopes before them and connects them.
	/// </summary>
	public static void ConnectBackwardsJumps(Unit unit)
	{
		foreach (var i in unit.Arrivals)
		{
			var destination = unit.Scopes[i.Key];
			var arrivals = i.Value;

			foreach (var arrival in arrivals)
			{
				if (arrival.Index < destination.Index) continue;
				ConnectBackwardsJump(unit, arrival, destination);
			}
		}
	}

	/// <summary>
	/// Assembles the specified function implementation
	/// </summary>
	private static AssemblyBuilder GetTextSection(FunctionImplementation implementation)
	{
		var builder = new AssemblyBuilder();

		var fullname = implementation.GetFullname();

		// Ensure this function is visible to other units
		builder.WriteLine($"{ExportDirective} {fullname}");
		builder.Export(fullname);

		var unit = new Unit(implementation);
		unit.Mode = UnitMode.ADD;

		var scope = new Scope(unit, Scope.ENTRY);

		// Update the variable usages before we start
		Analyzer.LoadVariableUsages(implementation);

		// Add virtual function header, if the implementation overrides a virtual function
		if (implementation.VirtualFunction != null)
		{
			builder.WriteLine($"{ExportDirective} {fullname + Mangle.VIRTUAL_FUNCTION_POSTFIX}");
			builder.Export(fullname + Mangle.VIRTUAL_FUNCTION_POSTFIX);

			AddVirtualFunctionHeader(unit, implementation, fullname);
		}

		// Add the function name to the output as a label
		unit.Add(new LabelInstruction(unit, new Label(fullname)));

		// Initialize this function
		unit.Add(new InitializeInstruction(unit));

		// Parameters are active from the start of the function, so they must be required now otherwise they would become active at their first usage
		var parameters = new List<Variable>(unit.Function.Parameters);

		if ((unit.Function.Metadata.IsMember && !unit.Function.IsStatic) || implementation.IsLambdaImplementation)
		{
			parameters.Add(unit.Self ?? throw new ApplicationException("Missing self pointer in a member function"));
		}

		// Include pack proxies as well
		var parameter_count = parameters.Count;

		for (var i = 0; i < parameter_count; i++)
		{
			var parameter = parameters[i];
			if (!parameter.Type!.IsPack) continue;
			parameters.AddRange(Common.GetPackProxies(parameter));
		}

		if (Settings.IsDebuggingEnabled)
		{
			Calls.MoveParametersToStack(unit);
		}

		Builders.Build(unit, implementation.Node!);

		// Connect scopes that jump backwards
		ConnectBackwardsJumps(unit);

		foreach (var instruction in unit.Instructions)
		{
			instruction.Reindex();
		}

		// Build:
		unit.Scope = null;
		unit.StackOffset = 0;
		unit.Mode = UnitMode.BUILD;

		foreach (var register in unit.Registers) { register.Reset(); }

		for (unit.Position = 0; unit.Position < unit.Instructions.Count; unit.Position++)
		{
			var instruction = unit.Instructions[unit.Position];

			// All instructions must have a scope
			if (instruction.Scope == null) throw new ApplicationException("Missing instruction scope");

			unit.Anchor = instruction;

			// Switch between scopes
			if (!ReferenceEquals(unit.Scope, instruction.Scope))
			{
				instruction.Scope.Enter();
			}

			instruction.Build();
		}

		// Reset the state
		unit.Mode = UnitMode.NONE;

		Translator.Translate(builder, unit);

		return builder;
	}

	/// <summary>
	/// Assembles all functions inside the specified context and returns the generated assembly grouped by the corresponding source files
	/// </summary>
	private static Dictionary<SourceFile, AssemblyBuilder> GetTextSections(Context context, List<SourceFile> files)
	{
		var all = Common.GetAllFunctionImplementations(context, false);

		// Group all functions by their owner files
		var implementations = all
			.Where(i => i.Metadata.Start != null)
			.GroupBy(i => i.Metadata.Start!.File ?? throw new ApplicationException("Missing declaration file"))
			.ToDictionary(i => i.Key, i => i.ToList());

		// Store the number of assembled functions
		var index = 0;

		var builders = new Dictionary<SourceFile, AssemblyBuilder>();

		foreach (var file in files)
		{
			var builder = new AssemblyBuilder();

			// Add the debug label, which indicates the start of debuggable code
			if (Settings.IsDebuggingEnabled)
			{
				var label = string.Format(Debug.FORMAT_COMPILATION_UNIT_START, file.Index);
				if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled) builder.WriteLine(label + ':');
				builder.Add(file, new LabelInstruction(null!, new Label(label)));
			}

			if (implementations.ContainsKey(file))
			{
				foreach (var implementation in implementations[file])
				{
					if (implementation.IsImported) continue;

					if (Settings.IsVerboseOutputEnabled)
					{
						Console.WriteLine($"[{index + 1}/{all.Length}]: Assembling {implementation.ToString()}");
					}

					builder.Add(GetTextSection(implementation));
					builder.Write(SEPARATOR);

					index++; // Increment the number of assembled functions
				}
			}

			// Add the debug label, which indicates the end of debuggable code
			if (Settings.IsDebuggingEnabled)
			{
				var label = string.Format(Debug.FORMAT_COMPILATION_UNIT_END, file.Index);
				if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled) builder.WriteLine(label + ':');
				builder.Add(file, new LabelInstruction(null!, new Label(label)));
			}

			builders.Add(file, builder);
		}

		return builders;
	}

	/// <summary>
	/// Allocates the specified static variable using assembly directives
	/// </summary>
	private static string AllocateStaticVariable(Variable variable)
	{
		var builder = new StringBuilder();

		var name = variable.GetStaticName();
		var size = variable.Type!.AllocationSize;

		builder.AppendLine(ExportDirective + ' ' + name);

		if (Settings.IsArm64)
		{
			builder.AppendLine($"{PowerOfTwoAlignment} 4");
		}

		builder.AppendLine($"{name}:");
		builder.AppendLine($"{ZeroAllocator} {size}");

		return builder.ToString();
	}

	/// <summary>
	/// Allocates a string using assembly directives while accounting for embedded hexadecimal numbers
	/// </summary>
	private static string AllocateString(string text)
	{
		var builder = new StringBuilder();
		var position = 0;

		while (position < text.Length)
		{
			var slice = new string(text.Skip(position).TakeWhile(i => i != '\\').ToArray());
			position += slice.Length;

			if (slice.Length > 0)
			{
				if (Settings.IsLegacyAssemblyEnabled) builder.AppendLine($"{CharactersAllocator} \"{slice}\"");
				else builder.AppendLine($"{CharactersAllocator} \'{slice}\'");
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
			if (!ulong.TryParse(hexadecimal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong value)) throw new ApplicationException(error);

			var bytes = BitConverter.GetBytes(value).Take(length / 2).ToArray();
			bytes.ForEach(i => builder.AppendLine($"{Size.BYTE.Allocator} {i}"));

			position += length;
		}

		return builder.Append($"{Size.BYTE.Allocator} 0").ToString();
	}

	/// <summary>
	/// Allocates the specified constants using the specified data section builder
	/// </summary>
	private static void AllocateConstants(AssemblyBuilder builder, SourceFile file, List<ConstantDataSectionHandle> constants)
	{
		var module = builder.GetDataSection(file, Assembler.DataSectionIdentifier);

		foreach (var constant in constants)
		{
			// Align the position and declare the constant
			var name = constant.Identifier;

			if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled)
			{
				builder.WriteLine(!Settings.IsLegacyAssemblyEnabled || Settings.IsArm64 ? $"{PowerOfTwoAlignment} 4" : $"{ByteAlignmentDirective} 16");
				builder.WriteLine($"{name}:");
			}

			DataEncoder.Align(module, 16);
			module.CreateLocalSymbol(name, module.Position);

			var bytes = (byte[]?)null;

			if (constant.Value is byte[] c0) { bytes = c0; }
			else if (constant.Value is double c1) { bytes = BitConverter.GetBytes(c1); }
			else if (constant.Value is float c2) { bytes = BitConverter.GetBytes(c2); }
			else { throw new NotImplementedException("Unsupported constant data"); }

			module.Write(bytes);

			foreach (var element in bytes) builder.WriteLine($"{Size.BYTE.Allocator} {element}");
		}
	}

	/// <summary>
	/// Allocates the specified table label using assembly directives
	/// </summary>
	private static string AddTableLabel(TableLabel label)
	{
		if (label.Declare)
		{
			return $"{label.Name}:";
		}

		if (label.IsSectionRelative)
		{
			return SectionRelativeDirective + label.Size.Bits.ToString() + ' ' + label.Name;
		}

		return $"{label.Size.Allocator} {label.Name}";
	}

	/// <summary>
	/// Allocates the specified table using assembly directives
	/// </summary>
	public static void AddTable(AssemblyBuilder builder, Table table, TableMarker marker)
	{
		if ((table.Marker & marker) != 0) return;
		table.Marker |= marker;

		if (table.IsSection)
		{
			builder.WriteLine(SectionDirective + ' ' + table.Name); // Create a section
		}
		else
		{
			builder.WriteLine(ExportDirective + ' ' + table.Name); // Export the table

			// Align the table
			if (Settings.IsArm64) builder.WriteLine($"{PowerOfTwoAlignment} 4");

			builder.WriteLine(table.Name + ':');
		}

		// Take care of the table items
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
				Table f => $"{Size.QWORD.Allocator} {f.Name}",
				Label g => $"{Size.QWORD.Allocator} {g.GetName()}",
				Offset h => $"{Size.DWORD.Allocator} {h.To.Name} - {h.From.Name}",
				TableLabel i => AddTableLabel(i),
				_ => throw new ApplicationException("Invalid table item")
			};

			builder.WriteLine(result);

			if (item is Table subtable) subtables.Add(subtable);
		}

		builder.Write(SEPARATOR);

		subtables.ForEach(i => AddTable(builder, i, marker));
	}

	/// <summary>
	/// Constructs file specific data sections based on the specified context
	/// </summary>
	private static Dictionary<SourceFile, AssemblyBuilder> GetDataSections(Context context)
	{
		var builders = new Dictionary<SourceFile, AssemblyBuilder>();
		var types = Common.GetAllTypes(context).Where(i => i.Position != null).GroupBy(i => i.Position?.File ?? throw new ApplicationException("Missing type declaration file")).ToArray();

		var data_section_directive = $"{Assembler.SectionDirective} {Assembler.DataSectionIdentifier}\n";

		// Add static variables
		foreach (var iterator in types)
		{
			var builder = new AssemblyBuilder(data_section_directive);

			foreach (var type in iterator)
			{
				// 1. Skip imported types, because they are already exported
				if (type.IsImported) continue;

				foreach (var variable in type.Variables.Values)
				{
					if (!variable.IsStatic) continue;

					if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled) builder.WriteLine(AllocateStaticVariable(variable));
					DataEncoder.AddStaticVariable(builder.GetDataSection(iterator.Key, Assembler.DataSectionIdentifier), variable);
				}

				builder.Write(SEPARATOR);
			}

			builders[iterator.Key] = builder;
		}

		// Add runtime information about types
		foreach (var iterator in types)
		{
			var builder = builders[iterator.Key];

			foreach (var type in iterator)
			{
				// 1. Skip if the runtime configuration is not created
				// 2. Skip imported types, because they are already exported
				// 3. The template type must be a variant
				// 4. Unnamed packs are not processed
				if (type.Configuration == null || type.IsImported || (type.IsTemplateType && !type.IsTemplateTypeVariant) || type.IsUnnamedPack) continue;

				if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled) AddTable(builder, type.Configuration.Entry, TableMarker.TextualAssembly);
				DataEncoder.AddTable(builder, builder.GetDataSection(iterator.Key, Assembler.DataSectionIdentifier), type.Configuration.Entry, TableMarker.DataEncoder);
			}
		}

		// Add all the strings into the data section
		var functions = Common.GetAllFunctionImplementations(context).Where(i => !i.Metadata!.IsImported)
			.GroupBy(i => i.Metadata!.Start?.File ?? throw new ApplicationException("Missing type declaration file"));

		foreach (var iterator in functions)
		{
			// Find all the strings inside the functions
			var nodes = iterator.Where(i => i.Node != null).SelectMany(i => i.Node!.FindAll(NodeType.STRING)).Cast<StringNode>().ToList();

			var builder = builders.GetValueOrDefault(iterator.Key, new AssemblyBuilder(data_section_directive))!;

			foreach (var node in nodes)
			{
				var name = node.Identifier;
				if (name == null) continue;

				if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled)
				{
					builder.WriteLine(!Settings.IsLegacyAssemblyEnabled || Settings.IsArm64 ? $"{PowerOfTwoAlignment} 4" : $"{ByteAlignmentDirective} 16");
					builder.WriteLine($"{name}:");
					builder.WriteLine(AllocateString(node.Text));
				}

				var module = builder.GetDataSection(iterator.Key, Assembler.DataSectionIdentifier);

				DataEncoder.Align(module, Settings.IsArm64 ? 8 : 16);

				module.CreateLocalSymbol(name, module.Position);
				module.String(node.Text);
			}

			builders[iterator.Key!] = builder;
		}

		return builders;
	}

	/// <summary>
	/// Appends debug information about the specified function
	/// </summary>
	public static void AddFunctionDebugInfo(Debug debug, FunctionImplementation implementation, HashSet<Type> types)
	{
		debug.AddFunction(implementation, types);
	}

	/// <summary>
	/// Constructs debugging information for each of the files inside the context
	/// </summary>
	public static Dictionary<SourceFile, AssemblyBuilder> GetDebugSections(Context context)
	{
		var builders = new Dictionary<SourceFile, AssemblyBuilder>();
		if (!Settings.IsDebuggingEnabled) return builders;

		var functions = Common.GetAllFunctionImplementations(context).Where(i => i.Metadata.Start != null)
			.GroupBy(i => i.Metadata!.Start!.File ?? throw new ApplicationException("Missing declaration file"));

		foreach (var file in functions)
		{
			var debug = new Debug();
			var types = new HashSet<Type>();

			debug.BeginFile(file.Key!);

			foreach (var implementation in file)
			{
				if (implementation.Metadata!.IsImported) continue;
				AddFunctionDebugInfo(debug, implementation, types);
			}

			var denylist = new HashSet<Type>();

			while (true)
			{
				var previous = new HashSet<Type>(types);

				foreach (var type in previous)
				{
					if (denylist.Contains(type)) continue;
					debug.AddType(type, types);
				}

				// Stop if the types have not increased
				if (previous.Count == types.Count) break;

				previous.ForEach(i => denylist.Add(i));
			}

			debug.EndFile();

			builders.Add(file.Key, debug.Export(file.Key));
		}

		return builders;
	}

	/// <summary>
	/// Builds an object file from the specified properties and writes it into a file
	/// </summary>
	public static void OutputObjectFile(string output, List<BinarySection> sections, HashSet<string> exports)
	{
		var binary = (byte[]?)null;

		if (Settings.IsTargetWindows)
		{
			binary = PeFormat.Build(sections, exports);
		}
		else
		{
			binary = ElfFormat.Build(sections, exports);
		}

		File.WriteAllBytes(output, binary);
	}

	public static Dictionary<SourceFile, string> Assemble(Context context, List<SourceFile> files, List<string> imports, Dictionary<SourceFile, List<string>> exports, string output_name, BinaryType output_type)
	{
		if (Settings.IsArm64)
		{
			CommentSpecifier = ARM64_COMMENT;

			Instructions.Arm64.Initialize();
		}
		else
		{
			Instructions.X64.Initialize();
		}

		Keywords.All.Clear(); // Remove all keywords for parsing assembly

		Settings.UseIndirectAccessTables = !Settings.IsTargetWindows && output_type == BinaryType.SHARED_LIBRARY && Settings.IsLegacyAssemblyEnabled;

		var assemblies = new Dictionary<SourceFile, string>();

		var entry_function = context.GetFunction(Keywords.INIT.Identifier)?.Overloads.FirstOrDefault()?.Implementations.FirstOrDefault();
		var entry_function_file = (SourceFile?)null;

		if (entry_function != null)
		{
			entry_function_file = entry_function.Metadata.Start?.File ?? throw new ApplicationException("Entry function declaration file missing");
		}

		var object_files = Settings.ObjectFiles;
		var standard_library_object_file = new SourceFile(AssemblyPhase.StandardLibrary, string.Empty, files.Max(i => i.Index) + 1);

		// Import user defined object files
		var user_imported_object_files = Settings.UserImportedObjectFiles;

		foreach (var object_file in user_imported_object_files)
		{
			var file = new SourceFile(object_file, string.Empty, -1);
			object_files.Add(file, Settings.IsTargetWindows ? PeFormat.Import(object_file) : ElfFormat.Import(object_file));
		}

		var text_sections = GetTextSections(context, files);
		var data_sections = GetDataSections(context);
		var debug_sections = GetDebugSections(context);

		// Static libraries and object files do not have entry points
		var is_entry_point_needed = output_type == BinaryType.EXECUTABLE || output_type == BinaryType.SHARED_LIBRARY;

		foreach (var file in files)
		{
			var builder = new AssemblyBuilder();

			// Legacy assembly requires the syntax to be specified
			if (Settings.IsX64 && Settings.IsLegacyAssemblyEnabled) builder.WriteLine(LEGACY_ASSEMBLY_SYNTAX_SPECIFIER);

			// Start the text section
			builder.WriteLine(SectionDirective + ' ' + TextSectionIdentifier);

			if (Settings.IsDebuggingEnabled)
			{
				var fullname = file.Fullname;

				var current_folder = Environment.CurrentDirectory.Replace('\\', '/');
				if (!current_folder.EndsWith('/')) { current_folder += '/'; }

				if (fullname.StartsWith(current_folder))
				{
					fullname = fullname.Remove(0, current_folder.Length);
					fullname = fullname.Insert(0, "./");
				}

				if (Settings.IsLegacyAssemblyEnabled)
				{
					builder.WriteLine(DebugFileDirective + " 1 " + $"\"{fullname.Replace('\\', '/')}\"");
				}
				else
				{
					builder.WriteLine(DebugFileDirective + ' ' + $"\'{fullname.Replace('\\', '/')}\'");
				}
			}

			// Add the entry header if the output type must have a entry point and we are processing the right file
			if (is_entry_point_needed && entry_function_file == file)
			{
				if (entry_function == null) throw new ApplicationException("Missing entry function");

				var template = FORMAT_WINDOWS_TEXT_SECTION_HEADER;
				var instructions = string.Empty;
				
				if (Settings.IsTargetLinux)
				{
					if (output_type == BinaryType.SHARED_LIBRARY)
					{
						// When creating a shared library, its initialization function must not shutdown the entire process
						template = FORMAT_X64_LINUX_TEXT_SECTION_HEADER_WITHOUT_SYSTEM_CALL;
					}
					else
					{
						template = Settings.IsArm64 ? FORMAT_ARM64_LINUX_TEXT_SECTION_HEADER : FORMAT_X64_LINUX_TEXT_SECTION_HEADER;
					}
				}

				var function = entry_function;

				// Load the stack pointer as the first parameter
				if (Settings.InitializationFunction != null)
				{
					if (Settings.IsX64)
					{
						if (Settings.IsTargetWindows) { instructions = "mov rcx, rsp\n"; }
						else { instructions = "mov rdi, rsp\n"; }
					}
					else { instructions = "mov x0, sp\n"; }

					function = Settings.InitializationFunction;
				}

				// Now determine the instruction, which will call the first function
				if (Settings.IsTargetWindows || output_type == BinaryType.SHARED_LIBRARY)
				{
					if (Settings.IsX64) { instructions += $"jmp {function.GetFullname()}"; }
					else { instructions += $"b {function.GetFullname()}"; }
				}
				else
				{
					if (Settings.IsX64) { instructions += $"call {function.GetFullname()}"; }
					else { instructions += $"bl {function.GetFullname()}"; }
				}

				builder.WriteLine(string.Format(template, ExportDirective, instructions));

				if (!Settings.IsLegacyAssemblyEnabled)
				{
					var parser = new AssemblyParser();
					parser.Parse(file, string.Format(template, ExportDirective, instructions));

					builder.Add(file, parser.Instructions);
					builder.Export(parser.Exports);
				}
			}

			if (text_sections.TryGetValue(file, out var text_section_builder))
			{
				builder.Add(text_section_builder);
				builder.Write(SEPARATOR);
			}

			if (data_sections.TryGetValue(file, out var data_section_builder))
			{
				if (builder.Constants.ContainsKey(file)) AllocateConstants(data_section_builder, file, builder.Constants[file]);

				builder.Add(data_section_builder);
				builder.Write(SEPARATOR);
			}

			if (debug_sections.TryGetValue(file, out var debug_section_builder))
			{
				builder.Add(debug_section_builder);
				builder.Write(SEPARATOR);
			}

			exports[file] = builder.Exports.ToList();

			if (Settings.IsAssemblyOutputEnabled || Settings.IsLegacyAssemblyEnabled)
			{
				assemblies.Add(file, Regex.Replace(builder.ToString().Replace("\r\n", "\n"), "\n{3,}", "\n\n"));
			}

			// Skip using the new system if requested
			if (Settings.IsLegacyAssemblyEnabled) continue;

			// Load all the section modules
			var modules = builder.Modules.GetValueOrDefault(file, new List<DataEncoderModule>());

			// Ensure there are no duplicated sections
			if (modules.Count != modules.Select(i => i.Name).Distinct().Count()) throw new ApplicationException("Duplicated sections are not allowed");

			var output = InstructionEncoder.Encode(builder.Instructions.GetValueOrDefault(file, new List<Instruction>()), Settings.IsDebuggingEnabled ? file.Fullname : null);
			var object_text_section = output.Section;
			var object_data_sections = modules.Select(i => i.Export()).ToList();
			var object_debug_lines = output.Lines?.Export();
			var object_debug_frames = output.Frames?.Export();

			var sections = new List<BinarySection>();
			sections.Add(object_text_section);
			if (object_debug_frames != null) sections.Add(object_debug_frames);
			sections.AddRange(object_data_sections);
			if (object_debug_lines != null) sections.Add(object_debug_lines);

			if (output_type == BinaryType.OBJECTS)
			{
				OutputObjectFile(file.GetFilenameWithoutExtension() + AssemblyPhase.ObjectFileExtension, sections, builder.Exports);
				continue;
			}

			var object_file = Settings.IsTargetWindows
				? PeFormat.Create(file.Fullname, sections, builder.Exports)
				: ElfFormat.Create(file.Fullname, sections, builder.Exports);

			object_files.Add(file, object_file);
		}

		if (output_type == BinaryType.RAW)
		{
			File.WriteAllBytes(output_name, ElfFormat.BuildBinaryFile(object_files.Values.ToList()));
			return assemblies;
		}

		if (output_type == BinaryType.OBJECTS)
		{
			return assemblies;
		}

		if (output_type == BinaryType.STATIC_LIBRARY)
		{
			if (!StaticLibraryFormat.Export(context, object_files, output_name).IsProblematic) return assemblies;

			throw new ApplicationException("Failed to create the static library");
		}

		if (!Settings.IsLegacyAssemblyEnabled)
		{
			var postfix = output_type == BinaryType.EXECUTABLE ? AssemblyPhase.ExecutableExtension : AssemblyPhase.SharedLibraryExtension;

			var linked_binary = Settings.IsTargetWindows
				? PeFormat.Link(object_files.Values.ToList(), imports, DefaultEntryPoint, output_name + postfix, output_type == BinaryType.EXECUTABLE)
				: Linker.Link(object_files.Values.ToList(), DefaultEntryPoint, output_type == BinaryType.EXECUTABLE);

			File.WriteAllBytes(output_name + postfix, linked_binary);

			// Make the produced binary executable
			if (Settings.IsTargetLinux)
			{
				var process = Process.Start(new ProcessStartInfo("bash", $"-c \"chmod +x '{output_name + postfix}'\"")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				});

				process?.WaitForExit();
			}
		}

		return assemblies;
	}
}