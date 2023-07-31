using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Chess.Testing.Versus
{
	public class TcpPlayer : MonoBehaviour
	{

		TcpClient client;
		MessageListener listener;

		protected virtual void Update()
		{
			if (client != null && listener == null)
			{
				string listenerName = "Player listening for server";
				listener = MessageListener.CreateInstance(client, listenerName);
				listener.onMessageReceived += MessageReceived;
				OnConnected();
			}
		}

		protected virtual void OnConnected()
		{

		}

		protected void SendMessageToServer(string message)
		{
			byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
			client.GetStream().Write(data, 0, data.Length);
		}

		protected virtual void MessageReceived(TcpClient client, string message)
		{

		}

		public void CreatePlayer()
		{
			new Task(TryConnectToServer).Start();
		}

		async void TryConnectToServer()
		{
			while (client == null)
			{
				try
				{
					client = new TcpClient(TCPServer.localhost, TCPServer.portNumber);
				}
				catch
				{
					await Task.Delay(10);
				}
			}
		}

		void OnDestroy()
		{
			client?.GetStream().Close();
			client?.Close();
		}

	}
}