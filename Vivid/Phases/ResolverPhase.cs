using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum DocumentDiagnosticSeverity
{
	ERROR = 0,
	WARNING = 1,
	INFORMATION = 2,
	HINT = 3
}

public class DocumentPosition
{
	public int Line { get; set; }
	public int Character { get; set; }

	public DocumentPosition(int line, int character)
	{
		Line = line;
		Character = character;
	}

	public bool Equals(DocumentPosition other)
	{
		return Line == other.Line && Character == other.Character;
	}
}

public class DocumentRange 
{
	public DocumentPosition Start { get; set; }
	public DocumentPosition End { get; set; }

	public DocumentRange(DocumentPosition start, DocumentPosition end)
	{
		Start = start;
		End = end;
	}

	public DocumentRange(Position? start, Position? end)
	{
		Start = start == null ? new DocumentPosition(-1, -1) : new DocumentPosition(start.Line, start.Character);
		End = end == null ? new DocumentPosition(-1, -1) : new DocumentPosition(end.Line, end.Character);
	}
}

public class DocumentDiagnostic 
{
	public DocumentRange Range { get; set; }
	public string Message { get; set; }
	public int Severity { get; set; }

	public DocumentDiagnostic(Position? start, string message, DocumentDiagnosticSeverity severity)
	{
		Range = new DocumentRange(start, start?.Translate(1));
		Message = message;
		Severity = (int)severity;

		// Find the colon after the filepath
		var i = Message.IndexOf(':');
		if (i == -1) return;
		
		// However, some filepaths have colon in them such as: C:/Users/...
		if (i + 1 < Message.Length && (Message[i + 1] == '/' || Message[i + 1] == '\\'))
		{
			i = Message.IndexOf(':', i + 1);
			if (i == -1) return;
		}

		// Skip the line number
		i = Message.IndexOf(':', i + 1);
		if (i == -1) return;

		// Skip the character number
		i = Message.IndexOf(':', i + 1);
		if (i == -1) return;

		// Skip the diagnostic type
		i = Message.IndexOf(':', i + 1);
		if (i == -1) return;

		i += 2;

		if (i > Message.Length) return;

		Message = Message[i..];
	}
}

public class ResolverPhase : Phase
{
	public static List<DocumentDiagnostic> GetFunctionDiagnostics(FunctionImplementation implementation)
	{
		var diagnostics = new List<DocumentDiagnostic>();

		// Report if the return type is not resolved
		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			diagnostics.Add(new DocumentDiagnostic(implementation.Metadata.Position, "Could not resolve the return type", DocumentDiagnosticSeverity.ERROR));
		}

		// Look for errors under the implementation node
		if (!Equals(implementation.Node, null))
		{
			foreach (var resolvable in implementation.Node.FindAll(i => i is IResolvable))
			{
				var status = ((IResolvable)resolvable).GetStatus();

				if (!status.IsProblematic)
				{
					continue;
				}

				diagnostics.Add(new DocumentDiagnostic(resolvable.Position, status.Description, DocumentDiagnosticSeverity.ERROR));
			}
		}

		// Look for variables which are not resolved
		diagnostics.AddRange(implementation.Variables.Values.Where(i => i.IsUnresolved)
			.Select(i => new DocumentDiagnostic(i.Position, $"Could not resolve type of local variable '{i.Name}'", DocumentDiagnosticSeverity.ERROR))
		);

