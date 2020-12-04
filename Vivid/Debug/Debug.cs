using System.Linq;
using System.Text;
using System;

public class Offset
{
	public TableLabel From { get; set; }
	public TableLabel To { get; set; }

	public Offset(TableLabel from, TableLabel to)
	{
		From = from;
		To = to;
	}
}

public class TableLabel
{
	public string Name { get; set; }
	public Size Size { get; set; }
	public bool IsSecrel { get; set; } = false;
	public bool Declare { get; set; }

	public TableLabel(string name, Size size, bool declare)
	{
		Name = name;
		Size = size;
		Declare = declare;
	}
}

public class Debug
{
	public const string COMPILATION_UNIT_START = "main";
	public const string COMPILATION_UNIT_END = "end";

	public const string DEBUG_ABBREVATION_TABLE = ".debug_abbrev";
	public const string DEBUG_INFO_TABLE = ".debug_info";
	public const string DEBUG_STRING_TABLE = ".debug_str";
	public const string DEBUG_LINE_TABLE = ".debug_line";
	public const string DEBUG_LINE_TABLE_START = ".debug_line_start";

	public const string DWARF_PRODUCER_TEXT = "Vivid version 1.0";
	public const short DWARF_LANGUANGE_IDENTIFIER = 0x7777;

	public const short DWARF_VERSION = 4;

	public const byte DWARF_ENCODING_ADDRESS = 1;
	public const byte DWARF_ENCODING_BOOL = 2;

	public const byte DWARF_ENCODING_DECIMAL = 4;

	public const byte DWARF_ENCODING_SIGNED = 5;
	public const byte DWARF_ENCODING_UNSIGNED = 7;

	public const byte DWARF_ENCODING_SIGNED_CHAR = 6;
	public const byte DWARF_ENCODING_UNSIGNED_CHAR = 8;

	public const byte DWARF_CALLING_CONVENTION_PASS_BY_REFERENCE = 4;

	public const byte DWARF_ACCESS_PUBLIC = 1;
	public const byte DWARF_ACCESS_PROTECTED = 2;
	public const byte DWARF_ACCESS_PRIVATE = 3;

	public const byte DWARF_OP_BASE_POINTER_OFFSET = 145;
	public const byte DWARF_OFFSET_ZERO = 128;

	public const byte DWARF_REGISTER_ZERO = 80;
	public const byte DWARF_BASE_POINTER_REGISTER = 86;

	public const byte DWARF_TAG_COMPILE_UNIT = 17;
	public const byte DWARF_HAS_CHILDREN = 1;
	public const byte DWARF_HAS_NO_CHILDREN = 0;
	public const byte DWARF_PRODUCER = 37;
	public const byte DWARF_LANGUANGE = 19;
	public const byte DWARF_NAME = 3;
	public const byte DWARF_LINE_NUMBER_INFORMATION = 16;
	public const byte DWARF_COMPILATION_FOLDER = 27;
	public const byte DWARF_LOW_PC = 17;
	public const byte DWARF_HIGH_PC = 18;
	public const byte DWARF_FRAME_BASE = 64;
	public const byte DWARF_DECLARATION_FILE = 58;
	public const byte DWARF_DECLARATION_LINE = 59;
	public const byte DWARF_CALLING_CONVENTION = 54;

	public const byte DWARF_FUNCTION = 46;
	public const byte DWARF_BASE_TYPE_DECLARATION = 36;
	public const byte DWARF_OBJECT_TYPE_DECLARATION = 2;
	public const byte DWARF_POINTER_TYPE_DECLARATION = 15;
	public const byte DWARF_MEMBER_DECLARATION = 13;
	public const byte DWARF_MEMBER_LOCATION = 56;
	public const byte DWARF_ACCESSIBILITY = 50;

	public const byte DWARF_TYPE = 73;
	public const byte DWARF_EXPORTED = 63;
	public const byte DWARF_VARIABLE = 52;
	public const byte DWARF_LOCATION = 2;
	public const byte DWARF_ENCODING = 62;
	public const byte DWARF_BYTE_SIZE = 11;

