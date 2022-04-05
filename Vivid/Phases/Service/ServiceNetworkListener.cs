using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System;

public class ServiceRequestInformation
{
	public int Id { get; set; }
	public byte[]? Bytes { get; set; }
}

public static class ServiceNetworkListener
{
	/// <summary>
	/// Receives the next message from the specified socket
	/// </summary>
	private static ServiceRequestInformation Receive(Socket socket)
	{
		// Receive the message header
		while (socket.Available < sizeof(int) * 2) {}

		// Extract the size of message
		var buffer = new byte[sizeof(int) * 2];
		socket.Receive(buffer);

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

	public static void Listen(TcpListener listener, Action<IServiceResponse, DocumentRequest> action)
	{
		listener.Start();

		while (true)
		{
			var socket = (TcpClient?)null;

			try
			{
				socket = listener.AcceptTcpClient();
				socket.ReceiveBufferSize = 10000000;
			}
			catch
			{
				continue;
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
						action(response, request);
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
		}
	}
}