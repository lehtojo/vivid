using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System;

public static class ServiceNetworkListener
{
	/// <summary>
	/// Receives the next message from the specified socket
	/// </summary>
	private static byte[]? Receive(Socket socket)
	{
		// Receive the message header
		while (socket.Available < sizeof(int)) {}

		// Extract the size of message
		var buffer = new byte[sizeof(int)];
		socket.Receive(buffer);
		var size = BitConverter.ToUInt32(buffer);

		// If the size of the message exceeds the size of the receive buffer, return a null array
		if (size > (uint)socket.ReceiveBufferSize) return null;

		// Wait for the message to arrive fully
		while ((uint)socket.Available < size) {}

		// Receive the buffer
		buffer = new byte[size];
		socket.Receive(buffer);
		return buffer;
	}

	public static void Listen(TcpListener listener, Action<ServiceNetworkClient, DocumentRequest> action)
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
					var bytes = Receive(socket.Client);

					if (bytes == null)
					{
						client.SendStatusCode(string.Empty, DocumentResponseStatus.INVALID_REQUEST, "Could not understand the request");
						continue;
					}

					// Deserialize the message
					var data = Encoding.UTF8.GetString(bytes);
					var request = (DocumentRequest?)JsonSerializer.Deserialize(data, typeof(DocumentRequest));

					// If the request can not be deserialized, send back a response which states the request is invalid
					if (request == null)
					{
						client.SendStatusCode(new Uri(string.Empty), DocumentResponseStatus.INVALID_REQUEST, "Could not understand the request");
						continue;
					}

					try
					{
						// Process the received message
						action(client, request);
					}
					catch
					{
						client.SendStatusCode(request.Uri, DocumentResponseStatus.ERROR);
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