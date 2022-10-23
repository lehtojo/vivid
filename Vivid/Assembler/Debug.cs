using System.Collections.Generic;
using System.Linq;
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
	public bool IsSectionRelative { get; set; } = false;
	public bool Declare { get; set; }

	public TableLabel(string name, Size size, bool declare = false)
	{
		Name = name;
		Size = size;
		Declare = declare;
	}

	public TableLabel(string name, bool declare = false)
	{
		Name = name;
		Size = Size.QWORD;
		Declare = declare;
	}
}

public class Debug
{
	public static string DebugAbbreviationTable { get; set; } = "debug_abbrev";
	public static string DebugInformationTable { get; set; } = "debug_info";
	public static string DebugLineTable { get; set; } = "debug_line";

	public const string STRING_TYPE_IDENTIFIER = "String";
	public const string STRING_TYPE_DATA_VARIABLE = "data";

	public const string ARRAY_TYPE_POSTFIX = "_array";
	public const short ARRAY_TYPE_ELEMENTS = 10000;

	public const string FORMAT_COMPILATION_UNIT_START = "debug_file_{0}_start";
	public const string FORMAT_COMPILATION_UNIT_END = "debug_file_{0}_end";

	public const string DWARF_PRODUCER_TEXT = "Vivid version 1.0";
	public const short DWARF_LANGUAGE_IDENTIFIER = 0x7777;

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
	public const byte DWARF_OP_DEREFERENCE = 6;
	public const byte DWARF_OP_ADD_BYTE_CONSTANT = 35;

	public const byte DWARF_REGISTER_ZERO = 80;

	public const byte X64_DWARF_STACK_POINTER_REGISTER = 87;
	public const byte ARM64_DWARF_STACK_POINTER_REGISTER = 111;

	public const byte DWARF_TAG_COMPILE_UNIT = 17;
	public const byte DWARF_HAS_CHILDREN = 1;
	public const byte DWARF_HAS_NO_CHILDREN = 0;
	public const byte DWARF_PRODUCER = 37;
	public const byte DWARF_LANGUAGE = 19;
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
	public const byte DWARF_PARAMETER = 5;
	public const byte DWARF_INHERITANCE = 28;
	public const byte DWARF_LOCATION = 2;
	public const byte DWARF_ENCODING = 62;
	public const byte DWARF_BYTE_SIZE = 11;

	public const byte DWARF_ARRAY_TYPE = 1;
	public const byte DWARF_SUBRANGE_TYPE = 33;
	public const byte DWARF_COUNT = 55;

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

	public Table Information { get; }
	public Table Abbreviation { get; }

	public TableLabel Start { get; }
	public TableLabel End { get; }

	public int Index { get; private set; } = 1;

	public byte FileAbbreviation { get; private set; } = 0;
	public byte ObjectTypeWithMembersAbbreviation { get; private set; } = 0;
	public byte ObjectTypeWithoutMembersAbbreviation { get; private set; } = 0;
	public byte BaseTypeAbbreviation { get; private set; } = 0;
	public byte PointerTypeAbbreviation { get; private set; } = 0;
	public byte MemberVariableAbbreviation { get; private set; } = 0;
	public byte ParameterVariableAbbreviation { get; private set; } = 0;
	public byte LocalVariableAbbreviation { get; private set; } = 0;
	public byte ArrayTypeAbbreviation { get; private set; } = 0;
	public byte SubrangeTypeAbbreviation { get; private set; } = 0;
	public byte InheritanceAbbreviation { get; private set; } = 0;

	public static object GetOffset(TableLabel from, TableLabel to)
	{
		return new Offset(from, to);
	}

