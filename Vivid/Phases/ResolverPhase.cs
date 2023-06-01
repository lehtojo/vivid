using System;
using System.Collections.Generic;
using System.Linq;

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
	/// Returns list of diagnostics which describes the state of the specified context
	/// </summary>
	public static List<DocumentDiagnostic> GetDiagnostics(Context context, Node root)
	{
		var errors = Resolver.GetReport(context, root);

		return errors.Select(i => new DocumentDiagnostic(i.Position, i.Message, DocumentDiagnosticSeverity.ERROR)).ToList();
	}

	public override Status Execute()
	{
		if (Settings.Parse == null) return new Status("Nothing to resolve");

		var parse = Settings.Parse;

		var context = parse.Context;
		var current = Resolver.GetReport(context, parse.Node);
		var evaluated = false;

		// Find the required functions
		Resolver.RegisterDefaultFunctions(context);

		// Try to resolve as long as errors change -- errors do not always decrease since the program may expand each cycle
		while (true)
		{
			var previous = current;

			ParserPhase.ImplementFunctions(context, null);
			GarbageCollector.CreateAllRequiredOverloads(context);
			
			// Try to resolve problems in the node tree and get the status after that
			Resolver.ResolveContext(context);
			Resolver.Resolve(context, parse.Node);

			current = Resolver.GetReport(context, parse.Node);

			if (Settings.IsVerboseOutputEnabled)
			{
				Console.WriteLine("Resolving " + current.Count + " issues...");
			}

			// Try again only if the errors have changed
			if (!current.SequenceEqual(previous)) continue;
			if (evaluated) break;

			Evaluator.Evaluate(context);
			evaluated = true;
		}

		// The compiler must not continue if the resolver phase has failed
		if (current.Count > 0)
		{
			Common.Report(current);
			return new Status("Compilation error");
		}

		if (Settings.IsVerboseOutputEnabled) Console.WriteLine("Resolved");

		// Finds objects whose values should be evaluated or finalized
		Analysis.Complete(context);

		// Align variables in memory
		Aligner.Align(context);

		// Analyze the output
		Analyzer.Analyze(parse.Node, context);

		// Report warnings at this point, because variables usages are now updated and we have the most information here before reconstruction
		Warnings.Report(context);

		// Apply analysis to the functions
		Analysis.Analyze(context);

		// Reanalyze the output after the changes
		Analyzer.Analyze(parse.Node, context);

		return Status.OK;
	}
}
