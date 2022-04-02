using System;

public static class DetailProvider
{
	public static Project Project { get; set; } = new();

	public static void ProcessOpenRequest(IServiceClient client, DocumentRequest request)
	{
		var start = DateTime.Now;

		var folder = ServiceUtility.ToPath(request.Uri);
		Console.WriteLine($"Opening project folder '{folder}'");

		ProjectLoader.OpenProject(Project.Documents, folder);
		client.SendStatusCode(request.Uri, DocumentResponseStatus.OK);

		Console.WriteLine($"Opening took {(DateTime.Now - start).TotalMilliseconds} ms");
	}

	public static void Provide(ServiceNetworkClient client, DocumentRequest request)
	{
		var absolute = ServiceUtility.ToAbsolutePosition(request.Document, request.Position.Line, request.Position.Character);
		request.Absolute = absolute ?? -1;

		// Preprocess the document
		request.Document = request.Document.Replace(FilePhase.CARRIAGE_RETURN_CHARACTER, ' ').Replace(FilePhase.TAB_CHARACTER, ' ');

		if (request.Type == DocumentRequestType.OPEN)
		{
			ProcessOpenRequest(client, request);
		}
		else if (request.Type == DocumentRequestType.COMPLETIONS)
		{
			CompletionProvider.Provide(Project, client, request);
		}
		else if (request.Type == DocumentRequestType.SIGNATURES)
		{
			FunctionSignatureProvider.Provide(Project, client, request);
		}
		else if (request.Type == DocumentRequestType.DEFINITION)
		{
			DefinitionProvider.Provide(Project, client, request);
		}
		else if (request.Type == DocumentRequestType.INFORMATION)
		{
			HoverProvider.Provide(Project, client, request);
		}
		// else if (request.Type == DocumentRequestType.FIND_REFERENCES)
		// {
		// 	ReferenceProvider.Provide(Files, client, request);
		// }
		else if (request.Type == DocumentRequestType.WORKSPACE_SYMBOLS)
		{
			WorkspaceSymbolProvider.Provide(Project, client, request);
		}
		else
		{
			client.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
		}
	}
}