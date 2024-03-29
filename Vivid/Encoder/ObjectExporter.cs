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
		if (Flag.Has(modifiers, Modifier.PRIVATE)) result.Add(Keywords.PRIVATE.Identifier);
		if (Flag.Has(modifiers, Modifier.PROTECTED)) result.Add(Keywords.PROTECTED.Identifier);
		if (Flag.Has(modifiers, Modifier.STATIC)) result.Add(Keywords.SHARED.Identifier);
		if (Flag.Has(modifiers, Modifier.READABLE)) result.Add(Keywords.READABLE.Identifier);
		if (Flag.Has(modifiers, Modifier.EXPORTED)) result.Add(Keywords.EXPORT.Identifier);
		if (Flag.Has(modifiers, Modifier.CONSTANT)) result.Add(Keywords.CONSTANT.Identifier);
		if (Flag.Has(modifiers, Modifier.OUTLINE)) result.Add(Keywords.OUTLINE.Identifier);
		if (Flag.Has(modifiers, Modifier.INLINE)) result.Add(Keywords.INLINE.Identifier);
		if (Flag.Has(modifiers, Modifier.PLAIN)) result.Add(Keywords.PLAIN.Identifier);
		if (Flag.Has(modifiers, Modifier.PACK)) result.Add(Keywords.PACK.Identifier);
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
		if (type.Inherited.Any())
		{
			builder.Append(string.Join(' ', type.Inherited));
			builder.Append(' ');
		}

		builder.Append(CreateTemplateName(type.Name, type.TemplateParameters));
		builder.Append(string.Join(' ', type.Blueprint.Skip(1)) + Lexer.LINE_ENDING);
		builder.Append(Lexer.LINE_ENDING);
	}

	/// <summary>
	/// Returns true if the specified function represents an actual template function or if any of its parameter types is not defined
	/// </summary>
	private static bool IsTemplateFunction(Function function)
	{
		return (function.IsTemplateFunction || function.Parameters.Any(i => i.Type == null)) && !function.IsTemplateFunctionVariant;
	}

	/// <summary>
	/// Returns true if the specified function represents an actual template function variant or if any of its parameter types is not defined
	/// </summary>
	private static bool IsTemplateFunctionVariant(Function function)
	{
		return function.IsTemplateFunctionVariant || function.Parameters.Any(i => i.Type == null);
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
				if (function.IsTemplateFunction)
				{
					ExportTemplateFunction(builder, function.To<TemplateFunction>());
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
				if (type.IsTemplateTypeVariant) continue;

				var template_functions = type.Functions.Values.SelectMany(i => i.Overloads).Where(IsTemplateFunction).ToArray();
				if (!template_functions.Any()) continue;

				if (!files.TryGetValue(iterator.Key!, out StringBuilder? builder))
				{
					builder = new StringBuilder();
					files.Add(iterator.Key!, builder);
				}

				builder.Append(type.Name);
				builder.Append(' ');
				builder.Append(ParenthesisType.CURLY_BRACKETS.Opening);

				foreach (var function in template_functions)
				{
					if (function.IsTemplateFunction)
					{
						ExportTemplateFunction(builder, function.To<TemplateFunction>());
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

	private static string ToString(Node node)
	{
		return node.Instance switch
		{
			NodeType.CAST => $"{ToString(node.First!)} as {node.To<CastNode>().GetType().ToString()}",
			NodeType.NUMBER => node.To<NumberNode>().Value.ToString()!.ToLowerInvariant(),
			NodeType.STRING => $"'{node.To<StringNode>().Text}'",
			_ => throw Errors.Get(node.Position, "Exporter does not support this constant value")
		};
	}

	/// <summary>
	/// Exports the specified function to the specified builder using the following pattern:
	/// $modifiers import $name($parameters): $return_type
	/// </summary>
	public static void ExportFunction(StringBuilder builder, Function function, FunctionImplementation implementation)
	{
		builder.Append(GetModifiers(function.Modifiers));
		builder.Append(' ');
		builder.Append(Keywords.IMPORT.Identifier);
		builder.Append(' ');
		builder.Append(function.Name);
		builder.Append('(');
		builder.Append(string.Join(", ", implementation.Parameters));
		builder.Append(")");

		// Add the return type if it is needed
		if (!Primitives.IsPrimitive(implementation.ReturnType, Primitives.UNIT))
		{
			builder.Append(": ");
			builder.Append(implementation.ReturnType);
		}

		builder.AppendLine();
	}

	/// <summary>
	/// Export constants from the specified context:
	/// Example output:
	/// constant a = 1
	/// constant b = 'Hello'
	///
	/// static Foo {
	/// constant c = 2
	/// constant d = 'There'
	///
	/// Bar {
	/// constant e = 3
	/// constant f = '!'
	/// }
	///
	/// }
	/// </summary>
	public static string ExportContext(Context context)
	{
		var builder = new StringBuilder();

		foreach (var variable in context.Variables.Values)
		{
			// Deny non-private constants
			if (variable.IsConstant && variable.IsPrivate) continue;

			// Deny hidden variables
			if (variable.IsHidden) continue;

			builder.Append(GetModifiers(variable.Modifiers));
			builder.Append(' ');
			builder.Append(variable.Name);

			if (variable.IsConstant)
			{
				// Extract the constant value
				var editor = Analyzer.GetEditor(variable.Writes.First());
				var constant_node_value = editor.Right;

				// Convert the constant value into a string
				var constant_value = ToString(constant_node_value);

				builder.Append(" = ");
				builder.AppendLine(constant_value);
				continue;
			}
			else
			{
				builder.Append(": ");
				builder.AppendLine(variable.Type!.ToString());
			}
		}

		foreach (var function in context.Functions.Values.SelectMany(i => i.Overloads))
		{
			if (IsTemplateFunction(function))
			{
				if (function.IsTemplateFunction)
				{
					ExportTemplateFunction(builder, function.To<TemplateFunction>());
				}
				else
				{
					ExportShortTemplateFunction(builder, function);
				}

				continue;
			}

			if (IsTemplateFunctionVariant(function)) continue;
			if (function.Name == Keywords.INIT.Identifier && function.IsGlobal) continue;

			// Export the function as follows: $modifiers import $name($parameters): $return_type
			foreach (var implementation in function.Implementations)
			{
				ExportFunction(builder, function, implementation);
			}
		}

		foreach (var type in context.Types.Values)
		{
			// 1. Do not export primitives, because they are created by the compiler
			// 2. Template type variants are exported separately
			if (type.IsPrimitive || type.IsTemplateTypeVariant) continue;

			// Handle template types separately
			if (type.IsTemplateType)
			{
				ExportTemplateType(builder, type.To<TemplateType>());
				continue;
			}

			if (type.Supertypes.Any())
			{
				builder.Append(string.Join(", ", type.Supertypes));
				builder.Append(' ');
			}

			var exports = ExportContext(type);

			builder.AppendLine();
			builder.Append(type.IsNamespace ? Keywords.NAMESPACE.Identifier : GetModifiers(type.Modifiers));
			builder.Append(' ');
			builder.Append(type.Name);
			builder.AppendLine(" {");
			builder.Append(exports);
			builder.AppendLine("}");
			builder.AppendLine();
		}

		return builder.ToString().ReplaceLineEndings("\n");
	}

	/// <summary>
	/// Exports all the template type variants from the specified context 
	public static string ExportTemplateTypeVariants(Context context)
	{
		var template_variants = Common.GetAllTypes(context).Where(i => i.IsTemplateTypeVariant).ToArray();
		if (template_variants.Length == 0) return string.Empty;

		// Export all variants in the following format: $T1.$T2...$Tn.$T<$P1,$P2,...,$Pn>
		var builder = new StringBuilder();

		foreach (var template_variant in template_variants)
		{
			builder.AppendLine(template_variant.ToString());
		}

		return builder.ToString().ReplaceLineEndings("\n");
	}

	/// <summary>
	/// Exports all the template function variants from the specified context using the following pattern:
	/// $T1.$T2...$Tn.$name<$U1, $U2, ..., $Un>($V1, $V2, ..., $Vn)
	/// </summary>
	public static string ExportTemplateFunctionVariants(Context context)
	{
		var template_variants = Common.GetAllFunctionImplementations(context).Where(i => i.Metadata.IsTemplateFunction || i.Metadata.Parameters.Any(i => i.Type == null)).ToArray();
		if (template_variants.Length == 0) return string.Empty;

		// Export all variants in the following format: $T1.$T2...$Tn.$name<$U1, $U2, ..., $Un>($V1, $V2, ..., $Vn)
		var builder = new StringBuilder();

		foreach (var template_variant in template_variants)
		{
			var path = template_variant.Parent!.ToString();
			builder.Append(path);

			if (!string.IsNullOrEmpty(path)) builder.Append('.');

			builder.Append(template_variant.Name);
			builder.Append('(');
			builder.Append(string.Join<Type>(", ", template_variant.ParameterTypes));
			builder.AppendLine(")");
		}

		return builder.ToString().ReplaceLineEndings("\n");
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