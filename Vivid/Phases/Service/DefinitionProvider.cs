using System.Collections.Generic;

public static class DefinitionProvider
{
	/// <summary>
	/// Marks the token targeted by the cursor in the specified tokens.
	/// </summary>
	public static bool MarkCursorToken(List<Token> tokens, int absolute)
	{
		// Try to find the token which surrounds the cursor
		var cursor = CursorInformationProvider.FindUnmarkedCursorToken(tokens, absolute);
		if (cursor == null) return false;

		// Mark the token as the cursor
		cursor.Position.IsCursor = true;
		return true;
	}

	/// <summary>
	/// Unmarks all cursor tokens in blueprints in the specified parse
	/// </summary>
	public static void UnmarkCursors(DocumentParse parse)
	{
		foreach (var blueprint in parse.Blueprints.Values)
		{
			CursorInformationProvider.UnmarkCursors(blueprint);
		}
	}

	/// <summary>
	/// Tries to find the function, which contains the cursor and marks it.
	/// Returns whether the cursor was found and marked.
	/// </summary>
	public static bool MarkCursorToken(DocumentParse parse, int absolute)
	{
		foreach (var blueprint in parse.Blueprints.Values)
		{
			// Try to find the token which surrounds the cursor
			var cursor = CursorInformationProvider.FindUnmarkedCursorToken(blueprint, absolute);
			if (cursor == null) continue;

			// Mark the token as the cursor
			cursor.Position.IsCursor = true;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Loads the specified files while taking into account the cursor specified in the request.
	/// Returns the function, which contains the cursor specified in the request.
	/// </summary>
	public static Function? UpdateAndMark(Project project, DocumentRequest request)
	{
		var document = request.Document;
		var line = request.Position.Line;
		var character = request.Position.Character;

		// Get the absolute position of the cursor in the document
		var absolute = ServiceUtility.ToAbsolutePosition(document, line, character);
		if (absolute == null) return null;

		var file = project.GetSourceFile(request.Uri);
		var parse = project.GetParse(file);

		// Unmark any previous cursors in function blueprints
		UnmarkCursors(parse);

		if (document != parse.Document)
		{
			// Update the document
			_ = project.Update(request.Uri, document);
		}

		// Since the document has remained the same, we can try to find and mark the cursor inside existing function blueprints
		if (!MarkCursorToken(parse, absolute.Value)) return null;

		return CursorInformationProvider.FindCursorFunction(parse);
	}

	/// <summary>
	/// Provides function signature information for the specified request.
	/// </summary>
	public static void Provide(Project project, IServiceResponse response, DocumentRequest request)
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

		var position = (Position?)null;
		var length = 0;

		// The cursor must be a function or a variable, so that its definition can be found
		if (cursor.Is(NodeType.FUNCTION))
		{
			position = cursor.To<FunctionNode>().Function.Metadata.Start;
			length = cursor.To<FunctionNode>().Function.Name.Length;
		}
		else if (cursor.Is(NodeType.VARIABLE))
		{
			position = cursor.To<VariableNode>().Variable.Position;
			length = cursor.To<VariableNode>().Variable.Name.Length;
		}
		else if (cursor.Is(NodeType.TYPE))
		{
			position = cursor.To<TypeNode>().Type.Position;
			length = cursor.To<TypeNode>().Type.Identifier.Length;
		}
		else if (cursor.Is(NodeType.CONSTRUCTION))
		{
			position = cursor.To<ConstructionNode>().Constructor.Function.Metadata.Start;
			length = cursor.To<ConstructionNode>().Constructor.Function.Name.Length;
		}

		if (position == null || position.File == null)
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		var start = new DocumentPosition(position.Line, position.Character);
		var end = new DocumentPosition(position.Line, position.Character + length);

		response.SendResponse(position.File.Fullname, DocumentResponseStatus.OK, new DocumentRange(start, end));
	}
}