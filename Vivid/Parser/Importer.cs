using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;

public static class Importer
{
	public const string ENTRY_POINT_START = "_V4initv";

	public const string WINDOWS_SHARED_LIBRARY_EXTENSION = ".dll";
	public const string UNIX_SHARED_LIBRARY_EXTENSION = ".so";

	public const string WINDOWS_STATIC_LIBRARY_EXTENSION = ".lib";
	public const string UNIX_STATIC_LIBRARY_EXTENSION = ".a";

	public const int STATIC_LIBRARY_SYMBOL_TABLE_OFFSET = 68;
	public const int STATIC_LIBRARY_SYMBOL_TABLE_FIRST_LOCATION_ENTRY_OFFSET = STATIC_LIBRARY_SYMBOL_TABLE_OFFSET + sizeof(int);

	private static Dictionary<char, Type> PrimitiveTypes { get; set; } = new Dictionary<char, Type>();

	/// <summary>
	/// Prepares the system for demangling symbols which is required for importing
	/// </summary>
	public static void Initialize()
	{
		PrimitiveTypes.Clear();

		var primitive = (Type?)null;
		
		primitive = Primitives.CreateBool();
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.DECIMAL, Format.DECIMAL);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.LARGE, Format.INT64);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.NORMAL, Format.INT32);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.SMALL, Format.INT16);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.TINY, Format.INT8);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.U64, Format.UINT64);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.U32, Format.UINT32);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.U16, Format.UINT16);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
		primitive = Primitives.CreateNumber(Primitives.U8, Format.UINT8);
		PrimitiveTypes.Add(primitive.Identifier.First(), primitive);
	}

	/// <summary>
	/// Consumes an integer starting from the specified position.
	/// </summary>
	private static (int Integer, int Position)? ConsumeInteger(string symbol, int position)
	{
		var digits = symbol.Skip(position).TakeWhile(char.IsDigit).ToArray();

		if (!int.TryParse(digits, out int integer)) return null;

		return (integer, position + digits.Length);
	}

	/// <summary>
	/// Consumes a name starting from the specified position using the specified length
	/// </summary>
	private static string ConsumeName(string symbol, int position, int length)
	{
		return new string(symbol.Skip(position).Take(length).ToArray());
	}

	/// <summary>
	/// Consumes a name starting from the specified position
	/// Examples of names: 6Symbol, 13ArrayIterator, 13response_code
	/// </summary>
	private static (string Name, int Position)? ConsumeName(string symbol, int position)
	{
		var result = ConsumeInteger(symbol, position);

		if (result == null) return null;

		position = result.Value.Position;

		var name = ConsumeName(symbol, position, result.Value.Integer);

		return (name, position + name.Length);
	}

	/// <summary>
	/// Consumes a type which has been loaded into the specified stack.
	/// This function ensures that the returned stack index is valid
	/// </summary>
	private static (int Type, int Position)? ConsumeStackType(List<MangleDefinition> stack, string symbol, int position)
	{
		var element = new string(symbol.Skip(position).TakeWhile(i => i != Mangle.STACK_REFERENCE_END).ToArray());
		var i = 0;

		if (!string.IsNullOrEmpty(element))
		{
			if (!int.TryParse(element, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i)) return null;
			i++;
		}

		position += element.Length + 1;

		if (i < 0 || i >= stack.Count) return null;

		return (i, position);
	}

	/// <summary>
	/// Wraps the specified type around a link type the specified amount of times
	/// </summary>
	private static Type CreatePointerType(Type type, int pointers)
	{
		if ((!type.IsPrimitive || type.Name == Primitives.LINK || type is ArrayType) && --pointers == 0)
		{
			return type;
		}

		for (var i = 0; i < pointers; i++)
		{
			type = new Link(type);
		}

		return type;
	}
	
	/// <summary>
	/// Consumes the next type starting from the specified position
	/// </summary>
	private static (Type Type, int Position)? ConsumeType(Context context, List<MangleDefinition> stack, string symbol, int position, bool parameter = true)
	{
		var command = symbol[position];
		var type = (Type?)null;

		if (command == Mangle.STACK_REFERENCE_COMMAND)
		{
			var result = ConsumeStackType(stack, symbol, position + 1);
			if (result == null) return null;

			type = stack[result.Value.Type].Type;

			if (type == null) throw new ApplicationException("Missing stack type");

			return (type, result.Value.Position);
		}

		// Consume all the pointer commands and count them
		var pointers = symbol.Skip(position).TakeWhile(i => i == Mangle.POINTER_COMMAND).Count();
		var pointer = pointers > 0;

		position += pointers;
		command = symbol[position];

		// Handles the following patterns: N $type-1 $type-2 ... $type-n E
		if (command == Mangle.START_LOCATION_COMMAND)
		{
			position++; // Skip over the command

			while (true)
			{
				command = symbol[position];

				if (command == Mangle.STACK_REFERENCE_COMMAND)
				{
					var result = ConsumeStackType(stack, symbol, position + 1);
					if (result == null) return null;

					position = result.Value.Position;
					type = stack[result.Value.Type].Type;

					// Something has went wrong, if the consumed type is null
					if (type == null) return null;

					context = type;
				}
				else if (char.IsDigit(command))
				{
					var result = ConsumeName(symbol, position);
					if (result == null) return null;

					position = result.Value.Position;

					// Switch the context to the type representing the extracted name
					/// NOTE: This might fail if the type is created yet, but the importer keeps trying until nothing changes.
					// Therefore, if there is something to import, it will be imported.
					var name = result.Value.Name;

					type = context.GetType(name);
					if (type == null) return null;

					context = type;
				}
				else
				{
					return null;
				}

				// Stop if the next character is the end of the location
				command = symbol[position];
				if (command != Mangle.END_COMMAND) continue;

				return (CreatePointerType(type, pointers), position + 1);
			}
		}

		// Look for primitive types
		if (!pointer && char.IsLetter(command))
		{
			// If the command is not a pointer command, it must be a primitive type
			if (!PrimitiveTypes.TryGetValue(command, out Type? primitive)) return null;

			return (primitive, position + 1);
		}

		// Disable non-pointer parameter types
		if (!pointer && parameter) return null;

		// Ensure there is a command to consume
		if (position >= symbol.Length) return null;

		command = symbol[position];

		if (char.IsDigit(command))
		{
			// Consume the name of the type
			var result = ConsumeName(symbol, position);

			if (result == null) return null;

			position = result.Value.Position;

			// Check if the type represents a template type
			if (position < symbol.Length && symbol[position] == Mangle.START_TEMPLATE_ARGUMENTS_COMMAND)
			{
				// The template type that will be constructed must be pushed to stack before it is fully created
				var definition = new MangleDefinition(null, stack.Count, 0);
				stack.Add(definition);

				// Consume the template arguments
				var arguments = ConsumeTemplateArguments(context, stack, symbol, position);
				if (arguments == null) return null;

				position = arguments.Value.Position;
				
				var template_type = (TemplateType?)null;

				// Check if the template type is already defined
				if (context.IsTypeDeclared(result.Value.Name))
				{
					type = context.GetType(result.Value.Name)!;

					// Ensure the declared type is a template type
					if (!type.IsTemplateType) return null;

					template_type = type.To<TemplateType>();

					// Require that the template type has the same amount of template arguments as captured above
					if (template_type.TemplateParameters.Count != arguments.Value.Arguments.Length) return null;
				}
				else
				{
					// Create the template type
					template_type = new TemplateType(context, result.Value.Name, Modifier.DEFAULT | Modifier.IMPORTED, arguments.Value.Arguments.Length);
				}

				// Get a template type variant using the template arguments
				type = template_type.GetVariant(arguments.Value.Arguments);

				// Now register the created template type to the definition which was pushed to the stack in the beginning
				definition.Type = type;
			}
			else
			{
				type = context.GetType(result.Value.Name) ?? new Type(context, result.Value.Name, Modifier.DEFAULT | Modifier.IMPORTED);
				stack.Add(new MangleDefinition(type, stack.Count, 0));
			}

			// Push the pointer versions of the type into the stack
			for (var i = 1; i <= pointers; i++)
			{
				type = CreatePointerType(type, pointers);
				stack.Add(new MangleDefinition(type, stack.Count, i));
			}

			return (type, position);
		}
		
		// Check if the type is already loaded into the stack
		if (command == Mangle.STACK_REFERENCE_COMMAND)
		{
			var result = ConsumeStackType(stack, symbol, position + 1);
			if (result == null) return null;

			position = result.Value.Position;

			var definition = stack[result.Value.Type];
			if (definition.Type == null) throw new ApplicationException("Missing stack type");

			type = definition.Type;

			// Push the pointer versions of the type into the stack
			for (var i = 1; i <= pointers; i++)
			{
				type = CreatePointerType(definition.Type, definition.Pointers + pointers);
				stack.Add(new MangleDefinition(type, stack.Count, definition.Pointers + i));
			}

			return (type!, result.Value.Position);
		}
		else
		{
			// It must be a primitive type
			if (!PrimitiveTypes.TryGetValue(command, out Type? primitive)) return null;

			type = primitive;

			// Push the pointer versions of the type into the stack
			for (var i = 1; i <= pointers; i++)
			{
				type = CreatePointerType(primitive, pointers);
				stack.Add(new MangleDefinition(type, stack.Count, i));
			}

			return (type, position + 1);
		}
	}

	/// <summary>
	/// Consumes a list of types until an end command is reached, starting from the specified position
	/// Examples: IxdE => [large, decimal], I5ArrayIxES_iiE => [Array<large>, Array<large>, normal, normal]
	/// </summary>
	private static (Type[] Arguments, int Position)? ConsumeTemplateArguments(Context context, List<MangleDefinition> stack, string symbol, int position)
	{
		if (symbol[position++] != Mangle.START_TEMPLATE_ARGUMENTS_COMMAND) return null;

		var types = new List<Type>();

		while (true)
		{
			var result = ConsumeType(context, stack, symbol, position);

			if (result == null) return null;

			position = result.Value.Position;
			types.Add(result.Value.Type);

			// Template arguments must always end with an end command
			if (position >= symbol.Length) return null;

			if (symbol[position] == Mangle.END_COMMAND) break;
		}

		return (types.ToArray(), position + 1); // Skip the end command 
	}

	/// <summary>
	/// Tries to find an existing function with the specified properties from the specified context.
	/// Other compiler utility functions return a function that is compatible with the properties, but this utility function is stricter.
	/// </summary>
	private static FunctionImplementation? TryFindExistingImplementation(Context context, string name, List<Type> parameter_types, Type[] template_arguments)
	{
		// Check if the function is already defined
		if (!context.IsFunctionDeclared(name)) return null;

		var function = context.GetFunction(name)!;
		var is_template_function_required = template_arguments.Length > 0;

		foreach (var overload in function.Overloads)
		{
			if (overload.IsTemplateFunction != is_template_function_required || overload.Parameters.Count != parameter_types.Count) continue;

			// Compare the parameter types strictly
			var match = true;

			for (var i = 0; i < overload.Parameters.Count; i++)
			{
				var expected = overload.Parameters[i].Type;
				var actual = parameter_types[i];

				if (expected == null || (expected.Name == actual.Name && expected.Identity == actual.Identity)) continue;

				match = false;
				break;
			}

			if (!match) continue;

			// Ensure there are the same number of template arguments
			if (overload.IsTemplateFunction && overload.To<TemplateFunction>().TemplateParameters.Count != template_arguments.Length) continue;

			return overload.Get(parameter_types);
		}

		return null;
	}

	/// <summary>
	/// Consumes a mangled function with its parameters and return type, starting from the specified position.
	/// This function also declares the consumed function.
	/// Examples: 8multiplyxd_rd => multiply(large, decimal): decimal, 3add5ArrayIxES_ii_ri => add(Array<large>, Array<large>, normal, normal): normal
	/// </summary>
	private static (Function Function, int Position)? ConsumeFunction(Context context, List<MangleDefinition> stack, string symbol, int position)
	{
		var name = ConsumeName(symbol, position);
		if (name == null) return null;

		position = name.Value.Position;
		if (position >= symbol.Length) return null;

		// When the function is inside a function example, there can be an end command to be consumed
		if (symbol[position] == Mangle.END_COMMAND) { position++; }

		if (position >= symbol.Length) return null;

		var template_arguments = Array.Empty<Type>();

		if (symbol[position] == Mangle.START_TEMPLATE_ARGUMENTS_COMMAND)
		{
			var result = ConsumeTemplateArguments(context, stack, symbol, position);
			if (result == null) return null;

			template_arguments = result.Value.Arguments;
			position = result.Value.Position;
		}

		if (position >= symbol.Length) return null;

		var parameter_types = new List<Type>();

		if (symbol[position] != Mangle.NO_PARAMETERS_COMMAND)
		{
			// Consume all parameter types
			while (true)
			{
				var type = ConsumeType(context, stack, symbol, position);
				if (type == null) return null;

				position = type.Value.Position;
				parameter_types.Add(type.Value.Type);

				if (position >= symbol.Length || symbol[position] == Mangle.PARAMETERS_END) break;
			}
		}
		else
		{
			position++; // Skip the command
		}

		var return_type = Primitives.CreateUnit();

		// Check if there is a return type defined
		if (position + 2 < symbol.Length && symbol[position] == Mangle.PARAMETERS_END && symbol[position + 1] == Mangle.START_RETURN_TYPE_COMMAND)
		{
			position += 2; // Skip the return type definition

			var result = ConsumeType(context, stack, symbol, position);
			if (result == null) return null;

			return_type = result.Value.Type;
		}

		if (template_arguments.Any())
		{
			// NOTE: We do not create a new template function, because the source code for the template function must be available for this situation

			// Try to implement the template function with the collected template arguments
			var implementation = TryFindExistingImplementation(context, name.Value.Name, parameter_types, template_arguments);
			if (implementation == null) return null; // Try later again

			// Fill in the return type straight away
			implementation.IsImported = true;
			implementation.ReturnType = return_type;

			return (implementation.Metadata, position);
		}
		else
		{
			var implementation = TryFindExistingImplementation(context, name.Value.Name, parameter_types, template_arguments);

			if (implementation != null)
			{
				implementation.IsImported = true;
				implementation.ReturnType = return_type;
				return (implementation.Metadata, position);
			}

			// Generate parameters so that the function can be created
			var parameters = new Parameter[parameter_types.Count];

			for (var i = 0; i < parameter_types.Count; i++)
			{
				parameters[i] = new Parameter($"p{i}", parameter_types[i]);
			}

			if (context.IsType && name.Value.Name == Keywords.INIT.Identifier)
			{
				// This function is a constructor:
				var constructor = new Constructor(context, Modifier.DEFAULT | Modifier.IMPORTED, null, null);
				constructor.Parameters.AddRange(parameters);

				context.To<Type>().AddConstructor(constructor);

				implementation = constructor.Implement(parameter_types);
			}
			else if (context.IsType && name.Value.Name == Keywords.DEINIT.Identifier)
			{
				// This function is a destructor:
				var destructor = new Destructor(context, Modifier.DEFAULT | Modifier.IMPORTED, null, null);
				destructor.Parameters.AddRange(parameters);
				
				context.To<Type>().AddDestructor(destructor);

				implementation = destructor.Implement(parameter_types);
			}
			else
			{
				// This function is a normal function:
				var function = new Function(context, Modifier.DEFAULT | Modifier.IMPORTED, name.Value.Name, return_type, parameters);
				implementation = function.Implementations.First();

				context.Declare(function);
			}

			// Fill in the return type straight away
			implementation.IsImported = true;
			implementation.ReturnType = return_type;

			// If the function is created inside a namespace, the function automatically becomes a static function
			if (context.IsNamespace)
			{
				implementation.Metadata.Modifiers |= Modifier.STATIC;
			}

			return (implementation.Metadata, position);
		}
	}

	/// <summary>
	/// Consumes a member variable and declares it to the specified destination type with the specified modifiers
	/// </summary>
	private static int? ConsumeVariableMetadata(Type destination, int modifiers, Context environment, List<MangleDefinition> stack, string symbol, int position)
	{
		// Consume the name of the member variable
		var name = ConsumeName(symbol, position);
		if (name == null) return null;

		position = name.Value.Position;

		// Consume the type of the member variable
		var type = ConsumeType(environment, stack, symbol, position);
		if (type == null) return null;
		
		position = type.Value.Position;

		// Declare the member variable
		var variable = destination.Declare(type.Value.Type, VariableCategory.MEMBER, name.Value.Name);
		variable.Modifiers = Modifier.Combine(variable.Modifiers, modifiers | Modifier.IMPORTED);

		// Return the position if the end has been reached
		if (position >= symbol.Length) return position;

		// Ensure the next command is supported
		var command = symbol[position];

		if (command != Mangle.START_MEMBER_VARIABLE_COMMAND && command != Mangle.START_MEMBER_VIRTUAL_FUNCTION_COMMAND && command != Mangle.END_COMMAND)
		{
			return null;
		}

		return position;
	}
	
	/// <summary>
	/// Consumes a member virtual function and declares it to the specified destination type with the specified modifiers
	/// </summary>
	private static int? ConsumeVirtualFunctionMetadata(Type destination, int modifiers, Context environment, List<MangleDefinition> stack, string symbol, int position)
	{
		var name = ConsumeName(symbol, position);
		if (name == null) return null;

		position = name.Value.Position;

		var parameter_types = new List<Type>();
		var return_type = Primitives.CreateUnit();

		while (true)
		{
			// Virtual function metadata should end with an end command
			if (position >= symbol.Length) return null;
			
			var command = symbol[position++];

			if (command == Mangle.NO_PARAMETERS_COMMAND)
			{
				if (position >= symbol.Length) return null;

				command = symbol[position];

				if (command != Mangle.END_COMMAND) throw new ApplicationException("Invalid or unsupported mangled symbol"); 
			}

			// Look for return type command
			if (command == Mangle.PARAMETERS_END)
			{
				if (position >= symbol.Length || symbol[position] == Mangle.START_RETURN_TYPE_COMMAND) return null;

				var result = ConsumeType(environment, stack, symbol, position + 1);
				if (result == null) return null;
				
				return_type = result.Value.Type;
				position = result.Value.Position;
			}

			if (command == Mangle.END_COMMAND)
			{
				var parameters = new Parameter[parameter_types.Count];

				for (var i = 0; i < parameter_types.Count; i++)
				{
					parameters[i] = new Parameter($"p{i}", parameter_types[i]);
				}

				var function = new VirtualFunction(destination, name.Value.Name, return_type, null, null);
				function.Modifiers = Modifier.Combine(modifiers, Modifier.IMPORTED);
				function.Parameters.AddRange(parameters);

				destination.Declare(function);

				return position;
			}

			var type = ConsumeType(environment, stack, symbol, position);
			if (type == null) return null;
			
			position = type.Value.Position;
			parameter_types.Add(type.Value.Type);
		}
	}

	/// <summary>
	/// Imports the specified symbol which represents type metadata
	/// Example: _T5RangeV5startxV4datax
	/// Range { 
	///   start: large
	///   end: large
	/// }
	/// </summary>
	private static bool ImportTypeMetadata(Context environment, string symbol)
	{
		var position = 2; // Skip the beginning

		var stack = new List<MangleDefinition>();

		var type = ConsumeType(environment, stack, symbol, position, false);
		if (type == null) return false;

		// Ensure the type is registered as an imported type
		var is_local_version = !type.Value.Type.IsImported;
		type.Value.Type.Modifiers |= Modifier.IMPORTED;

		// If a local version of the type exists, it already has the properties of the imported type
		if (is_local_version) return true;

		position = type.Value.Position;

		var destination = type.Value.Type;
		var visibility = Modifier.PUBLIC;

		while (true)
		{
			// If there is nothing left to consume, return true
			if (position >= symbol.Length) return true;

			var command = symbol[position++];

			if (command == Mangle.START_MEMBER_VARIABLE_COMMAND)
			{
				var result = ConsumeVariableMetadata(destination, visibility, environment, stack, symbol, position);
				if (result == null) return false;

				position = (int)result;
				continue;
			}

			if (command == Mangle.START_MEMBER_VIRTUAL_FUNCTION_COMMAND)
			{
				var result = ConsumeVirtualFunctionMetadata(destination, visibility, environment, stack, symbol, position);
				if (result == null) return false;

				position = (int)result;
				continue;
			}

			if (command == Mangle.END_COMMAND)
			{
				visibility <<= 1;
				continue;
			}
			
			return false; // Unknown command
		}
	}
	
	/// <summary>
	/// Imports the specified symbol into the specified environment context
	/// </summary>
	private static bool ImportSymbol(Context environment, string symbol)
	{
		if (symbol.StartsWith(Mangle.EXPORT_TYPE_TAG))
		{
			return ImportTypeMetadata(environment, symbol);
		}

		// Skip symbols which do not start with a known language tag
		if (!symbol.StartsWith(Mangle.CPP_LANGUAGE_TAG))
		{
			// It is normal that there are exported symbols which are not generated by this compiler
			return true;
		}

		// Skip the language tag
		/// NOTE: C language tag has the same length as the tag of this language
		var position = Mangle.VIVID_LANGUAGE_TAG.Length;
		var context = environment;

		if (position >= symbol.Length) return false;

		var command = symbol[position];
		var stack = new List<MangleDefinition>();

		if (command == Mangle.TYPE_COMMAND)
		{
			position++; // Skip the type command
			
			/// TODO: If there is two types, assume the first is a namespace
			var consumption = ConsumeType(context, stack, symbol, position, false);
			if (consumption == null) return false;

			position = consumption.Value.Position;
			context = consumption.Value.Type;

			if (position >= symbol.Length) return false;

			command = symbol[position];

			// No need to import type configurations or type descriptors
			if (command == Mangle.CONFIGURATION_COMMAND || command == Mangle.DESCRIPTOR_COMMAND) return true;

			if (command == Mangle.STATIC_VARIABLE_COMMAND)
			{
				// Consume the name of the static variable
				var name = ConsumeName(symbol, position + 1);
				if (name == null || name.Value.Position >= symbol.Length) return false;

				position = name.Value.Position;

				// The symbol should not end here, since the type of the variable is not loaded yet
				if (symbol[position] == Mangle.END_COMMAND) return false;

				// Consume the type of the variable
				var type = ConsumeType(context, stack, symbol, position);

				if (type == null) return false;

				// Declare the static variable
				var variable = context.Declare(type.Value.Type, VariableCategory.GLOBAL, name.Value.Name);
				variable.Modifiers = Modifier.Combine(variable.Modifiers, Modifier.IMPORTED | Modifier.STATIC);

				return true;
			}

			// Try to consume a name in order to check whether there is a function to be consumed
			var result = ConsumeName(symbol, position);

			if (result == null) return false;
		}
		else if (!char.IsDigit(command))
		{
			return false;
		}

		return ConsumeFunction(context, stack, symbol, position) != null;
	}

	/// <summary>
	/// Keeps importing symbols as long as the number of symbols decreases
	/// </summary>
	private static void ImportSymbols(Context context, List<string> symbols)
	{
		// The idea here is to keep importing symbols as long as the number of symbols decreases
		// NOTE: This is done, because some symbols might be dependent on other symbols that are imported later
		var previous_symbol_count = symbols.Count;

		while (true)
		{
			for (var i = symbols.Count - 1; i >= 0; i--)
			{
				if (!ImportSymbol(context, symbols[i])) continue;

				symbols.RemoveAt(i);
			}

			// If the number of unimported symbols stays the same, nothing more can be done
			if (symbols.Count == previous_symbol_count) break;

			previous_symbol_count = symbols.Count;
		}
	}

	/// <summary>
	/// Imports the specified symbols into the specified environment context
	/// </summary>
	private static void ImportAllSymbols(Context context, string[] symbols)
	{
		// Filter out entry point symbols
		symbols = symbols.Where(i => !i.StartsWith(ENTRY_POINT_START)).ToArray();

		// First, we need to import the types
		var types = symbols.Where(i => i.StartsWith(Mangle.EXPORT_TYPE_TAG)).ToList();
		ImportSymbols(context, types);

		// Then, we need to import the functions
		var functions = symbols.Where(i => !i.StartsWith(Mangle.EXPORT_TYPE_TAG)).ToList();
		ImportSymbols(context, functions);
	}

	/// <summary>
	/// Imports the specified dynamic library by finding the exported symbols and importing them
	/// </summary>
	private static bool ImportDynamicLibrary(Context context, string library)
	{
		var symbols = PeFormat.LoadExportedSymbols(library);
		if (symbols == null) return false;

		ImportAllSymbols(context, symbols);
		return true;
	}

	/// <summary>
	/// Iterates through the specified sections which represent template exports and imports them
	/// </summary>
	private static bool ImportTemplates(Context context, byte[] bytes, List<StaticLibraryFormatFileHeader> headers, string library, List<SourceFile> files)
	{
		for (var i = 0; i < headers.Count; i++)
		{
			// Look for files which represent source code of this language
			var header = headers[i];

			// Ensure the file ends with the extension of this language
			if (!header.Filename.EndsWith(ConfigurationPhase.VIVID_EXTENSION)) continue;

			var start = header.PointerOfData;
			var end = start + header.Size;

			if (start < 0 || start > bytes.Length || end < 0 || end > bytes.Length) return false;
			
			// Since the file is source code, it can be converted into text
			var text = Encoding.UTF8.GetString(bytes[start..end]);
			var file = new SourceFile(library + "/" + header.Filename, text, files.Max(i => i.Index) + 1);

			files.Add(file);

			try
			{
				// Produce tokens from the template code
				file.Tokens.AddRange(Lexer.GetTokens(text));

				// Register the file to the produced tokens
				Lexer.RegisterFile(file.Tokens, file);

				// Parse all the tokens
				var root = new ScopeNode(context, null, null, false);

				Parser.Parse(root, context, file.Tokens);
				
				file.Root = root;
				file.Context = context;

				// Find all the types and parse them
				var type_nodes = root.FindAll(NodeType.TYPE).Cast<TypeNode>().ToArray();

				foreach (var type_node in type_nodes)
				{
					type_node.Parse();
				}

				// Find all the namespaces and parse them
				var namespace_nodes = root.FindAll(NodeType.NAMESPACE).Cast<NamespaceNode>().ToArray();

				foreach (var namespace_node in namespace_nodes)
				{
					namespace_node.Parse(context);
				}
			}
			catch
			{
				// If an exception is thrown, it means that the template code from library can not be compiled for some reason
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Iterates through the specified file headers and imports all object files by adding them to the specified object file list.
	/// Object files are determined using filenames stored in the file headers.
	/// </summary>
	private static void ImportObjectFilesFromStaticLibrary(string file, List<StaticLibraryFormatFileHeader> headers, byte[] bytes, Dictionary<SourceFile, BinaryObjectFile> object_files)
	{
		foreach (var header in headers)
		{
			// Ensure the file ends with the extension of this language
			if (!header.Filename.EndsWith(AssemblyPhase.ObjectFileExtension)) continue;

			var object_file_name = file + '/' + header.Filename;
			var object_file_bytes = bytes[header.PointerOfData..(header.PointerOfData + header.Size)];
			var object_file = Assembler.IsTargetWindows
				? PeFormat.Import(object_file_name, object_file_bytes)
				: ElfFormat.Import(object_file_name, object_file_bytes);

			var object_file_source = new SourceFile(object_file_name, string.Empty, -1);
			object_files.Add(object_file_source, object_file);
		}
	}

	/// <summary>
	/// Imports all template type variants using the specified static library file headers
	/// </summary>
	private static void ImportTemplateTypeVariants(Context context, List<StaticLibraryFormatFileHeader> headers, byte[] bytes)
	{
		foreach (var header in headers)
		{
			if (!header.Filename.EndsWith(".types.templates")) continue;

			var template_variant_bytes = bytes[header.PointerOfData..(header.PointerOfData + header.Size)];
			var template_variants = Encoding.UTF8.GetString(template_variant_bytes).Split('\n');

			foreach (var template_variant in template_variants)
			{
				if (string.IsNullOrWhiteSpace(template_variant)) continue;

				// Create the template variant from the current line
				var imported_type = Common.ReadType(context, new Queue<Token>(Lexer.GetTokens(template_variant)));
				if (imported_type == null) throw new Exception("Could not to import template type variant");

				imported_type.Modifiers |= Modifier.IMPORTED;
			}
		}
	}

	/// <summary>
	/// Imports all template function variants using the specified static library file headers
	/// </summary>
	private static void ImportTemplateFunctionVariants(Context context, List<StaticLibraryFormatFileHeader> headers, byte[] bytes)
	{
		foreach (var header in headers)
		{
			if (!header.Filename.EndsWith(".functions.templates")) continue;

			var template_variant_bytes = bytes[header.PointerOfData..(header.PointerOfData + header.Size)];
			var template_variants = Encoding.UTF8.GetString(template_variant_bytes).Split('\n');

			foreach (var template_variant_text in template_variants)
			{
				if (string.IsNullOrWhiteSpace(template_variant_text)) continue;

				// Extract the container type components
				var tokens = new Queue<Token>(Lexer.GetTokens(template_variant_text));
				var components = new List<UnresolvedTypeComponent>();

				while (tokens.Any())
				{
					components.Add(Common.ReadTypeComponent(context, tokens));

					// Stop collecting type components if there are no tokens left or if the next token is not a dot operator
					if (!tokens.Any() || !tokens.Peek().Is(Operators.DOT)) break;

					tokens.Dequeue();
				}

				if (tokens.Count != 1) throw new ApplicationException("Missing template function variant parameter types");
				
				// Extract the parameter types
				var parameter_types = new List<Type>();
				tokens = new Queue<Token>(tokens.First().To<ContentToken>().Tokens);

				while (tokens.Count > 0)
				{
					var parameter_type = Common.ReadType(context, tokens);
					if (parameter_type == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);

					parameter_types.Add(parameter_type);

					if (tokens.Count == 0) break;
					if (tokens.Dequeue().Is(Operators.COMMA)) continue;

					throw new ApplicationException("Could not import template function variant: " + template_variant_text);
				}

				// Extract the type, which will contain the template function variant
				var environment = context;

				if (components.Count > 1)
				{
					environment = new UnresolvedType(components.GetRange(0, components.Count - 1).ToArray()).TryResolveType(context);
					if (environment == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);
				}

				var template_function_name = components.Last().Identifier;

				// Find the template function from the container type
				var template_function = environment.GetFunction(template_function_name);
				if (template_function == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);

				// Now, find the overload which accepts the template arguments
				var template_variant = template_function.GetImplementation(parameter_types, components.Last().Arguments);
				if (template_variant == null) throw new ApplicationException("Could not import template function variant: " + template_variant_text);

				template_variant.IsImported = true;
			}
		}
	}

	/// <summary>
	/// Imports the specified static library by finding the exported symbols and importing them
	/// </summary>
	private static bool ImportStaticLibrary(Context context, string file, List<SourceFile> files, Dictionary<SourceFile, BinaryObjectFile> object_files)
	{
		var bytes = File.ReadAllBytes(file);
		var entries = BitConverter.ToInt32(bytes[STATIC_LIBRARY_SYMBOL_TABLE_OFFSET..(STATIC_LIBRARY_SYMBOL_TABLE_OFFSET + sizeof(int))].Reverse().ToArray());

		// Skip all of the location entries to reach the actual symbol names
		var position = STATIC_LIBRARY_SYMBOL_TABLE_FIRST_LOCATION_ENTRY_OFFSET + entries * sizeof(int);

		// Load all the exported symbols
		var exported_symbols = PeFormat.LoadNumberOfStrings(bytes, position, entries);
		if (exported_symbols == null) return false;

		var headers = LoadFileHeaders(bytes);
		if (!headers.Any()) return false;

		ImportTemplates(context, bytes, headers, file, files);
		ImportAllSymbols(context, exported_symbols);
		ImportObjectFilesFromStaticLibrary(file, headers, bytes, object_files);
		ImportTemplateTypeVariants(context, headers, bytes);
		ImportTemplateFunctionVariants(context, headers, bytes);

		return true;
	}

	/// <summary>
	/// Assigns the actual filenames to the specified file headers from the specified filename table
	/// </summary>
	private static bool LoadFilenames(byte[] bytes, StaticLibraryFormatFileHeader filenames, List<StaticLibraryFormatFileHeader> headers)
	{
		foreach (var header in headers)
		{
			// Look for files which have names such as: /10
			if (!header.Filename.StartsWith("/")) continue;

			var digits = header.Filename[1..];

			if (!digits.Any() || digits.Any(i => !char.IsDigit(i))) continue;

			// This still might fail if the index is too large
			if (!int.TryParse(digits.ToArray(), out int offset)) continue;

			// Compute the position of the filename
			var position = filenames.PointerOfData + offset;

			// Check whether the position is out of bounds
			if (position < 0 || position >= bytes.Length) continue;

			var name = Encoding.UTF8.GetString(bytes.Skip(position).TakeWhile(i => i != 0).ToArray());

			header.Filename = name;
		}

		return true;
	}

	/// <summary>
	/// Loads all static library file headers from the specified file.
	/// Returns an empty list if it fails, since static libraries should not be empty
	/// </summary>
	public static List<StaticLibraryFormatFileHeader> LoadFileHeaders(byte[] bytes)
	{
		var headers = new List<StaticLibraryFormatFileHeader>();
		var position = StaticLibraryFormat.SIGNATURE.Length;

		while (position < bytes.Length)
		{
			// If a line ending is encountered, it means that the file headers have been consumed
			if (bytes[position] == '\n') break;

			// Extract the file name
			var name_buffer = bytes[position..(position + StaticLibraryFormat.FILENAME_LENGTH)];
			var name = Encoding.UTF8.GetString(name_buffer.TakeWhile(i => i != StaticLibraryFormat.PADDING_VALUE).ToArray());
			
			// Extract the file size
			position += StaticLibraryFormat.FILENAME_LENGTH + StaticLibraryFormat.TIMESTAMP_LENGTH + StaticLibraryFormat.IDENTITY_LENGTH * 2 + StaticLibraryFormat.FILEMODE_LENGTH;
			
			// Load the file size text into a string
			var size_text_buffer = bytes[position..(position + StaticLibraryFormat.SIZE_LENGTH)];
			var size_text = Encoding.UTF8.GetString(size_text_buffer.TakeWhile(i => i != StaticLibraryFormat.PADDING_VALUE).ToArray());

			// Parse the file size
			if (!int.TryParse(size_text, out int size)) return new List<StaticLibraryFormatFileHeader>();

			// Go to the end of the header, that is the start of the file data
			position += StaticLibraryFormat.SIZE_LENGTH + StaticLibraryFormat.END_COMMAND.Length;

			headers.Add(new StaticLibraryFormatFileHeader(name, size, position));

			// Skip to the next header
			position += size;
			position += position % 2;
		}

		// Try to find the section which has the actual filenames
		/// NOTE: Sometimes file headers contain their actual filenames
		var i = headers.FindIndex(i => i.Filename == StaticLibraryFormat.FILENAME_TABLE_NAME);

		// If the filename table was found, apply it to the headers
		if (i != -1 && !LoadFilenames(bytes, headers[i], headers))
		{
			return new List<StaticLibraryFormatFileHeader>();
		}

		return headers;
	}

	/// <summary>
	/// Imports the specified file.
	/// This function assumes the file represents a library
	/// </summary>
	public static bool Import(Context context, string file, List<SourceFile> files, Dictionary<SourceFile, BinaryObjectFile> object_files)
	{
		var import_context = Parser.CreateRootContext(file);

		if (file.EndsWith(WINDOWS_SHARED_LIBRARY_EXTENSION) || file.EndsWith(UNIX_SHARED_LIBRARY_EXTENSION))
		{
			if (!ImportDynamicLibrary(import_context, file)) return false;
		}
		else
		{
			if (!ImportStaticLibrary(import_context, file, files, object_files)) return false;
		}

		// Ensure all functions are marked as imported
		var functions = Common.GetAllVisibleFunctions(import_context);

		foreach (var function in functions)
		{
			function.Modifiers |= Modifier.IMPORTED;

			// Create default implementations for imported functions that do not require template arguments
			var parameter_types = function.Parameters.Select(i => i.Type).ToArray();

			if (!function.IsTemplateFunction && parameter_types.All(i => i != null && !i.IsUnresolved))
			{
				function.Get(parameter_types!);
			}

			// Register the default implementations as imported
			foreach (var implementation in function.Implementations)
			{
				implementation.IsImported = true;
			}
		}

		// Ensure all types are marked as imported
		var types = Common.GetAllTypes(import_context);

		foreach (var type in types)
		{
			type.Modifiers = Modifier.Combine(type.Modifiers, Modifier.IMPORTED);
		}

		// TODO: Verify all parameter types are resolved

		context.Merge(import_context);
		return true;
	}
}