	public void BeginFile(SourceFile file)
	{
		Information.Add(FileAbbreviation); // DW_TAG_compile_unit
		Information.Add(DWARF_PRODUCER_TEXT); // DW_AT_producer
		Information.Add(DWARF_LANGUAGE_IDENTIFIER); // DW_AT_language

		var fullname = file.Fullname;

		if (fullname.StartsWith(Environment.CurrentDirectory))
		{
			fullname = fullname.Remove(0, Environment.CurrentDirectory.Length);
			fullname = fullname.Insert(0, ".");
		}

		Information.Add(fullname.Replace("\\", "/")); // DW_AT_name

		Information.Add(new TableLabel(DebugLineTable, Size.DWORD, false) { IsSectionRelative = Settings.IsX64 && Settings.IsTargetWindows }); // DW_AT_stmt_list

		Information.Add(Environment.CurrentDirectory.Replace("\\", "/") ?? throw new ApplicationException("Could not retrieve source file folder")); // DW_AT_comp_dir

		var start = new TableLabel(string.Format(FORMAT_COMPILATION_UNIT_START, file.Index));
		var end = new TableLabel(string.Format(FORMAT_COMPILATION_UNIT_END, file.Index));

		Information.Add(start); // DW_AT_low_pc
		Information.Add(GetOffset(start, end)); /// DW_AT_high_pc
	}

	public static TableLabel GetEnd(FunctionImplementation implementation)
	{
		return new TableLabel(implementation.GetFullname() + "_end", Size.QWORD, false);
	}

	public static int GetFile(FunctionImplementation implementation)
	{
		return implementation.Metadata.Start?.File?.Index ?? throw new ApplicationException($"Declaration file of function '{implementation.GetHeader()}' missing");
	}

	public static int GetLine(FunctionImplementation implementation)
	{
		return implementation.Metadata!.Start?.FriendlyLine ?? throw new ApplicationException($"Declaration position of function '{implementation.GetHeader()}' missing");
	}

	public static int GetFile(Type type)
	{
		return type.Position?.File?.Index ?? throw new ApplicationException($"Declaration file of type '{type}' missing");
	}

	public static int GetLine(Type type)
	{
		return type.Position?.FriendlyLine ?? throw new ApplicationException($"Declaration position of type '{type.Name}' missing");
	}

	public static int GetFile(Variable variable)
	{
		return variable.Position?.File?.Index ?? throw new ApplicationException($"Declaration file of variable '{variable.Name}' missing");
	}

	public static int GetLine(Variable variable)
	{
		return variable.Position?.FriendlyLine ?? throw new ApplicationException($"Declaration position of variable '{variable.Name}' missing");
	}

	public static string GetTypeLabelName(Type type, bool pointer = false)
	{
		if (Primitives.IsPrimitive(type, Primitives.LINK))
		{
			return type.GetFullname();
		}

		if (type.IsPrimitive)
		{
			if (pointer) throw new NotSupportedException("Pointer of a primitive type required, but it was not requested using a link type");

			return Mangle.VIVID_LANGUAGE_TAG + type.Identifier;
		}

		/// NOTE: Since the type is a user defined type, it must have a pointer symbol in its fullname. It must be removed, if the pointer flag is set to true.
		var fullname = type.GetFullname();
		return pointer ? fullname.Insert(Mangle.VIVID_LANGUAGE_TAG.Length, Mangle.POINTER_COMMAND.ToString()) : fullname;
	}

	public static TableLabel GetTypeLabel(Type type, HashSet<Type> types, bool pointer = false)
	{
		types.Add(type);
		return new TableLabel(GetTypeLabelName(type, pointer), Size.QWORD, false);
	}

	public void AddOperation(byte command, params byte[] parameters)
	{
		Information.Add((byte)(parameters.Length + 1)); // Length of the operation
		Information.Add(command);
		parameters.ForEach(i => Information.Add(i));
	}