	public const byte DWARF_STRING_POINTER = 14;
	public const byte DWARF_STRING = 8;
	public const byte DWARF_DATA_8 = 11;
	public const byte DWARF_DATA_16 = 5;
	public const byte DWARF_DATA_32 = 6;
	public const byte DWARF_ADDRESS = 1;
	public const byte DWARF_REFERENCE_32 = 19;
	public const byte DWARF_DATA_SECTION_OFFSET = 23;
	public const byte DWARF_EXPRESSION = 24;
	public const byte DWARF_PRESENT = 25;

	public const byte DWARF_END = 0;

	public Table Entry { get; }
	public Table Abbrevation { get; }
	public Table Strings { get; }
	public Table Lines { get; }

	public TableLabel Start { get; }
	public TableLabel End { get; }

	public byte Index { get; private set; } = 1;

	public byte ObjectTypeAbbrevation { get; private set; } = 0;
	public byte BaseTypeAbbrevation { get; private set; } = 0;
	public byte PointerTypeAbbrevation { get; private set; } = 0;
	public byte MemberVariableAbbrevation { get; private set; } = 0;
	public byte VariableAbbrevation { get; private set; } = 0;

	public static object GetOffset(TableLabel from, TableLabel to)
	{
		return new Offset(from, to );
	}

	public void AppendFile(string file, string folder, TableLabel start, TableLabel end)
	{
		Entry.Add(Index); // DW_TAG_compile_unit
		Entry.Add(DWARF_PRODUCER_TEXT); // DW_AT_producer
		Entry.Add(DWARF_LANGUANGE_IDENTIFIER); // DW_AT_language

		Entry.Add(file); // DW_AT_name

		Entry.Add(new TableLabel(DEBUG_LINE_TABLE_START, Size.DWORD, false) { IsSecrel = true }); // DW_AT_stmt_list

		Entry.Add(folder); // DW_AT_comp_dir
		Entry.Add(start); // DW_AT_low_pc

		Entry.Add(GetOffset(start, end)); /// DW_AT_high_pc

		Abbrevation.Add(Index++); // Define the current abbrevation code

		Abbrevation.Add(DWARF_TAG_COMPILE_UNIT); // This is a compile unit and it has children
		Abbrevation.Add(DWARF_HAS_CHILDREN);

		Abbrevation.Add(DWARF_PRODUCER); // The producer is identified with a string pointer
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_LANGUANGE); // The languange is identified with a short integer
		Abbrevation.Add(DWARF_DATA_16);

		Abbrevation.Add(DWARF_NAME); // The name of the file is added with a string pointer
		Abbrevation.Add(DWARF_STRING);
		
		Abbrevation.Add(DWARF_LINE_NUMBER_INFORMATION); // The line number information is added with a section offset
		Abbrevation.Add(DWARF_DATA_SECTION_OFFSET);

