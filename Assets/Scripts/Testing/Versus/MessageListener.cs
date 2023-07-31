using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

// Waits for data on NetworkStream on separate thread.
// Triggers onMessageReceived event on main thread when data has been read.
// Create an instance using MessageListener.CreateInstance(NetworkStream)
namespace Chess.Testing.Versus
{
	public class MessageListener : MonoBehaviour
	{

		const int refreshDelayMillis = 10;

		public event System.Action<TcpClient, string> onMessageReceived;

		TcpClient client;
		ConcurrentQueue<string> messageQueue;

		void Awake()
		{
			messageQueue = new ConcurrentQueue<string>();
		}

		// Process messages on the main thread
		void Update()
		{
			if (messageQueue.Count > 0)
			{
				string message;
				if (messageQueue.TryDequeue(out message))
				{
					onMessageReceived?.Invoke(client, message);
				}
			}
		}

		async void ListenForMessage()
		{
			NetworkStream stream = client.GetStream();

			while (true)
			{
				if (stream.DataAvailable)
				{
					byte[] data = new byte[256];

					string receivedMessage = string.Empty;
					int numBytes = stream.Read(data, 0, data.Length);
					receivedMessage = System.Text.Encoding.ASCII.GetString(data, 0, numBytes);
					if (!string.IsNullOrEmpty(receivedMessage))
					{
						messageQueue.Enqueue(receivedMessage);
					}
				}
				else
				{
					await Task.Delay(refreshDelayMillis);
				}
			}
		}

		public static MessageListener CreateInstance(TcpClient client, string name = "Listener")
		{
			GameObject gameObject = new GameObject(name);
			DontDestroyOnLoad(gameObject);
			MessageListener listener = gameObject.AddComponent<MessageListener>();
			listener.client = client;
			new Task(() => listener.ListenForMessage()).Start();

			return listener;
		}
	}
}