	public void AddFunction(FunctionImplementation implementation, HashSet<Type> types)
	{
		var file = GetFile(implementation);
		var abbreviation = ToULEB128(Index++); // DW_TAG_subprogram

		foreach (var value in abbreviation) Information.Add(value);

		var start = new TableLabel(implementation.GetFullname(), Size.QWORD, false);
		Information.Add(start); // DW_AT_low_pc

		Information.Add(GetOffset(start, GetEnd(implementation))); // DW_AT_high_pc

		AddOperation(Settings.IsX64 ? X64_DWARF_STACK_POINTER_REGISTER : ARM64_DWARF_STACK_POINTER_REGISTER); // DW_AT_frame_base
		Information.Add(implementation.GetHeader()); // DW_AT_name
		Information.Add(file); // DW_AT_decl_file
		Information.Add(GetLine(implementation)); // DW_AT_decl_line

		var has_children = implementation.Self != null || implementation.Parameters.Any() || implementation.Locals.Any();

		foreach (var value in abbreviation) Abbreviation.Add(value);

		Abbreviation.Add(DWARF_FUNCTION);
		Abbreviation.Add(has_children ? DWARF_HAS_CHILDREN : DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_LOW_PC);
		Abbreviation.Add(DWARF_ADDRESS);

		Abbreviation.Add(DWARF_HIGH_PC);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_FRAME_BASE);
		Abbreviation.Add(DWARF_EXPRESSION);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_DECLARATION_FILE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_LINE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		if (implementation.Metadata.IsExported)
		{
			Abbreviation.Add(DWARF_EXPORTED);
			Abbreviation.Add(DWARF_PRESENT);
		}

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		if (implementation.ReturnType != null)
		{
			Information.Add(GetOffset(Start, GetTypeLabel(implementation.ReturnType!, types))); // DW_AT_type
		}

		foreach (var local in implementation.Locals)
		{
			AddLocalVariable(local, types, file, implementation.SizeOfLocalMemory);
		}

		var self = implementation.GetSelfPointer();

		if (self != null)
		{
			AddParameterVariable(self, types, file, implementation.SizeOfLocalMemory);
		}

		foreach (var parameter in implementation.Parameters)
		{
			AddParameterVariable(parameter, types, file, implementation.SizeOfLocalMemory);
		}

