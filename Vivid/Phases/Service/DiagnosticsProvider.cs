using System.Collections.Generic;
using System.Linq;
using System;

public static class DiagnosticsProvider
{
	public const int DebugPort = 2222;

	public static Project Project { get; set; } = new();

	/// <summary>
	/// Figures out the ranges of the specified diagnostics by examining the specified tokens
	/// </summary>
	public static void MapDiagnosticRanges(List<Token> tokens, List<DocumentDiagnostic> diagnostics)
	{
		foreach (var token in tokens)
		{
			for (var i = diagnostics.Count - 1; i >= 0; i--)
			{
				var diagnostic = diagnostics[i];

				// Ensure the diagnostic starts at the beginning of the token
				if (!diagnostic.Range.Start.Equals(token.Position)) continue;

				// Remove the diagnostic, because now we can map it
				diagnostics.RemoveAt(i);

				// Get the position end of the current
				var end = Common.GetEndOfToken(token);
				if (end == null) break;

				// Give the diagnostic the range and continue
				diagnostic.Range.End = new DocumentPosition(end.Line, end.Character);
				break;
			}

			if (token.Type == TokenType.PARENTHESIS) MapDiagnosticRanges(token.To<ParenthesisToken>().Tokens, diagnostics);
			if (token.Type == TokenType.FUNCTION) MapDiagnosticRanges(token.To<FunctionToken>().Parameters.Tokens, diagnostics);
		}
	}

	public static void ProcessOpenRequest(IServiceResponse response, DocumentRequest request)
	{
		var start = DateTime.Now;

		var folder = ServiceUtility.ToPath(request.Uri);
		Console.WriteLine($"Opening project folder '{folder}'");

		ProjectLoader.OpenProject(Project, folder);
		response.SendStatusCode(request.Uri, DocumentResponseStatus.OK);

		Console.WriteLine($"Opening took {(DateTime.Now - start).TotalMilliseconds} ms");
	}

	public static List<DocumentDiagnostic> FilterDiagnostics(List<DocumentDiagnostic> diagnostics, SourceFile filter)
	{
		return diagnostics
			.Where(i => i.File != null && i.File == filter)
			.Where(i => i.Range.Start.Line > 0).ToList();
	}

	public static void AttachBlueprints(DocumentParse parse)
	{
		foreach (var iterator in parse.Blueprints)
		{
			var function = iterator.Key;
			var blueprint = iterator.Value;

			function.Blueprint = blueprint.Select(i => (Token)i.Clone()).ToList();
		}
	}

	public static void ProcessDiagnosticsRequest(IServiceResponse response, DocumentRequest request)
	{
		var tokens = (List<Token>?)null;
		var path = ServiceUtility.ToPath(request.Uri);
		var file = Project.GetSourceFile(path);

		try
		{
			// Preprocess the document
			request.Document = request.Document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

			// Tokenize the document in case of some unexpected error
			tokens = Lexer.GetTokens(request.Document);
			Lexer.RegisterFile(tokens, file);

			// Update the project document
			_ = Project.Update(path, request.Document);

			AttachBlueprints(Project.GetParse(file));

			// Build the document
			var parse = ProjectBuilder.Build(Project.Documents, path, true, null);

			// Collect diagnostics and send them back
			var diagnostics = ResolverPhase.GetDiagnostics(parse.Context, parse.Node);
			diagnostics = FilterDiagnostics(diagnostics, file);

			MapDiagnosticRanges(parse.Tokens, new List<DocumentDiagnostic>(diagnostics));

			response.SendResponse(request.Uri, DocumentResponseStatus.OK, diagnostics);
		}
		catch (LexerException error)
		{
			var diagnostics = new List<DocumentDiagnostic> { new DocumentDiagnostic(error.Position, error.Description, DocumentDiagnosticSeverity.ERROR) };
			diagnostics = FilterDiagnostics(diagnostics, file);

			response.SendResponse(request.Uri, DocumentResponseStatus.OK, diagnostics);
		}
		catch (SourceException error)
		{
			var diagnostics = new List<DocumentDiagnostic> { new DocumentDiagnostic(error.Position, error.Message, DocumentDiagnosticSeverity.ERROR) };
			diagnostics = FilterDiagnostics(diagnostics, file);

			if (tokens != null)
			{
				MapDiagnosticRanges(tokens, new List<DocumentDiagnostic>(diagnostics));
			}

			response.SendResponse(request.Uri, DocumentResponseStatus.OK, diagnostics);
		}
		catch
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
		}
	}

	public static void Provide(IServiceResponse connection, DocumentRequest request)
	{
		try
		{
			if (request.Type == DocumentRequestType.OPEN)
			{
				ProcessOpenRequest(connection, request);
			}
			else if (request.Type == DocumentRequestType.DIAGNOSE)
			{
				ProcessDiagnosticsRequest(connection, request);
			}
			else
			{
				connection.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			}
		}
		catch
		{
			connection.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
		}
	}

	public static void Reset()
	{
		Project = new();
	}
}