		Abbrevation.Add(DWARF_COMPILATION_FOLDER); // The compilation folder is added with a string pointer
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_LOW_PC);
		Abbrevation.Add(DWARF_ADDRESS);

		Abbrevation.Add(DWARF_HIGH_PC);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		AppendObjectTypeAbbrevation();
		AppendBaseTypeAbbrevation();
		AppendPointerTypeAbbrevation();
		AppendMemberVariableAbbrevation();
		AppendVariableAbbrevation();
	}

	public static TableLabel GetStart(FunctionImplementation implementation)
	{
		return new TableLabel(implementation.GetFullname(), Size.QWORD, false);
	}

	public static TableLabel GetEnd(FunctionImplementation implementation)
	{
		return new TableLabel(implementation.GetFullname() + "_end", Size.QWORD, false);
	}

	public static int GetFile(FunctionImplementation implementation)
	{
		return 1;
	}

	public static int GetLine(FunctionImplementation implementation)
	{
		return implementation.Metadata!.Position?.FriendlyLine ?? throw new ApplicationException($"Declaration position of function '{implementation.GetHeader()}' missing");
	}

	public static int GetFile(Type type)
	{
		return 1;
	}

	public static int GetLine(Type type)
	{
		return type.Position?.FriendlyLine ?? throw new ApplicationException($"Declaration position of type '{type.Name}' missing");
	}

	public static int GetFile(Variable variable)
	{
		return 1;
	}

	public static int GetLine(Variable variable)
	{
		return variable.Position?.FriendlyLine ?? throw new ApplicationException($"Declaration position of variable '{variable.Name}' missing");
	}

	public static string GetTypeLabelName(Type type, bool pointer = false)
	{
		return type.GetFullname() + (pointer ? "_pointer_debug" : "_debug");
	}

	public static TableLabel GetTypeLabel(Type type, bool pointer = false)
	{
		return new TableLabel(GetTypeLabelName(type, pointer), Size.QWORD, false);
	}

	public void AppendOperation(byte command, params byte[] parameters)
	{
		Entry.Add((byte)(parameters.Length + 1)); // Length of the operation
		Entry.Add(command);
		parameters.ForEach(i => Entry.Add(i));
	}

	public void AppendFunction(FunctionImplementation implementation)
	{
		var file = GetFile(implementation);

		Entry.Add(Index); // DW_TAG_subprogram

		var start = new TableLabel(implementation.GetFullname(), Size.QWORD, false);
		Entry.Add(start); // DW_AT_low_pc

		Entry.Add(GetOffset(start, GetEnd(implementation))); // DW_AT_high_pc

		AppendOperation(DWARF_BASE_POINTER_REGISTER); // DW_AT_frame_base
		Entry.Add(implementation.GetFullname()); // DW_AT_name
		Entry.Add(file); // DW_AT_decl_file
		Entry.Add(GetLine(implementation)); // DW_AT_decl_line

		var has_children = implementation.Locals.Any();

		Abbrevation.Add(Index++);
		Abbrevation.Add(DWARF_FUNCTION);
		Abbrevation.Add(has_children ? DWARF_HAS_CHILDREN : DWARF_HAS_NO_CHILDREN);

		Abbrevation.Add(DWARF_LOW_PC);
		Abbrevation.Add(DWARF_ADDRESS);

		Abbrevation.Add(DWARF_HIGH_PC);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_FRAME_BASE);
		Abbrevation.Add(DWARF_EXPRESSION);

		Abbrevation.Add(DWARF_NAME);
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_DECLARATION_FILE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_DECLARATION_LINE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_TYPE);
		Abbrevation.Add(DWARF_DATA_32);

		if (implementation.Metadata.IsExported)
		{
			Abbrevation.Add(DWARF_EXPORTED);
			Abbrevation.Add(DWARF_PRESENT);
		}

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		if (implementation.ReturnType != null)
		{
			Entry.Add(GetOffset(Start, GetTypeLabel(implementation.ReturnType!))); // DW_AT_type
		}
		
		foreach (var variable in implementation.Variables.Where(i => i.Value.Category == VariableCategory.LOCAL))
		{
			AppendVariable(variable.Value, file);
		}

		if (has_children) 
		{
			Entry.Add(DWARF_END); // End Of Children Mark
		}
	}

	public void AppendObjectTypeAbbrevation()
	{
		Abbrevation.Add(Index);
		Abbrevation.Add(DWARF_OBJECT_TYPE_DECLARATION);
		Abbrevation.Add(DWARF_HAS_CHILDREN);

		Abbrevation.Add(DWARF_CALLING_CONVENTION);
		Abbrevation.Add(DWARF_DATA_8);

		Abbrevation.Add(DWARF_NAME);
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_BYTE_SIZE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_DECLARATION_FILE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_DECLARATION_LINE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		ObjectTypeAbbrevation = Index++;
	}

	public void AppendBaseTypeAbbrevation()
	{
		Abbrevation.Add(Index);
		Abbrevation.Add(DWARF_BASE_TYPE_DECLARATION);
		Abbrevation.Add(DWARF_HAS_NO_CHILDREN);

		Abbrevation.Add(DWARF_NAME);
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_ENCODING);
		Abbrevation.Add(DWARF_DATA_8);

		Abbrevation.Add(DWARF_BYTE_SIZE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		BaseTypeAbbrevation = Index++;
	}

	public void AppendPointerTypeAbbrevation()
	{
		Abbrevation.Add(Index);
		Abbrevation.Add(DWARF_POINTER_TYPE_DECLARATION);
		Abbrevation.Add(DWARF_HAS_NO_CHILDREN);

		Abbrevation.Add(DWARF_TYPE);
		Abbrevation.Add(DWARF_REFERENCE_32);

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		PointerTypeAbbrevation = Index++;
	}

	public void AppendMemberVariableAbbrevation()
	{
		Abbrevation.Add(Index);
		Abbrevation.Add(DWARF_MEMBER_DECLARATION);
		Abbrevation.Add(DWARF_HAS_NO_CHILDREN);

		Abbrevation.Add(DWARF_NAME);
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_TYPE);
		Abbrevation.Add(DWARF_REFERENCE_32);

		Abbrevation.Add(DWARF_DECLARATION_FILE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_DECLARATION_LINE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_MEMBER_LOCATION);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_ACCESSIBILITY);
		Abbrevation.Add(DWARF_DATA_8);

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		MemberVariableAbbrevation = Index++;
	}

	public void AppendVariableAbbrevation()
	{
		Abbrevation.Add(Index);
		Abbrevation.Add(DWARF_VARIABLE);
		Abbrevation.Add(DWARF_HAS_NO_CHILDREN);

		Abbrevation.Add(DWARF_LOCATION);
		Abbrevation.Add(DWARF_EXPRESSION);

		Abbrevation.Add(DWARF_NAME);
		Abbrevation.Add(DWARF_STRING);

		Abbrevation.Add(DWARF_DECLARATION_FILE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_DECLARATION_LINE);
		Abbrevation.Add(DWARF_DATA_32);

		Abbrevation.Add(DWARF_TYPE);
		Abbrevation.Add(DWARF_REFERENCE_32);

		Abbrevation.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		VariableAbbrevation = Index++;
	}

	public static bool IsPointerType(Type type)
	{
		return !Types.IsPrimitive(type) && type != Types.LINK;
	}

	public void AppendMemberVariable(Variable variable)
	{
		Entry.Add(MemberVariableAbbrevation);
		Entry.Add(variable.Name);
		Entry.Add(GetOffset(Start, GetTypeLabel(variable.Type!, IsPointerType(variable.Type!))));
		Entry.Add(GetFile(variable));
		Entry.Add(GetLine(variable));
		Entry.Add(variable.LocalAlignment!);

		if (Flag.Has(variable.Modifiers, AccessModifier.PRIVATE))
		{
			Entry.Add(DWARF_ACCESS_PRIVATE);
		}
		else if (Flag.Has(variable.Modifiers, AccessModifier.PROTECTED))
		{
			Entry.Add(DWARF_ACCESS_PROTECTED);
		}
		else
		{
			Entry.Add(DWARF_ACCESS_PUBLIC);
		}
	}

	public void AppendObjectType(Type type)
	{
		Entry.Add(ObjectTypeAbbrevation);
		Entry.Add(DWARF_CALLING_CONVENTION_PASS_BY_REFERENCE);
		Entry.Add(type.Name);
		Entry.Add(type.ContentSize);
		Entry.Add(GetFile(type));
		Entry.Add(GetLine(type));

		foreach (var member in type.Variables.Values)
		{
			if (member.IsGenerated)
			{
				continue;
			}

			AppendMemberVariable(member);
		}

		Entry.Add(new TableLabel(GetTypeLabelName(type, true), Size.QWORD, true));
		Entry.Add(PointerTypeAbbrevation);
		Entry.Add(GetOffset(Start, GetTypeLabel(type)));
	}

	public void AppendType(Type type)
	{
		if (type == Types.UNIT)
		{
			return;
		}

		Entry.Add(new TableLabel(GetTypeLabelName(type), Size.QWORD, true));

		var encoding = (byte)0;

		if (type == Types.TINY)
		{
			encoding = DWARF_ENCODING_SIGNED_CHAR;
		}
		else if (type == Types.U8)
		{
			encoding = DWARF_ENCODING_UNSIGNED_CHAR;
		}
		else if (type == Types.DECIMAL)
		{
			encoding = DWARF_ENCODING_DECIMAL;
		}
		else if (type == Types.LINK)
		{
			encoding = DWARF_ENCODING_ADDRESS;
		}
		else if (type == Types.BOOL)
		{
			encoding = DWARF_ENCODING_BOOL;
		}
		else if (type is Number number)
		{
			encoding = number.IsUnsigned ? DWARF_ENCODING_UNSIGNED : DWARF_ENCODING_SIGNED;
		}
		else
		{
			AppendObjectType(type);
			return;
		}

		Entry.Add(BaseTypeAbbrevation);
		Entry.Add(type.Name);

		Entry.Add(encoding);
		Entry.Add(type.ReferenceSize);
	}

	public void AppendVariable(Variable variable, int file)
	{
		if (variable.IsGenerated)
		{
			return;
		}

		Entry.Add(VariableAbbrevation); // DW_TAG_variable

		var offset = (byte)(DWARF_OFFSET_ZERO + variable.LocalAlignment! + variable.Type!.ReferenceSize);
		AppendOperation(DWARF_OP_BASE_POINTER_OFFSET, offset); // DW_AT_location

		Entry.Add(variable.Name); // DW_AT_name

		Entry.Add(file); // DW_AT_decl_file
		Entry.Add(GetLine(variable)); // DW_AT_decl_line

		Entry.Add(GetOffset(Start, GetTypeLabel(variable.Type!, IsPointerType(variable.Type!)))); // DW_AT_type
	}

	public Debug(string file, string folder)
	{
		Abbrevation = new Table(DEBUG_ABBREVATION_TABLE) { IsSection = true };
		Entry = new Table(DEBUG_INFO_TABLE) { IsSection = true };
		Strings = new Table(DEBUG_STRING_TABLE) { IsSection = true };
		Lines = new Table(DEBUG_LINE_TABLE) { IsSection = true };

		Start = new TableLabel("debug_info_start", Size.QWORD, true);
		End = new TableLabel("debug_info_end", Size.QWORD, true);

		var version_number_label = new TableLabel("debug_info_version", Size.QWORD, true);
		
		Entry.Add(Start);
		Entry.Add(GetOffset(version_number_label, End));
		Entry.Add(version_number_label);
		Entry.Add(DWARF_VERSION);
		Entry.Add(new TableLabel(DEBUG_ABBREVATION_TABLE, Size.DWORD, false) { IsSecrel = true });
		Entry.Add((byte)Assembler.Size.Bytes);

		Lines.Add(new TableLabel(DEBUG_LINE_TABLE_START, Size.QWORD, true));

		AppendFile(file, folder, new TableLabel(COMPILATION_UNIT_START, Size.QWORD, false), new TableLabel(COMPILATION_UNIT_END, Size.QWORD, false));
	}

	public void Export(StringBuilder builder)
	{
		Entry.Add(DWARF_END);
		Abbrevation.Add(DWARF_END);

		Entry.Add(End);

		Assembler.AppendTable(builder, Abbrevation);
		Assembler.AppendTable(builder, Entry);
		Assembler.AppendTable(builder, Strings);
		Assembler.AppendTable(builder, Lines);
	}
}