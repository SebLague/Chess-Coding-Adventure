using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Chess.Testing.Versus
{
	public abstract class TCPServer : MonoBehaviour
	{

		public const int portNumber = 1300;
		public const string localhost = "127.0.0.1";

		protected List<TcpClient> connectedClients;
		ConcurrentQueue<TcpClient> connectionsToProcess;
		TcpListener server;

		protected void StartServer()
		{
			connectedClients = new List<TcpClient>();
			connectionsToProcess = new ConcurrentQueue<TcpClient>();

			server = new TcpListener(System.Net.IPAddress.Parse(localhost), portNumber);
			server.Start();

			Task task = new Task(CheckForConnections);
			task.Start();
		}

		protected virtual void Update()
		{
			ProcessConnections();
		}

		protected void SendMessageToClient(TcpClient player, string message)
		{
			byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
			player.GetStream().Write(data, 0, data.Length);
		}

		protected abstract void MessageReceived(TcpClient client, string message);

		async void CheckForConnections()
		{
			while (true)
			{
				TcpClient client = await server.AcceptTcpClientAsync();
				connectionsToProcess.Enqueue(client);
			}
		}

		protected virtual void OnClientConnected(TcpClient client)
		{

		}

		void ProcessConnections()
		{
			if (connectionsToProcess == null) { return; }

			while (connectionsToProcess.Count > 0)
			{
				if (connectionsToProcess.TryDequeue(out TcpClient connectedClient))
				{
					string listenerName = "Server listening for client";
					MessageListener listener = MessageListener.CreateInstance(connectedClient, listenerName);
					listener.onMessageReceived += MessageReceived;
					connectedClients.Add(connectedClient);
					OnClientConnected(connectedClient);
				}
				else
				{
					break;
				}
			}
		}

		protected virtual void OnDestroy()
		{
			server?.Stop();
		}

	}
}