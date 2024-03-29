using System;

public static class DetailProvider
{
	public const int DebugPort = 1111;

	public static Project Project { get; set; } = new();

	public static void ProcessOpenRequest(IServiceResponse response, DocumentRequest request)
	{
		var start = DateTime.Now;

		var folder = ServiceUtility.ToPath(request.Uri);
		Console.WriteLine($"Opening project folder '{folder}'");

		ProjectLoader.OpenProject(Project, folder);
		response.SendStatusCode(request.Uri, DocumentResponseStatus.OK);

		Console.WriteLine($"Opening took {(DateTime.Now - start).TotalMilliseconds} ms");
	}

	public static void Provide(IServiceResponse response, DocumentRequest request)
	{
		var absolute = ServiceUtility.ToAbsolutePosition(request.Document, request.Position.Line, request.Position.Character);
		request.Absolute = absolute ?? -1;

		// Preprocess the document
		request.Document = request.Document.Replace('\r', ' ').Replace('\t', ' ');

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
		else if (request.Type == DocumentRequestType.FIND_REFERENCES)
		{
			ReferenceProvider.Provide(Project, response, request);
		}
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