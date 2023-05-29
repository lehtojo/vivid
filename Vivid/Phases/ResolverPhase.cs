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

	public bool Equals(Position other)
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
	private const string UNTITLED_FILE_SCHEME = "untitled";

	public DocumentRange Range { get; set; }
	public SourceFile? File { get; set; }
	public string Message { get; set; }
	public int Severity { get; set; }

	public DocumentDiagnostic(Position? start, string message, DocumentDiagnosticSeverity severity)
	{
		Range = new DocumentRange(start, start?.Translate(1));
		File = start != null ? start.File : null;
		Message = message;
		Severity = (int)severity;

		// Find the colon after the filepath
		var i = Message.IndexOf(':');
		if (i == -1) return;
		
		// However, some filepaths have colon in them such as 'C:/Users/...' and 'untitled:Untitled'
		if (i + 1 < Message.Length && (Message[i + 1] == '/' || Message[i + 1] == '\\' || Message.Substring(0, i) == UNTITLED_FILE_SCHEME))
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
	/// <summary>
	/// Finds supertypes which are not constructed and reports them
	/// </summary>
	private static List<DocumentDiagnostic> FindUnconstructedSupertypes(Type type)
	{
		var diagnostics = new List<DocumentDiagnostic>();

		if (type.IsImported || !type.IsUserDefined) return diagnostics;

		// Look for types which inherit some other type
		if (!type.Supertypes.Any()) return diagnostics;

		// If any of the inherited types do not have a default constructor, the compiler can not generate automatic construction for the supertypes
		var types = type.Supertypes.FindAll(i => i.Constructors.GetOverload() == null);
		if (!types.Any()) return diagnostics;

		foreach (var constructor in type.Constructors.Overloads.SelectMany(i => i.Implementations))
		{
			var links = constructor.Node!.FindAll(NodeType.LINK);
			var supertypes = new HashSet<Type>(type.Supertypes.Where(i => !i.IsUnresolved));

			foreach (var link in links)
			{
				// Skip all link which are not function calls
				if (!link.Right.Is(NodeType.FUNCTION)) continue;

				// Extract the called function
				var implementation = link.Right.To<FunctionNode>().Function;

				// 1. Require the called function to be constructor
				// 2. It must use the self pointer
				if (!implementation.Metadata.IsConstructor || !ReconstructionAnalysis.IsUsingLocalSelfPointer(link.Right)) continue;

				// Remove the constructed type from the supertypes, if it is there
				supertypes.Remove((Type)implementation.Parent!);
			}

			// If there is any supertype, which was not constructed, it is an error
			if (!supertypes.Any()) continue;

			foreach (var supertype in supertypes)
			{
				diagnostics.Add(new DocumentDiagnostic(constructor.Metadata.Start, $"Can not automatically construct the inherited type '{supertype}', because it does not have a default constructor", DocumentDiagnosticSeverity.ERROR));
			}
		}

		return diagnostics;
	}

	public static List<DocumentDiagnostic> GetFunctionDiagnostics(FunctionImplementation implementation)
	{
		var diagnostics = new List<DocumentDiagnostic>();

		// Report if the return type is not resolved
		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			diagnostics.Add(new DocumentDiagnostic(implementation.Metadata.Start, "Can not resolve the return type", DocumentDiagnosticSeverity.ERROR));
		}

		// Look for errors under the implementation node
		if (!Equals(implementation.Node, null))
		{
			foreach (var resolvable in implementation.Node.FindAll(i => i is IResolvable))
			{
				var status = ((IResolvable)resolvable).GetStatus();
				if (!status.IsProblematic) continue;

				diagnostics.Add(new DocumentDiagnostic(resolvable.Position, status.Description, DocumentDiagnosticSeverity.ERROR));
			}

			// Ensure assignments are used properly
			var nodes = implementation.Node.FindAll(i => i.Is(OperatorType.ASSIGNMENT));

			foreach (var iterator in nodes)
			{
				if (iterator.Left.Is(NodeType.VARIABLE, NodeType.LINK, NodeType.ACCESSOR)) continue;
				diagnostics.Add(new DocumentDiagnostic(iterator.Position, "Can not understand the assignment", DocumentDiagnosticSeverity.ERROR));
			}

			// Ensure all the assignments are exposed to their parent scopes
			foreach (var iterator in nodes)
			{
				if (iterator.Parent == null || iterator.Parent.Is(NodeType.SCOPE, NodeType.NORMAL, NodeType.INLINE)) continue;
				diagnostics.Add(new DocumentDiagnostic(iterator.Position, "Assignment must be exposed", DocumentDiagnosticSeverity.ERROR));
			}

			// Ensure increments and decrements are used properly
			nodes = implementation.Node.FindAll(NodeType.INCREMENT, NodeType.DECREMENT);

			foreach (var iterator in nodes)
			{
				var source = Analyzer.GetSource(iterator);
				if (source.Is(NodeType.CALL, NodeType.CONSTRUCTION, NodeType.DATA_POINTER, NodeType.DECREMENT, NodeType.FUNCTION, NodeType.INCREMENT, NodeType.LINK, NodeType.NEGATE, NodeType.NOT, NodeType.ACCESSOR, NodeType.STACK_ADDRESS, NodeType.VARIABLE)) continue;

				var name = iterator.Is(NodeType.INCREMENT) ? "increment" : "decrement";
				diagnostics.Add(new DocumentDiagnostic(iterator.Position, $"Can not understand the {name}", DocumentDiagnosticSeverity.ERROR));
			}
		}

		// Look for variables which are not resolved
		diagnostics.AddRange(implementation.Variables.Values.Where(i => i.IsUnresolved)
			.Select(i => new DocumentDiagnostic(i.Position, $"Can not resolve type of local variable '{i.Name}'", DocumentDiagnosticSeverity.ERROR))
		);

		return diagnostics;
	}

	/// <summary>
	/// Returns list of diagnostics which describes the state of the specified context
	/// </summary>
	public static List<DocumentDiagnostic> GetDiagnostics(Context context)
	{
		var diagnostics = new List<DocumentDiagnostic>();

		// Go through all the variables which are unresolved
		foreach (var variable in context.Variables.Values.Where(i => i.IsUnresolved))
		{
			diagnostics.Add(new DocumentDiagnostic(variable.Position, $"Can not resolve the type of variable '{variable}'", DocumentDiagnosticSeverity.ERROR));
		}

		foreach (var type in context.Types.Values)
		{
			diagnostics.AddRange(FindUnconstructedSupertypes(type));

			foreach (var supertype in type.Supertypes.Where(i => i.IsUnresolved))
			{
				diagnostics.Add(new DocumentDiagnostic(type.Position, $"Type '{type}' can not inherit type '{supertype}' since either it was not found or it would have caused a cyclic inheritance", DocumentDiagnosticSeverity.ERROR));
			}

			foreach (var virtual_function in type.Virtuals.Values.SelectMany(i => i.Overloads).Cast<VirtualFunction>())
			{
				if (virtual_function.ReturnType != null && !virtual_function.ReturnType.IsUnresolved) continue;
				diagnostics.Add(new DocumentDiagnostic(virtual_function.Start, "Can not resolve virtual function return type", DocumentDiagnosticSeverity.ERROR));
			}

			// There must be at least one destructor which requires no parameters
			if (type.IsUserDefined && type.Destructors.Overloads.All(i => i.Parameters.Count > 0))
			{
				diagnostics.Add(new DocumentDiagnostic(type.Position, $"Type '{type}' does not have a default destructor", DocumentDiagnosticSeverity.ERROR));
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
				diagnostics.Add(new DocumentDiagnostic(parameter.Position, $"Can not resolve the type of the parameter '{parameter.Name}'", DocumentDiagnosticSeverity.ERROR));
			}
		}

		foreach (var implementation in Common.GetLocalFunctionImplementations(context))
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
		var extensions = root.FindAll(NodeType.EXTENSION_FUNCTION).Cast<ExtensionFunctionNode>();

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
		var errors = new List<Status>();

		// Report if the return type is not resolved
		if (implementation.ReturnType == null || implementation.ReturnType.IsUnresolved)
		{
			errors.Add(Status.Error(implementation.Metadata.Start, "Can not resolve the return type"));
		}

		// Look for errors under the implementation node
		if (!Equals(implementation.Node, null))
		{
			errors.AddRange(implementation.Node.FindAll(i => i is IResolvable)
				.Cast<IResolvable>().Select(i => i.GetStatus()).Where(i => i.IsProblematic).ToList()
			);

			// Ensure assignments are used properly
			var nodes = implementation.Node.FindAll(i => i.Is(OperatorType.ASSIGNMENT));

			foreach (var iterator in nodes)
			{
				if (iterator.Left.Is(NodeType.VARIABLE, NodeType.LINK, NodeType.ACCESSOR)) continue;
				errors.Add(Status.Error(iterator.Position, "Can not understand the assignment"));
			}

			// Ensure all the assignments are exposed to their parent scopes
			foreach (var iterator in nodes)
			{
				if (iterator.Parent == null || iterator.Parent.Is(NodeType.SCOPE, NodeType.NORMAL, NodeType.INLINE)) continue;
				errors.Add(Status.Error(iterator.Position, "Assignment must be exposed"));
			}

			// Ensure increments and decrements are used properly
			nodes = implementation.Node.FindAll(NodeType.INCREMENT, NodeType.DECREMENT);

			foreach (var iterator in nodes)
			{
				var source = Analyzer.GetSource(iterator);
				if (source.Is(NodeType.CALL, NodeType.CONSTRUCTION, NodeType.DATA_POINTER, NodeType.DECREMENT, NodeType.FUNCTION, NodeType.INCREMENT, NodeType.LINK, NodeType.NEGATE, NodeType.NOT, NodeType.ACCESSOR, NodeType.STACK_ADDRESS, NodeType.VARIABLE)) continue;

				var name = iterator.Is(NodeType.INCREMENT) ? "increment" : "decrement";
				errors.Add(Status.Error(iterator.Position, $"Can not understand the {name}"));
			}
		}

		// Look for variables which are not resolved
		errors.AddRange(implementation.Locals.Where(i => i.IsUnresolved).Select(i => Status.Error(i.Position, $"Can not resolve type of local variable '{i.Name}'")));

		if (errors.Count == 0) return string.Empty;

		return $"Function {implementation.GetHeader()}:\n" + string.Join('\n', errors.Select(i => i.Description));
	}

	/// <summary>
	/// Returns a string which describes the state of the specified function
	/// </summary>
	public static string GetFunctionReport(Function function)
	{
		var errors = new List<Status>();
		if (function.IsTemplateFunction) return string.Empty;

		foreach (var parameter in function.Parameters)
		{
			// Explicit parameter types are optional, but they must be resolved if specified
			if (parameter.Type == null || parameter.Type.IsResolved()) continue;
			errors.Add(Status.Error(parameter.Position, "Can not resolve the type of the parameter " + parameter.Name));
		}

		// Explicit return types are optional, but they must be resolved if specified
		if (function.ReturnType != null)
		{
			if (function.ReturnType.IsUnresolved)
			{
				errors.Add(Status.Error(function.Start, "Can not resolve the return type"));
			}
			else if (function.ReturnType is ArrayType)
			{
				errors.Add(Status.Error(function.Start, "Array type is not allowed as a return type"));
			}
		}

		if (errors.Count == 0) return string.Empty;

		return $"Function {function}:\n" + string.Join('\n', errors.Select(i => i.Description));
	}

	/// <summary>
	/// Returns a string which describes the state of the specified context
	/// </summary>
	public static string GetReport(Context context)
	{
		var variables = new StringBuilder();

		// Go through all the variables which are unresolved
		foreach (var variable in context.Variables.Values.Where(variable => variable.IsUnresolved))
		{
			variables.Append(Errors.Format(variable.Position, $"Can not resolve the type of variable '{variable}'"));
			variables.AppendLine();
		}

		var imports = new StringBuilder();

		// Report unresolved imports
		foreach (var imported in context.Imports)
		{
			if (imported.IsResolved()) continue;
			imports.AppendLine(Errors.Format(imported.Position, "Can not resolve the import"));
		}

		// Report errors in defined types
		var types = new StringBuilder();

		foreach (var type in context.Types.Values)
		{
			// TODO: Support pack supertypes
			if (type.IsPack && type.Supertypes.Any())
			{
				types.AppendLine("Packs do not support supertypes currently");
			}

			foreach (var diagnostic in FindUnconstructedSupertypes(type).Select(i => Status.Error(new Position(type.Position?.File, i.Range.Start.Line, i.Range.Start.Character), i.Message)))
			{
				types.AppendLine(diagnostic.Description);
			}

			foreach (var supertype in type.Supertypes.Where(i => i.IsUnresolved))
			{
				types.AppendLine(Errors.Format(type.Position, $"Type '{type}' can not inherit type '{supertype}' since either it was not found or it would have caused a cyclic inheritance"));
			}

			foreach (var virtual_function in type.Virtuals.Values.SelectMany(i => i.Overloads).Cast<VirtualFunction>())
			{
				if (virtual_function.ReturnType != null && !virtual_function.ReturnType.IsUnresolved) continue;
				types.AppendLine(Errors.Format(virtual_function.Start, "Can not resolve virtual function return type"));
			}

			// Look for override functions, which do not override anything
			foreach (var override_function in type.Overrides.Values.SelectMany(i => i.Overloads))
			{
				if (override_function.Implementations.Count > 0) continue;
				types.AppendLine(Errors.Format(override_function.Start, "Override function has no matching virtual function"));
			}

			if (type.IsUserDefined && type.Destructors.Overloads.All(i => i.Parameters.Count > 0))
			{
				types.AppendLine(Errors.Format(type.Position, $"Type '{type}' does not have a default destructor"));
			}

			var report = GetReport(type);

			if (string.IsNullOrEmpty(report)) continue;

			types.Append(report);
			types.AppendLine();
		}

		// Report errors in defined functions
		var functions = new StringBuilder();

		foreach (var overload in context.Functions.Values.SelectMany(i => i.Overloads))
		{
			if (overload.Parameters.All(i => i.Type == null || !i.Type.IsUnresolved)) continue;

			// Template functions can have unresolved parameter types, since the parameter types can require template arguments, but only the variants matter
			if (overload.IsTemplateFunction) continue;

			if (functions.Length > 0)
			{
				functions.AppendLine();
			}

			functions.AppendLine($"Function {overload}:");

			foreach (var parameter in overload.Parameters)
			{
				functions.AppendLine(Errors.Format(parameter.Position, $"Can not resolve the type of the parameter '{parameter.Name}'"));
			}
		}

		// Report errors in function headers
		foreach (var function in Common.GetAllVisibleFunctions(context))
		{
			functions.Append(GetFunctionReport(function));
		}

		foreach (var implementation in Common.GetLocalFunctionImplementations(context))
		{
			var report = GetFunctionReport(implementation);
			var subreport = GetReport(implementation);

			if (!string.IsNullOrEmpty(report)) { report += "\n" + subreport; }
			else { report = subreport; }

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
		if (variables.Length > 0) builder.AppendLine();

		builder.Append(imports);
		if (imports.Length > 0) builder.AppendLine();

		builder.Append(types);
		if (types.Length > 0) builder.AppendLine();

		builder.Append(functions);

		return builder.ToString();
	}

	/// <summary>
	/// Returns a string which describes the state of the specified context
	/// </summary>
	public static string GetReport(Context context, Node root)
	{
		var report = GetReport(context);
		var extensions = root.FindAll(NodeType.EXTENSION_FUNCTION).Cast<ExtensionFunctionNode>();

		if (!extensions.Any()) return report;

		var builder = new StringBuilder();

		foreach (var extension in extensions)
		{
			builder.AppendLine(extension.GetStatus().Description);
		}

		builder.AppendLine();

		return builder.ToString() + report;
	}

	/// <summary>
	/// Finds all static member assignments which should be executed before the entry function
	/// </summary>
	private static List<Node> CollectStaticInitializers(Context context)
	{
		var types = Common.GetAllTypes(context);
		var initializers = types.SelectMany(i => i.Initialization).Select(i => i.Clone()).ToList();

		for (var i = initializers.Count - 1; i >= 0; i--)
		{
			// Look for static member assignments
			var initializer = initializers[i];
			var edited = Analyzer.GetEdited(initializer);

			// Ensure the edited node is a variable node
			if (edited.Instance == NodeType.VARIABLE)
			{
				var member = edited.To<VariableNode>().Variable;

				// Ensure the member variable is static
				if (member.IsStatic)
				{
					edited.Replace(new LinkNode(
						new TypeNode(member.Parent.To<Type>(), member.Position),
						edited.Clone(),
						member.Position
					));

					continue;
				}
			}

			// Remove the initializer, since it is not a static member assignment
			initializers.RemoveAt(i);
		}

		return initializers;
	}

	/// <summary>
	/// Finds the implementations of the allocation and the inheritance functions and registers them to be used
	/// </summary>
	public static void RegisterDefaultFunctions(Context context)
	{
		var allocation_function = context.GetFunction("allocate") ?? throw new ApplicationException("Missing the allocation function, please implement it or include the standard library");
		var deallocation_function = context.GetFunction("deallocate") ?? throw new ApplicationException("Missing the deallocation function, please implement it or include the standard library");
		var inheritance_function = context.GetFunction("internal_is") ?? throw new ApplicationException("Missing the inheritance function, please implement it or include the standard library");
		var initialization_function = context.GetFunction("internal_init");

		var integer_parameter_type = Primitives.CreateNumber(Primitives.LARGE, Format.INT64);

		Settings.AllocationFunction = allocation_function.GetImplementation(integer_parameter_type);

		Settings.DeallocationFunction = deallocation_function.GetImplementation(new Link());

		Settings.InheritanceFunction = inheritance_function.GetImplementation(new Link(), new Link());

		// Find all the static member assignments and add them to the application initialization function
		var static_initializers = CollectStaticInitializers(context);

		if (initialization_function != null)
		{
			Settings.InitializationFunction = initialization_function.GetImplementation(new Link()) ?? initialization_function.GetImplementation();
		}
		else if (static_initializers.Any())
		{
			// Application initialization function calls the entry function: init()
			var initialization_function_blueprint = new List<Token>()
			{
				new FunctionToken(new IdentifierToken(Keywords.INIT.Identifier), new ParenthesisToken())
			};

			// Create an application initialization function, which calls the entry function, so that the static member assignments can be executed
			var initialization_function_metadata = new Function(context, Modifier.EXPORTED, "internal_init", initialization_function_blueprint, Settings.AllocationFunction!.Metadata.Start, null);
			context.Declare(initialization_function_metadata);
	
			Settings.InitializationFunction = initialization_function_metadata.Get(Array.Empty<Type>());
		}

		if (static_initializers.Any())
		{
			if (Settings.InitializationFunction == null) throw new ApplicationException("Missing the application initialization function");

			// Add the static initializers to the application initialization function
			for (var i = static_initializers.Count - 1; i >= 0; i--)
			{
				var initializer = static_initializers[i];
				var initializer_destination = Settings.InitializationFunction!.Node!.First;
				Settings.InitializationFunction.Node!.Insert(initializer_destination, initializer);
			}
		}
	}

	public override Status Execute()
	{
		if (Settings.Parse == null) return Status.Error("Nothing to resolve");

		var parse = Settings.Parse;

		var context = parse.Context;
		var report = GetReport(context, parse.Node);
		var evaluated = false;

		// Find the required functions
		RegisterDefaultFunctions(context);

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = report;

			ParserPhase.ApplyExtensionFunctions(context, parse.Node);
			ParserPhase.ImplementFunctions(context, null);
			GarbageCollector.CreateAllRequiredOverloads(context);
			
			// Try to resolve problems in the node tree and get the status after that
			Resolver.ResolveContext(context);
			Resolver.Resolve(context, parse.Node);

			report = GetReport(context, parse.Node);

			if (Settings.IsVerboseOutputEnabled)
			{
				Console.WriteLine("Resolving " + report.Length + " issues...");
			}

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

		if (Settings.IsVerboseOutputEnabled)
		{
			Console.WriteLine("Resolved");
		}

		// Finds objects whose values should be evaluated or finalized
		Analysis.Complete(context);

		// Align variables in memory
		Aligner.Align(context);

		// Analyze the output
		Analyzer.Analyze(parse.Node, context);

		// Report warnings at this point, because variables usages are now updated and we have the most information here before reconstruction
		var warnings = Warnings.Analyze(context);

		foreach (var warning in warnings)
		{
			Console.WriteLine(warning.Description);
		}

		// Apply analysis to the functions
		Analysis.Analyze(context);

		// Reanalyze the output after the changes
		Analyzer.Analyze(parse.Node, context);

		return Status.OK;
	}
}
