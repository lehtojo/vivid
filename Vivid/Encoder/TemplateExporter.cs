using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

/// <summary>
/// Converts objects such as template functions and types to exportable formats such as mangled strings
/// </summary>
public static class ObjectExporter
{
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
	public static string? ExportType(AssemblyBuilder builder, Type type)
	{
		// 1. Skip template types since they will be exported in a different way
		// 2. Unnamed packs are not exported
		if (type.IsTemplateType || type.IsUnnamedPack) return null;

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
			builder.WriteLine($"{Assembler.ExportDirective} {mangle.Value}");
			builder.WriteLine($"{mangle.Value}:");
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
			builder.WriteLine($"{Assembler.ExportDirective} {mangle.Value}");
			builder.WriteLine($"{mangle.Value}:");
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
		
		builder.WriteLine($"{Assembler.ExportDirective} {mangle.Value}");
		builder.WriteLine($"{mangle.Value}:");

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
				if (!type.IsTemplateType || type.IsTemplateTypeVariant) continue;

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
}