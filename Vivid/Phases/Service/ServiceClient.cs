using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System.Linq;
using System;

public interface IServiceResponse
{
	void SendStatusCode(Uri uri, int status, string message);
	void SendStatusCode(Uri uri, int status);
	void SendResponse<T>(Uri uri, int status, T response);
	void SendResponse<T>(string path, int status, T response);
	void SendResponse<T>(Uri uri, int status, T[] response);
}

public interface IServiceClient
{
	void SendStatusCode(int id, string path, int code, string message);
	void SendStatusCode(int id, Uri uri, int status, string message);
	void SendStatusCode(int id, Uri uri, int status);
	void SendResponse<T>(int id, Uri uri, int status, T response);
	void SendResponse<T>(int id, string path, int status, T response);
	void SendResponse<T>(int id, Uri uri, int status, T[] response);
}

public class ServiceResponse : IServiceResponse
{
	public IServiceClient Client { get; }
	public int Id { get; }

	public ServiceResponse(IServiceClient client, int id)
	{
		Client = client;
		Id = id;
	}

	public void SendResponse<T>(Uri uri, int status, T response)
	{
		Client.SendResponse<T>(Id, uri, status, response);
	}

	public void SendResponse<T>(string path, int status, T response)
	{
		Client.SendResponse<T>(Id, path, status, response);
	}

	public void SendStatusCode(string path, int status, string response)
	{
		Client.SendStatusCode(Id, path, status, response);
	}

	public void SendResponse<T>(Uri uri, int status, T[] response)
	{
		Client.SendResponse<T>(Id, uri, status, response);
	}

	public void SendStatusCode(Uri uri, int status, string message)
	{
		Client.SendStatusCode(Id, uri, status, message);
	}

	public void SendStatusCode(Uri uri, int status)
	{
		Client.SendStatusCode(Id, uri, status);
	}
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

	public void SendStatusCode(int id, Uri uri, int code, string message)
	{
		var serialized = JsonSerializer.Serialize(new DocumentAnalysisResponse(code, uri, message), typeof(DocumentAnalysisResponse));
		var payload = Encoding.UTF8.GetBytes(serialized);

		var header = BitConverter.GetBytes(payload.Length).Concat(BitConverter.GetBytes(id));

		Socket.Send(header.Concat(payload).ToArray());
	}

	public void SendStatusCode(int id, string path, int code, string message)
	{
		var serialized = JsonSerializer.Serialize(new DocumentAnalysisResponse(code, path, message), typeof(DocumentAnalysisResponse));
		var payload = Encoding.UTF8.GetBytes(serialized);

		var header = BitConverter.GetBytes(payload.Length).Concat(BitConverter.GetBytes(id));

		Socket.Send(header.Concat(payload).ToArray());
	}

	public void SendStatusCode(int id, Uri uri, int code)
	{
		SendStatusCode(id, uri, code, string.Empty);
	}

	/// <summary>
	/// Sends the specified response to the specified receiver by serializing the response to JSON-format
	/// </summary>
	public void SendResponse(int id, DocumentAnalysisResponse response)
	{
		var serialized = JsonSerializer.Serialize(response, typeof(DocumentAnalysisResponse));
		var payload = Encoding.UTF8.GetBytes(serialized);

		var header = BitConverter.GetBytes(payload.Length).Concat(BitConverter.GetBytes(id));

		Socket.Send(header.Concat(payload).ToArray());
	}

	public void SendResponse<T>(int id, Uri uri, int status, T response)
	{
		SendResponse(id, new DocumentAnalysisResponse(DocumentResponseStatus.OK, uri, JsonSerializer.Serialize(response)));
	}

	public void SendResponse<T>(int id, string path, int status, T response)
	{
		SendResponse(id, new DocumentAnalysisResponse(DocumentResponseStatus.OK, path, JsonSerializer.Serialize(response)));
	}

	public void SendResponse<T>(int id, Uri uri, int status, T[] response)
	{
		SendResponse(id, new DocumentAnalysisResponse(DocumentResponseStatus.OK, uri, JsonSerializer.Serialize(response)));
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