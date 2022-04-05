using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

public static class ReferenceProvider
{
	/// <summary>
	/// Marks the tokens targeted by the cursor specified in the request.
	/// </summary>
	public static bool MarkCursorToken(DocumentRequest request, List<Token> tokens)
	{
		// Try to find the token which surrounds the cursor
		var cursor = CursorInformationProvider.FindUnmarkedCursorToken(tokens, request.Absolute);
		if (cursor == null) return false;

		// Mark the token as the cursor
		cursor.Position.IsCursor = true;
		return true;
	}

	/// <summary>
	/// Loads the specified files while taking into account the cursor specified in the request.
	/// Returns the function, which contains the cursor specified in the request.
	/// </summary>
	public static Function? Load(Dictionary<SourceFile, DocumentParse> files, DocumentRequest request)
	{
		var path = ServiceUtility.ToPath(request.Uri);
		var document = request.Document;

		// Tokenize the document
		var tokens = Lexer.GetTokens(document, false);
		var source = new List<Token>(tokens);

		// Join the tokens now, because the 'source' list now contains the most accurate version of the document
		Lexer.Join(tokens);

		if (!MarkCursorToken(request, tokens)) return null;

		// Find the source file which has the same filename as the specified filename
		var file = files.Keys.FirstOrDefault(i => i.Fullname == path);
		var index = file != null ? file.Index : files.Count;

		if (file == null)
		{
			file = new SourceFile(path, document, index);
		}

		files[file] = new DocumentParse(tokens);

		ProjectLoader.Parse(file, files[file]);
		ProjectLoader.ResolveOld(file, files[file]);

		// Find the function that contains the cursor
		var cursor_function = CursorInformationProvider.FindCursorFunction(files[file]);
		return cursor_function;
	}

	/// <summary>
	/// Provides function signature information for the specified request.
	/// </summary>
	public static void Provide(Dictionary<SourceFile, DocumentParse> files, IServiceResponse response, DocumentRequest request)
	{
		var filename = ServiceUtility.ToPath(request.Uri);
		var cursor_function = Load(files, request);

		// If no function contains the cursor, send an error
		if (cursor_function == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		// Finally, build the document
		var parse = ProjectBuilder.Build(files, filename, true, cursor_function);

		// Find the cursor
		var cursor = CursorInformationProvider.FindCursorNode(cursor_function);

		// If the cursor is not found, send an error
		if (cursor == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		if (!cursor.Is(NodeType.VARIABLE))
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		var variable = cursor.To<VariableNode>().Variable;
		Task.WaitAll(Analyzer.FindAllUsagesAsync(variable, parse.Node, parse.Context));

		var locations = variable.References
			.Select(i => i.Position)
			.Where(i => i != null && i.File != null)
			.GroupBy(i => i!.File)
			.Select(i => new FileDivider
			(
				ServiceUtility.ToUri(i.Key!.Fullname),
				JsonSerializer.Serialize(i.Select(i => new DocumentPosition(i!.Line, i!.Character)).ToArray())
			)).ToArray();

		response.SendResponse(request.Uri, DocumentResponseStatus.OK, locations);
	}
}