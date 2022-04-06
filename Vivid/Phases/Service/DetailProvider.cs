using System;

public static class DetailProvider
{
	public static Project Project { get; set; } = new();

	public static void ProcessOpenRequest(IServiceResponse response, DocumentRequest request)
	{
		var start = DateTime.Now;

		var folder = ServiceUtility.ToPath(request.Uri);
		Console.WriteLine($"Opening project folder '{folder}'");

		ProjectLoader.OpenProject(Project.Documents, folder);
		response.SendStatusCode(request.Uri, DocumentResponseStatus.OK);

		Console.WriteLine($"Opening took {(DateTime.Now - start).TotalMilliseconds} ms");
	}

	public static void Provide(IServiceResponse response, DocumentRequest request)
	{
		var absolute = ServiceUtility.ToAbsolutePosition(request.Document, request.Position.Line, request.Position.Character);
		request.Absolute = absolute ?? -1;

		// Preprocess the document
		request.Document = request.Document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

		if (request.Type == DocumentRequestType.OPEN)
		{
			ProcessOpenRequest(response, request);
		}
		else if (request.Type == DocumentRequestType.COMPLETIONS)
		{
			CompletionProvider.Provide(Project, response, request);
		}
		else if (request.Type == DocumentRequestType.SIGNATURES)
		{
			FunctionSignatureProvider.Provide(Project, response, request);
		}
		else if (request.Type == DocumentRequestType.DEFINITION)
		{
			DefinitionProvider.Provide(Project, response, request);
		}
		else if (request.Type == DocumentRequestType.INFORMATION)
		{
			HoverProvider.Provide(Project, response, request);
		}
		// else if (request.Type == DocumentRequestType.FIND_REFERENCES)
		// {
		// 	ReferenceProvider.Provide(Files, response, request);
		// }
		else if (request.Type == DocumentRequestType.WORKSPACE_SYMBOLS)
		{
			WorkspaceSymbolProvider.Provide(Project, response, request);
		}
		else
		{
			response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
		}
	}

	public static void Reset()
	{
		Project = new();
	}
}