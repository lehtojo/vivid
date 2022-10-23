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
	public static Function? UpdateAndMark(Project project, DocumentRequest request)
	{
		var document = request.Document;
		var file = project.GetSourceFile(request.Uri);
		var parse = project.GetParse(file);

		var changed = project.Update(request.Uri, document);

		if (!MarkCursorToken(request, changed)) return null;

		// Find the function that contains the cursor
		return CursorInformationProvider.FindCursorFunction(parse);
	}

	/// <summary>
	/// Provides function signature information for the specified request.
	/// </summary>
	public static void Provide(Project project,IServiceResponse response, DocumentRequest request)
	{
		var filename = ServiceUtility.ToPath(request.Uri);
		var cursor_function = UpdateAndMark(project, request);

		// If no function contains the cursor, send an error
		if (cursor_function == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		// Finally, build the document
		var parse = ProjectBuilder.Build(project.Documents, filename, false, cursor_function);

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

		var locations = variable.Usages
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