		return diagnostics;
	}

	/// <summary>
	/// Returns list of diagnostics which describes the state of the specified context
	/// </summary>
	public static List<DocumentDiagnostic> GetDiagnostics(Context context)
	{
		var diagnostics = new List<DocumentDiagnostic>();

		foreach (var variable in context.Variables.Values.Where(i => i.IsUnresolved))
		{
			if (variable.Context.IsType)
			{
				diagnostics.Add(new DocumentDiagnostic(variable.Position, $"Could not resolve the type of the member variable '{variable.Name}'", DocumentDiagnosticSeverity.ERROR));
			}
			else if (variable.Context.Parent == null)
			{
				diagnostics.Add(new DocumentDiagnostic(variable.Position, $"Could not resolve the type of the global variable '{variable.Name}'", DocumentDiagnosticSeverity.ERROR));
			}
		}

		foreach (var type in context.Types.Values)
		{
			foreach (var supertype in type.Supertypes.Where(i => i.IsUnresolved))
			{
				diagnostics.Add(new DocumentDiagnostic(type.Position, $"Type '{type.Name}' could not inherit type '{supertype.Name}' since either it was not found or it would have caused a cyclic inheritance", DocumentDiagnosticSeverity.ERROR));
			}

			diagnostics.AddRange(GetDiagnostics(type));
		}

		foreach (var overload in context.Functions.Values.SelectMany(i => i.Overloads))
		{
			if (overload.Parameters.All(i => i.Type == null || !i.Type.IsUnresolved))
			{
				continue;
			}

			foreach (var parameter in overload.Parameters)
			{
				diagnostics.Add(new DocumentDiagnostic(parameter.Position, $"Could not resolve the type of the parameter '{parameter.Name}'", DocumentDiagnosticSeverity.ERROR));
			}
		}

		foreach (var implementation in context.GetFunctionImplementations())
		{
			diagnostics.AddRange(GetFunctionDiagnostics(implementation));
			diagnostics.AddRange(GetDiagnostics(implementation));
		}

		return diagnostics;
	}

	/// <summary>
	/// Returns list of diagnostics which describes the state of the specified context
	/// </summary>
	public static List<DocumentDiagnostic> GetDiagnostics(Context context, Node root)
	{
		var diagnostics = GetDiagnostics(context);
		var extensions = root.FindAll(i => i.Is(NodeType.EXTENSION_FUNCTION)).Cast<ExtensionFunctionNode>();

		foreach (var extension in extensions)
		{
			var status = extension.GetStatus();
			diagnostics.Add(new DocumentDiagnostic(extension.Position, status.Description, DocumentDiagnosticSeverity.ERROR));
		}

		return diagnostics;
	}

	/// <summary>
	/// Returns a string which describes the state of the specified function implementation
	/// </summary>
	public static string GetFunctionReport(FunctionImplementation implementation)
	{
		var builder = new StringBuilder();

		builder.Append($"Function {implementation.GetHeader()}:\n");

		var errors = new List<Status>();

		// Report if the return type is not resolved
		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			errors.Add(Status.Error(implementation.Metadata.Position, "Could not resolve the return type"));
		}

		// Look for errors under the implementation node
		if (!Equals(implementation.Node, null))
		{
			errors.AddRange(implementation.Node.FindAll(i => i is IResolvable)
				.Cast<IResolvable>().Select(i => i.GetStatus()).Where(i => i.IsProblematic).ToList()
			);
		}

		// Look for variables which are not resolved
		errors.AddRange(implementation.Variables.Values.Where(i => i.IsUnresolved)
			.Select(i => Status.Error(i.Position, $"Could not resolve type of local variable '{i.Name}'"))
		);

		// Build the report if there are errors
		if (!errors.Any())
		{
			return string.Empty;
		}

		foreach (var error in errors)
		{
			builder.Append($"{error.Description}\n");
		}

		return builder.ToString();
	}

	/// <summary>
	/// Returns a string which describes the state of the specified context
	/// </summary>
	public static string GetReport(Context context)
	{
		var variables = new StringBuilder();

		foreach (var variable in context.Variables.Values.Where(variable => variable.IsUnresolved))
		{
			if (variable.Context.IsType)
			{
				variables.Append(Errors.Format(variable.Position, $"Could not resolve the type of the member variable '{variable.Name}'"));
			}
			else if (variable.Context.Parent == null)
			{
				variables.Append(Errors.Format(variable.Position, $"Could not resolve the type of the global variable '{variable.Name}'"));
			}

			variables.AppendLine();
		}

		var types = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			foreach (var supertype in type.Supertypes.Where(i => i.IsUnresolved))
			{
				types.AppendLine(Errors.Format(type.Position, $"Type '{type}' could not inherit type '{supertype}' since either it was not found or it would have caused a cyclic inheritance"));
			}

			var report = GetReport(type);

			if (string.IsNullOrEmpty(report))
			{
				continue;
			}

			types.Append(report);
			types.AppendLine();
		}

		var functions = new StringBuilder();

		foreach (var overload in context.Functions.Values.SelectMany(i => i.Overloads))
		{
			if (overload.Parameters.All(i => i.Type == null || !i.Type.IsUnresolved))
			{
				continue;
			}

			if (functions.Length > 0)
			{
				functions.AppendLine();
			}

			functions.AppendLine($"Function {overload}:");

			foreach (var parameter in overload.Parameters)
			{
				functions.AppendLine(Errors.Format(parameter.Position, $"Could not resolve the type of the parameter '{parameter.Name}'"));
			}
		}

		foreach (var implementation in context.GetFunctionImplementations())
		{
			var report = GetFunctionReport(implementation);
			var subreport = GetReport(implementation);

			if (!string.IsNullOrEmpty(report))
			{
				report += "\n" + subreport;
			}

			if (!string.IsNullOrEmpty(report))
			{
				if (functions.Length > 0)
				{
					functions.AppendLine();
				}

				functions.Append(report);
			}
		}

		var builder = new StringBuilder(variables.ToString());

		if (builder.Length > 0)
		{
			builder.AppendLine();
		}

		builder.Append(types);

		if (builder.Length > 0)
		{
			builder.AppendLine();
		}

		builder.Append(functions);

		return builder.ToString();
	}

	/// <summary>
	/// Returns a string which describes the state of the specified context
	/// </summary>
	public static string GetReport(Context context, Node root)
	{
		var report = GetReport(context);
		var extensions = root.FindAll(i => i.Is(NodeType.EXTENSION_FUNCTION)).Cast<ExtensionFunctionNode>();

		if (!extensions.Any())
		{
			return report;
		}

		var builder = new StringBuilder();

		foreach (var extension in extensions)
		{
			builder.AppendLine(extension.GetStatus().Description);
		}

		builder.AppendLine();

		return builder.ToString() + report;
	}

	/// <summary>
	/// Generates the function which are used for keeping track of used objects
	/// </summary>
	private static void CreateReferenceCountingFunctions(Context root)
	{
		var instance_parameter_name = "a";

		var link = new Function(root, Modifier.DEFAULT, "link") { Position = new Position() };
		link.Position.File = Parser.AllocationFunction!.Metadata.Position!.File;
		link.Parameters.Add(new Parameter(instance_parameter_name));

		root.Declare(link);

		// Increments the reference count of the passed instance
		// Result:
		// if a != 0 { a..references += 1 }
		// => a
		link.Blueprint.AddRange(new List<Token>
		{
			new KeywordToken(Keywords.IF),
			new IdentifierToken(instance_parameter_name),
			new OperatorToken(Operators.NOT_EQUALS),
			new NumberToken(0),

			new ContentToken
			(
				ParenthesisType.CURLY_BRACKETS,
				new IdentifierToken(instance_parameter_name),
				new OperatorToken(Operators.DOT),
				new IdentifierToken(RuntimeConfiguration.REFERENCE_COUNT_VARIABLE),
				new OperatorToken(Operators.ASSIGN_ADD),
				new NumberToken(1)
			),

			new OperatorToken(Operators.IMPLICATION),
			new IdentifierToken(instance_parameter_name)
		});

		Lexer.RegisterFile(link.Blueprint, link.Position!.File!);

		var unlink = new Function(root, Modifier.DEFAULT, "unlink") { Position = new Position() };
		unlink.Position.File = Parser.AllocationFunction!.Metadata.Position!.File;
		unlink.Parameters.Add(new Parameter(instance_parameter_name));

		root.Declare(unlink);

		// Result:
		// if a != 0 and exchange_add(a..references, -1) == 1 {
		//  a.deinit()
		//  deallocate(a as link)
		// }
		unlink.Blueprint.AddRange(new List<Token>
		{
			new KeywordToken(Keywords.IF),

			new IdentifierToken(instance_parameter_name),
			new OperatorToken(Operators.NOT_EQUALS),
			new NumberToken(0),

			new OperatorToken(Operators.AND),

			new IdentifierToken(instance_parameter_name),
			new OperatorToken(Operators.DOT),
			new IdentifierToken(RuntimeConfiguration.REFERENCE_COUNT_VARIABLE),
			new OperatorToken(Operators.ATOMIC_EXCHANGE_ADD),
			new NumberToken(-1),
			new OperatorToken(Operators.EQUALS),
			new NumberToken(1),

			new ContentToken
			(
				ParenthesisType.CURLY_BRACKETS,

				new IdentifierToken(instance_parameter_name),
				new OperatorToken(Operators.DOT),
				new FunctionToken
				(
					new IdentifierToken(Keywords.DEINIT.Identifier), 
					new ContentToken()
				),

				new Token(TokenType.END),

				new FunctionToken
				(
					new IdentifierToken(Parser.DeallocationFunction!.Name), 
					new ContentToken
					(
						new IdentifierToken(instance_parameter_name),
						new KeywordToken(Keywords.AS),
						new IdentifierToken(Types.LINK.Name)
					)
				)
			)
		});

		Lexer.RegisterFile(unlink.Blueprint, unlink.Position!.File!);

		Parser.LinkFunction = link;
		Parser.UnlinkFunction = unlink;

		foreach (var type in Common.GetAllTypes(root))
		{
			if (type.IsStatic || !type.Destructors.Overloads.Any())
			{
				continue;
			}

			link.Implement(type);
			unlink.Implement(type);
		}
	}

	/// <summary>
	/// Finds the implementations of the allocation and the inheritance functions and registers them to be used
	/// </summary>
	public static void RegisterDefaultFunctions(Context context)
	{
		var allocation_function = context.GetFunction("allocate") ?? throw new ApplicationException("Missing the allocation function, please implement it or include the standard library");
		var deallocation_function = context.GetFunction("deallocate") ?? throw new ApplicationException("Missing the deallocation function, please implement it or include the standard library");
		var inheritance_function = context.GetFunction("inherits") ?? throw new ApplicationException("Missing the inheritance function, please implement it or include the standard library");

		Parser.AllocationFunction = allocation_function.GetImplementation(Types.LARGE);
		Assembler.AllocationFunction = allocation_function.GetOverload(Types.LARGE);

		Parser.DeallocationFunction = deallocation_function.GetImplementation(Types.LINK);
		Assembler.DeallocationFunction = deallocation_function.GetOverload(Types.LINK);

		Parser.InheritanceFunction = inheritance_function.GetImplementation(Types.LINK, Types.LINK);

		if (!Analysis.IsGarbageCollectorEnabled)
		{
			return;
		}

		CreateReferenceCountingFunctions(context);
	}

	public override Status Execute(Bundle bundle)
	{
		if (!bundle.Contains(ParserPhase.OUTPUT))
		{
			return Status.Error("Nothing to resolve");
		}

		var parse = bundle.Get<Parse>(ParserPhase.OUTPUT);

		var context = parse.Context;
		var report = GetReport(context, parse.Node);
		var evaluated = false;

		// Find the required functions
		RegisterDefaultFunctions(context);

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			// Try to resolve any problems in the node tree
			ParserPhase.ApplyExtensionFunctions(context, parse.Node);
			ParserPhase.ImplementFunctions(context);
			
			Resolver.ResolveContext(context);
			report = GetReport(context, parse.Node);

			// Try again only if the errors have changed
			if (report != previous) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		// The compiler must not continue if the resolver phase has failed
		if (report.Length > 0)
		{
			Console.Error.WriteLine(report);

			return Status.Error("Compilation error");
		}

		// Finds objects whose values should be evaluated or finalized
		Analysis.Complete(context);

		// Align variables in memory
		Aligner.Align(context);

		// Analyze the output
		Analyzer.Analyze(parse.Node, context);

		// Apply analysis to the functions
		Analysis.Analyze(bundle, context);

		// Reanalyze the output after the changes
		Analyzer.Analyze(parse.Node, context);

		return Status.OK;
	}
}
