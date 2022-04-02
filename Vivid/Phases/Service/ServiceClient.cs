using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System.Linq;
using System;

public interface IServiceClient
{
	void SendStatusCode(Uri uri, int status, string message);
	void SendStatusCode(Uri uri, int status);
	void SendResponse<T>(Uri uri, int status, T response);
	void SendResponse<T>(string path, int status, T response);
	void SendResponse<T>(Uri uri, int status, T[] response);
}

public class ServiceNetworkClient : IServiceClient
{
	public Socket Socket { get; }
	public NetworkStream Stream { get; }

	public ServiceNetworkClient(Socket socket, NetworkStream stream)
	{
		Socket = socket;
		Stream = stream;
	}

	public void SendStatusCode(Uri uri, int code, string message)
	{
		var payload = JsonSerializer.Serialize(new DocumentAnalysisResponse(code, uri, message), typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		Socket.Send(BitConverter.GetBytes((uint)bytes.Length).Concat(bytes).ToArray());
	}

	public void SendStatusCode(string path, int code, string message)
	{
		var payload = JsonSerializer.Serialize(new DocumentAnalysisResponse(code, path, message), typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		Socket.Send(BitConverter.GetBytes((uint)bytes.Length).Concat(bytes).ToArray());
	}

	public void SendStatusCode(Uri uri, int code)
	{
		SendStatusCode(uri, code, string.Empty);
	}

	/// <summary>
	/// Sends the specified response to the specified receiver by serializing the response to JSON-format
	/// </summary>
	public void SendResponse(DocumentAnalysisResponse response)
	{
		var payload = JsonSerializer.Serialize(response, typeof(DocumentAnalysisResponse));
		var bytes = Encoding.UTF8.GetBytes(payload);

		Socket.Send(BitConverter.GetBytes((uint)bytes.Length).Concat(bytes).ToArray());
	}

	public void SendResponse<T>(Uri uri, int status, T response)
	{
		SendResponse(new DocumentAnalysisResponse(DocumentResponseStatus.OK, uri, JsonSerializer.Serialize(response)));
	}

	public void SendResponse<T>(string path, int status, T response)
	{
		SendResponse(new DocumentAnalysisResponse(DocumentResponseStatus.OK, path, JsonSerializer.Serialize(response)));
	}

	public void SendResponse<T>(Uri uri, int status, T[] response)
	{
		SendResponse(new DocumentAnalysisResponse(DocumentResponseStatus.OK, uri, JsonSerializer.Serialize(response)));
	}

	public void Disconnect()
	{
		try
		{
			Socket.Disconnect(false);
		}
		catch {}
	}
}