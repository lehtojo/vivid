using System.Collections.Generic;

public static class HoverProvider
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

		var information = string.Empty;

		// The cursor must be a function or a variable, so that its definition can be found
		if (cursor.Is(NodeType.FUNCTION))
		{
			information = $"(function) {cursor.To<FunctionNode>().Function.ToString()}";
		}
		else if (cursor.Is(NodeType.VARIABLE))
		{
			var variable = cursor.To<VariableNode>().Variable;
			var header = cursor.To<VariableNode>().Variable.ToString();

			if (variable.IsConstant) { information = $"(constant) {header}"; }
			else if (variable.IsMember) { information = $"(member) {header}"; }
			else { information = $"(variable) {header}"; }
		}
		else if (cursor.Is(NodeType.TYPE))
		{
			var type = cursor.To<TypeNode>().Type;
			var header = cursor.To<TypeNode>().Type.ToString();

			if (type.IsNamespace) { information = $"(namespace) {header}"; }
			else { information = $"(type) {header}"; }
		}
		else if (cursor.Is(NodeType.CONSTRUCTION))
		{
			information = $"(constructor) {cursor.To<ConstructionNode>().Constructor.Function.Metadata.ToString()}";
		}

		if (string.IsNullOrEmpty(information))
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
			return;
		}

		response.SendResponse(request.Uri, DocumentResponseStatus.OK, information);
	}
}