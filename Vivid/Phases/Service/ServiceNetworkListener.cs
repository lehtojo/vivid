using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System;

public class ServiceRequestInformation
{
	public int Id { get; set; }
	public byte[]? Bytes { get; set; }
}

public static class SocketExtensions
{
	public static bool IsConnected(this Socket socket)
	{
		try
		{
			return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
		}
		catch (SocketException) { return false; }
	}
}

public static class ServiceNetworkListener
{
	/// <summary>
	/// Receives the next message from the specified socket
	/// </summary>
	private static ServiceRequestInformation? Receive(Socket socket)
	{
		// Receive the message header that contains the size and id of the message
		var buffer = new byte[sizeof(int) * 2];
		if (socket.Receive(buffer, 0, buffer.Length, SocketFlags.None) != buffer.Length) return null;

		var size = BitConverter.ToUInt32(buffer);
		var id = BitConverter.ToInt32(buffer, sizeof(int));

		if (size > (uint)socket.ReceiveBufferSize) return new ServiceRequestInformation() { Id = id };

		// Wait for the message to arrive fully
		while ((uint)socket.Available < size) {}

		// Receive the buffer
		buffer = new byte[size];
		socket.Receive(buffer);

		return new ServiceRequestInformation() { Id = id, Bytes = buffer };
	}

	public static void Listen(TcpListener listener, bool[] listening, Action<IServiceResponse, DocumentRequest> receive, Action disconnect)
	{
		listener.Start();

		while (true)
		{
			var socket = (TcpClient?)null;

			try
			{
				// Tell we are ready
				listening[0] = true;

				// Wait for the next connection
				socket = listener.AcceptTcpClient();
				socket.ReceiveBufferSize = 10000000;

				Console.WriteLine("Connection established");
			}
			catch
			{
				continue; // Something went wrong, start over
			}

			var client = new ServiceNetworkClient(socket.Client, socket.GetStream());

			while (true)
			{
				try
				{
					// Stop if the socket has disconnected and start over
					if (!socket.Connected) break;

					// Receive the next message buffer
					var information = Receive(socket.Client);

					// If the returned information is null, the socket disconnected so stop waiting for other messages
					if (information == null) break;

					var response = new ServiceResponse(client, information.Id);

					if (information.Bytes == null)
					{
						response.SendStatusCode(string.Empty, DocumentResponseStatus.INVALID_REQUEST, "Could not understand the request");
						continue;
					}

					// Deserialize the message
					var data = Encoding.UTF8.GetString(information.Bytes);
					var request = (DocumentRequest?)JsonSerializer.Deserialize(data, typeof(DocumentRequest));

					// If the request can not be deserialized, send back a response which states the request is invalid
					if (request == null)
					{
						response.SendStatusCode(new Uri(string.Empty), DocumentResponseStatus.INVALID_REQUEST, "Could not understand the request");
						continue;
					}

					try
					{
						// Process the received message
						receive(response, request);
					}
					catch
					{
						response.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
					}
				}
				catch (Exception error)
				{
					Console.WriteLine("ERROR: Something went wrong while processing request: " + error.ToString());
				}
			}

			try
			{
				disconnect(); // Notify about the disconnection
			}
			catch (Exception e)
			{
				Console.WriteLine("ERROR: Something went wrong while processing disconnection: " + e.ToString());
			}

			Console.WriteLine("Waiting for the next connection...");
		}
	}
}