using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Net;
using System;

public enum DocumentRequestType
{
	COMPLETIONS = 1,
	SIGNATURES = 2,
	DIAGNOSE = 3,
	OPEN = 4,
	DEFINITION = 5,
	INFORMATION = 6,
	FIND_REFERENCES = 7,
	WORKSPACE_SYMBOLS = 8,
}

public class DocumentRequest
{
	public DocumentRequestType Type { get; set; }
	public Uri Uri { get; set; }
	public string Document { get; set; }
	public DocumentPosition Position { get; set; }
	public string? Query { get; set; }
	public int Absolute { get; set; } = -1;

	public DocumentRequest(string document, Uri uri, DocumentPosition position)
	{
		Document = document;
		Uri = uri;
		Position = position;
	}
}

public class DocumentToken
{
	public List<Token> Container { get; }
	public int Index { get; }
	public Token Token { get; }

	public DocumentToken(List<Token> container, int index, Token token)
	{
		Container = container;
		Index = index;
		Token = token;
	}
}

public static class DocumentResponseStatus
{
	public const int OK = 0;
	public const int INVALID_REQUEST = 1;
	public const int ERROR = 2;
}

public class FileDivider
{
	public Uri File { get; set; }
	public string Data { get; set; }

	public FileDivider(Uri file, string data)
	{
		File = file;
		Data = data;
	}
}

public class DocumentAnalysisResponse
{
	public int Status { get; set; }
	public string Path { get; set; }
	public string Data { get; set; }

	public DocumentAnalysisResponse(int status, Uri uri, string data)
	{
		Status = status;
		Path = ServiceUtility.ToPath(uri);
		Data = data;
	}

	public DocumentAnalysisResponse(int status, string path, string data)
	{
		Status = status;
		Path = path;
		Data = data;
	}
}

public class DocumentParse
{
	public List<Token> Tokens { get; set; } = new List<Token>();
	public Context? Context { get; set; }
	public Node? Root { get; set; }
	public Dictionary<Function, List<Token>> Blueprints { get; } = new Dictionary<Function, List<Token>>();
	public List<Function> Functions => Blueprints.Keys.ToList();
	public ContextRecovery? Recovery { get; set; }
	public string Document { get; set; } = string.Empty;

	public DocumentParse() {}

	public DocumentParse(List<Token> tokens)
	{
		Tokens = tokens;
	}
}

public class ServicePhase : Phase
{
	private const bool DEBUG = false;

	public override Status Execute()
	{
		if (!Settings.Service) return Status.OK;

		var detail_provider_socket = new TcpListener(IPAddress.Loopback, DEBUG ? DetailProvider.DebugPort : 0);
		var diagnostics_provider_socket = new TcpListener(IPAddress.Loopback, DEBUG ? DiagnosticsProvider.DebugPort : 0);

		var is_detail_provider_ready = new bool[1];
		var is_diagnostics_provider_ready = new bool[1];
		var detail_provider = Task.Run(() => ServiceNetworkListener.Listen(detail_provider_socket, is_detail_provider_ready, DetailProvider.Provide, DetailProvider.Reset));
		var diagnostics_provider = Task.Run(() => ServiceNetworkListener.Listen(diagnostics_provider_socket, is_diagnostics_provider_ready, DiagnosticsProvider.Provide, DiagnosticsProvider.Reset));

		// Wait for the providers to become ready
		while (!is_detail_provider_ready[0] || !is_diagnostics_provider_ready[0]) {}

		// Now, inform tell the client the ports of the providers after a quick delay
		Thread.Sleep(500);

		var detail_provider_end_point = (detail_provider_socket.LocalEndpoint as IPEndPoint) ?? throw new ApplicationException("Failed to determine socket port");
		var diagnostics_provider_end_point = (diagnostics_provider_socket.LocalEndpoint as IPEndPoint) ?? throw new ApplicationException("Failed to determine socket port");

		Console.WriteLine($"Ports: {detail_provider_end_point.Port}, {diagnostics_provider_end_point.Port}");
		Task.WaitAll(diagnostics_provider, detail_provider);
		return Status.OK;
	}
}