		if (has_children)
		{
			Information.Add(DWARF_END); // End Of Children Mark
		}
	}

	public void AddFileAbbreviation()
	{
		Abbreviation.Add((byte)Index); // Define the current abbreviation code

		Abbreviation.Add(DWARF_TAG_COMPILE_UNIT); // This is a compile unit and it has children
		Abbreviation.Add(DWARF_HAS_CHILDREN);

		Abbreviation.Add(DWARF_PRODUCER); // The producer is identified with a string pointer
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_LANGUAGE); // The language is identified with a short integer
		Abbreviation.Add(DWARF_DATA_16);

		Abbreviation.Add(DWARF_NAME); // The name of the file is added with a string pointer
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_LINE_NUMBER_INFORMATION); // The line number information is added with a section offset
		Abbreviation.Add(DWARF_DATA_SECTION_OFFSET);

		Abbreviation.Add(DWARF_COMPILATION_FOLDER); // The compilation folder is added with a string pointer
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_LOW_PC);
		Abbreviation.Add(DWARF_ADDRESS);

		Abbreviation.Add(DWARF_HIGH_PC);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		FileAbbreviation = (byte)Index++;
	}

	public void AddObjectTypeWithMembersAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_OBJECT_TYPE_DECLARATION);
		Abbreviation.Add(DWARF_HAS_CHILDREN);

		Abbreviation.Add(DWARF_CALLING_CONVENTION);
		Abbreviation.Add(DWARF_DATA_8);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_BYTE_SIZE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_FILE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_LINE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		ObjectTypeWithMembersAbbreviation = (byte)Index++;
	}

	public void AddObjectTypeWithoutMembersAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_OBJECT_TYPE_DECLARATION);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_CALLING_CONVENTION);
		Abbreviation.Add(DWARF_DATA_8);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_BYTE_SIZE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_FILE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_LINE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		ObjectTypeWithoutMembersAbbreviation = (byte)Index++;
	}

	public void AddBaseTypeAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_BASE_TYPE_DECLARATION);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_ENCODING);
		Abbreviation.Add(DWARF_DATA_8);

		Abbreviation.Add(DWARF_BYTE_SIZE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		BaseTypeAbbreviation = (byte)Index++;
	}

	public void AddPointerTypeAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_POINTER_TYPE_DECLARATION);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		PointerTypeAbbreviation = (byte)Index++;
	}

	public void AddMemberVariableAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_MEMBER_DECLARATION);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_DECLARATION_FILE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_LINE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_MEMBER_LOCATION);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_ACCESSIBILITY);
		Abbreviation.Add(DWARF_DATA_8);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		MemberVariableAbbreviation = (byte)Index++;
	}

	public void AddLocalVariableAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_VARIABLE);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_LOCATION);
		Abbreviation.Add(DWARF_EXPRESSION);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_DECLARATION_FILE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_LINE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		LocalVariableAbbreviation = (byte)Index++;
	}

	public void AddParameterVariableAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_PARAMETER);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_LOCATION);
		Abbreviation.Add(DWARF_EXPRESSION);

		Abbreviation.Add(DWARF_NAME);
		Abbreviation.Add(DWARF_STRING);

		Abbreviation.Add(DWARF_DECLARATION_FILE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_DECLARATION_LINE);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		ParameterVariableAbbreviation = (byte)Index++;
	}

	public void AddArrayTypeAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_ARRAY_TYPE);
		Abbreviation.Add(DWARF_HAS_CHILDREN);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		ArrayTypeAbbreviation = (byte)Index++;
	}

	public void AddSubrangeTypeAbbreviation()
	{
		Abbreviation.Add((byte)Index);
		Abbreviation.Add(DWARF_SUBRANGE_TYPE);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_COUNT);
		Abbreviation.Add(DWARF_DATA_16);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		SubrangeTypeAbbreviation = (byte)Index++;
	}

	public void AddInheritanceAbbreviation()
	{
		Abbreviation.Add((byte)Index);

		Abbreviation.Add(DWARF_INHERITANCE);
		Abbreviation.Add(DWARF_HAS_NO_CHILDREN);

		Abbreviation.Add(DWARF_TYPE);
		Abbreviation.Add(DWARF_REFERENCE_32);

		Abbreviation.Add(DWARF_MEMBER_LOCATION);
		Abbreviation.Add(DWARF_DATA_32);

		Abbreviation.Add(DWARF_ACCESSIBILITY);
		Abbreviation.Add(DWARF_DATA_8);

		Abbreviation.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		InheritanceAbbreviation = (byte)Index++;
	}

	public static bool IsPointerType(Type type)
	{
		return !type.IsPrimitive && !type.IsPack;
	}

	public void AddMemberVariable(Variable variable, HashSet<Type> types)
	{
		if (variable.Type is ArrayType) return;
		
		Information.Add(MemberVariableAbbreviation);
		Information.Add(variable.Name.Replace(".", ""));
		Information.Add(GetOffset(Start, GetTypeLabel(variable.Type!, types, IsPointerType(variable.Type!))));
		Information.Add(GetFile(variable));
		Information.Add(GetLine(variable));
		Information.Add(variable.GetAlignment(variable.Parent.To<Type>()) ?? throw new ApplicationException("Missing member variable alignment"));

		if (Flag.Has(variable.Modifiers, Modifier.PRIVATE))
		{
			Information.Add(DWARF_ACCESS_PRIVATE);
		}
		else if (Flag.Has(variable.Modifiers, Modifier.PROTECTED))
		{
			Information.Add(DWARF_ACCESS_PROTECTED);
		}
		else
		{
			Information.Add(DWARF_ACCESS_PUBLIC);
		}
	}

	public void AddObjectType(Type type, HashSet<Type> types)
	{
		var members = type.Variables.Values.Where(i => !i.IsStatic && !i.IsConstant).ToArray();
		var has_members = type.Supertypes.Any() || members.Any();

		Information.Add(has_members ? ObjectTypeWithMembersAbbreviation : ObjectTypeWithoutMembersAbbreviation);
		Information.Add(DWARF_CALLING_CONVENTION_PASS_BY_REFERENCE);
		Information.Add(type.Name);
		Information.Add(type.ContentSize);
		Information.Add(GetFile(type));
		Information.Add(GetLine(type));

		// Include the supertypes
		foreach (var supertype in type.Supertypes)
		{
			Information.Add(InheritanceAbbreviation);
			Information.Add(GetOffset(Start, GetTypeLabel(supertype, types)));
			Information.Add(type.GetSupertypeBaseOffset(supertype) ?? throw new ApplicationException("Could not resolve supertype base offset"));
			Information.Add(DWARF_ACCESS_PUBLIC);
		}

		foreach (var member in members)
		{
			// NOTE: This is a bit hacky, but it should not cause any harm and is a temporary feature
			var hidden = member.IsGenerated;
			if (hidden) { member.Position = type.Position!; }

			AddMemberVariable(member, types);

			// Remove the temporary position
			if (hidden) { member.Position = null; }
		}

		if (has_members) Information.Add(DWARF_END);

		Information.Add(new TableLabel(GetTypeLabelName(type, true), Size.QWORD, true));
		Information.Add(PointerTypeAbbreviation);
		Information.Add(GetOffset(Start, GetTypeLabel(type, types)));
	}
	
	/// <summary>
	/// Appends a link type which enables the user to see its elements
	/// </summary>
	public void AddArrayLink(Type type, Type element, HashSet<Type> types)
	{
		// Create the array type
		var is_pointer = IsPointerType(element);
		var name = GetTypeLabelName(type, is_pointer) + ARRAY_TYPE_POSTFIX;
		var subrange = new TableLabel(name, Size.QWORD, true);

		Information.Add(subrange);
		Information.Add(ArrayTypeAbbreviation); // Abbreviation code
		Information.Add(GetOffset(Start, GetTypeLabel(element, types, is_pointer))); // DW_AT_type

		Information.Add(SubrangeTypeAbbreviation); // Abbreviation code
		Information.Add(GetOffset(Start, GetTypeLabel(element, types, is_pointer))); // DW_AT_type
		Information.Add(ARRAY_TYPE_ELEMENTS); // DW_AT_count

		Information.Add(DWARF_END); // End of children

		Information.Add(new TableLabel(GetTypeLabelName(type, true), Size.QWORD, true));
		Information.Add(PointerTypeAbbreviation);
		Information.Add(GetOffset(Start, subrange));

		types.Add(element);
	}

	public void AddLink(Type type, HashSet<Type> types)
	{
		var element = type.GetAccessorType() ?? throw new ApplicationException("Missing link offset type");

		if (!Primitives.IsPrimitive(element, Primitives.BYTE) && !Primitives.IsPrimitive(element, Primitives.CHAR) && !Primitives.IsPrimitive(element, Primitives.U8))
		{
			AddArrayLink(type, element, types);
			return;
		}

		Information.Add(new TableLabel(GetTypeLabelName(type, true), Size.QWORD, true));
		Information.Add(PointerTypeAbbreviation);
		Information.Add(GetOffset(Start, GetTypeLabel(element, types, IsPointerType(element))));

		types.Add(element);
	}

	public void AddType(Type type, HashSet<Type> types)
	{
		if (Primitives.IsPrimitive(type, Primitives.LINK))
		{
			AddLink(type, types);
			return;
		}

		Information.Add(new TableLabel(GetTypeLabelName(type), Size.QWORD, true));

		var encoding = (byte)0;

		if (type.IsPrimitive)
		{
			encoding = type.Name switch
			{
				Primitives.U8 => DWARF_ENCODING_UNSIGNED_CHAR,
				Primitives.BYTE => DWARF_ENCODING_UNSIGNED_CHAR,
				Primitives.DECIMAL => DWARF_ENCODING_DECIMAL,
				Primitives.BOOL => DWARF_ENCODING_BOOL,
				Primitives.UNIT => DWARF_ENCODING_SIGNED,
				_ => (byte)0
			};

			if (encoding == 0 && type is Number number)
			{
				encoding = number.IsUnsigned ? DWARF_ENCODING_UNSIGNED : DWARF_ENCODING_SIGNED;
			}
			else if (encoding == 0 && type is ArrayType)
			{
				encoding = DWARF_ENCODING_SIGNED_CHAR;
			}
		}

		if (encoding == 0)
		{
			AddObjectType(type, types);
			return;
		}

		Information.Add(BaseTypeAbbreviation);
		Information.Add(type.Name);

		Information.Add(encoding);
		Information.Add(type.AllocationSize);
	}

	public static byte[] ToULEB128(int value)
	{
		var bytes = new List<byte>();

		do
		{
			var x = value & 0x7F;
			value >>= 7;

			if (value != 0)
			{
				x |= (1 << 7);
			}
	
			bytes.Add((byte)x);

		} while (value != 0);

		return bytes.ToArray();
	}

	public static byte[] ToSLEB128(int value)
	{
		var bytes = new List<byte>();

		var more = true;
		var negative = value < 0;

		while (more) 
		{
			var x = value & 0x7F;
			value >>= 7;

			// The following is only necessary if the implementation of >>= uses a logical shift rather than an arithmetic shift for a signed left operand
			if (negative)
			{
				value |= (~0 << (sizeof(int) * 8 - 7)); // Sign extend
			}

			// Sign bit of byte is second high order bit (0x40)
			if ((value == 0 && ((x & 0x40) == 0)) || (value == -1 && ((x & 0x40) == 0x40)))
			{
				more = false;
			}
			else
			{
				x |= (1 << 7);
			}

			bytes.Add((byte)x);
		}

		return bytes.ToArray();
	}

	/// <summary>
	/// Returns whether specified variable is a string
	/// </summary>
	private static bool IsStringType(Variable variable)
	{
		return variable.Type != null && variable.Type.Name == STRING_TYPE_IDENTIFIER && variable.Type.Parent!.IsGlobal;
	}

	public void AddLocalVariable(Variable variable, HashSet<Type> types, int file, int local_memory_size)
	{
		// Do not add generated variables
		if (variable.IsGenerated || variable.Type is ArrayType) return;

		// Before adding the local variable, it must have a stack alignment
		var alignment = variable.LocalAlignment;
		if (alignment == null) return;

		Information.Add(LocalVariableAbbreviation); // DW_TAG_variable
		
		var type = variable.Type ?? throw new ApplicationException("Missing variable type");
		var local_variable_alignment = ToSLEB128(local_memory_size + (int)alignment);

		if (IsStringType(variable))
		{
			// Get the member variable which points to the actual data in the string type
			var data = type.GetVariable(STRING_TYPE_DATA_VARIABLE) ?? throw new ApplicationException("Missing string data variable");
			
			alignment = data.LocalAlignment ?? throw new ApplicationException("Member variable was not aligned");
			type = data.Type ?? throw new ApplicationException("Missing variable type");

			var data_variable_alignment = ToSLEB128((int)alignment);

			if (data_variable_alignment.Length != 1) throw new ApplicationException("String member variable has too large offset");

			AddOperation(DWARF_OP_BASE_POINTER_OFFSET, local_variable_alignment.Concat(new[] { DWARF_OP_DEREFERENCE, DWARF_OP_ADD_BYTE_CONSTANT, data_variable_alignment[0] }).ToArray()); // DW_AT_location
		}
		else
		{
			AddOperation(DWARF_OP_BASE_POINTER_OFFSET, local_variable_alignment); // DW_AT_location
		}

		Information.Add(variable.Name); // DW_AT_name

		Information.Add(file); // DW_AT_decl_file
		Information.Add(GetLine(variable)); // DW_AT_decl_line

		Information.Add(GetOffset(Start, GetTypeLabel(type, types, IsPointerType(type)))); // DW_AT_type
	}

	public void AddParameterVariable(Variable variable, HashSet<Type> types, int file, int local_memory_size)
	{
		// Do not add generated variables
		if (variable.IsGenerated || variable.Type is ArrayType) return;

		// Before adding the local variable, it must have a stack alignment
		var alignment = variable.LocalAlignment;
		if (alignment == null) return;

		Information.Add(ParameterVariableAbbreviation); // DW_TAG_variable
		
		var type = variable.Type ?? throw new ApplicationException("Missing variable type");
		var parameter_alignment = ToSLEB128(local_memory_size + (int)alignment);

		if (IsStringType(variable))
		{
			// Get the member variable which points to the actual data in the string type
			var data = type.GetVariable(STRING_TYPE_DATA_VARIABLE) ?? throw new ApplicationException("Missing string data variable");
			
			alignment = data.LocalAlignment ?? throw new ApplicationException("Member variable was not aligned");
			type = data.Type ?? throw new ApplicationException("Missing variable type");

			var data_variable_alignment = ToSLEB128((int)alignment);

			if (data_variable_alignment.Length != 1) throw new ApplicationException("String member variable has too large offset");

			AddOperation(DWARF_OP_BASE_POINTER_OFFSET, parameter_alignment.Concat(new[] { DWARF_OP_DEREFERENCE, DWARF_OP_ADD_BYTE_CONSTANT, data_variable_alignment[0] }).ToArray()); // DW_AT_location
		}
		else
		{
			AddOperation(DWARF_OP_BASE_POINTER_OFFSET, parameter_alignment); // DW_AT_location
		}

		Information.Add(variable.Name); // DW_AT_name

		Information.Add(file); // DW_AT_decl_file
		Information.Add(GetLine(variable)); // DW_AT_decl_line

		Information.Add(GetOffset(Start, GetTypeLabel(type, types, IsPointerType(type)))); // DW_AT_type
	}

	public Debug()
	{
		Abbreviation = new Table(DebugAbbreviationTable) { IsSection = true };
		Information = new Table(DebugInformationTable) { IsSection = true };

		Start = new TableLabel("debug_info_start", Size.QWORD, true);
		End = new TableLabel("debug_info_end", Size.QWORD, true);

		var version_number_label = new TableLabel("debug_info_version", Size.QWORD, true);

		Information.Add(Start);
		Information.Add(GetOffset(version_number_label, End));
		Information.Add(version_number_label);
		Information.Add(DWARF_VERSION);
		Information.Add(new TableLabel(DebugAbbreviationTable, Size.DWORD, false) { IsSectionRelative = Settings.IsX64 && Settings.IsTargetWindows });
		Information.Add((byte)Settings.Bytes);

		AddFileAbbreviation();
		AddObjectTypeWithMembersAbbreviation();
		AddObjectTypeWithoutMembersAbbreviation();
		AddBaseTypeAbbreviation();
		AddPointerTypeAbbreviation();
		AddMemberVariableAbbreviation();
		AddParameterVariableAbbreviation();
		AddLocalVariableAbbreviation();
		AddArrayTypeAbbreviation();
		AddSubrangeTypeAbbreviation();
		AddInheritanceAbbreviation();
	}

	public void EndFile()
	{
		Information.Add(DWARF_END);
	}

	public AssemblyBuilder Export(SourceFile file)
	{
		Information.Add(DWARF_END);
		Abbreviation.Add(DWARF_END);

		Information.Add(End);

		var builder = new AssemblyBuilder();
		Assembler.AddTable(builder, Abbreviation, TableMarker.TextualAssembly);
		Assembler.AddTable(builder, Information, TableMarker.TextualAssembly);

		if (Settings.IsDebuggingEnabled)
		{
			var abbreviation_section = builder.GetDataSection(file, Abbreviation.Name);
			var information_section = builder.GetDataSection(file, Information.Name);

			// The abbreviation and information sections must be tightly packed and the start of these sections will be at least multiple of 8 bytes (this is guaranteed by the linker).s
			abbreviation_section.Alignment = 1;
			information_section.Alignment = 1;

			DataEncoder.AddTable(builder, abbreviation_section, Abbreviation, TableMarker.DataEncoder);
			DataEncoder.AddTable(builder, information_section, Information, TableMarker.DataEncoder);
		}

		return builder;
	}
}