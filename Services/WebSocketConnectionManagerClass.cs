using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using webchat.Models;

namespace WsChatApi.Service
{
    public class WebSocketConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets =
            new ConcurrentDictionary<string, WebSocket>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public void AddSocket(WebSocket socket)
        {
            string connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, socket);
        }

        public async Task RemoveSocket(string id)
        {
            _sockets.TryRemove(id, out WebSocket socket);
            await socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closed by the WebSocketConnectionManager",
                CancellationToken.None
            );
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
            return _sockets;
        }

        public async Task BroadcastMessageAsync(ActionPayload payload)
        {
            var messageJson = JsonConvert.SerializeObject(payload);
            var bytes = Encoding.UTF8.GetBytes(messageJson);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            var tasks = new List<Task>();

            foreach (var socket in _sockets.Values)
            {
                if (socket.State == WebSocketState.Open)
                {
                    tasks.Add(
                        socket.SendAsync(
                            arraySegment,
                            WebSocketMessageType.Text,
                            true,
                            _cancellationTokenSource.Token
                        )
                    );
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            string connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, webSocket);
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // Handle client-initiated close
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closed by the client",
                            CancellationToken.None
                        );
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Handle received message here
                    // Optionally, broadcast the message to all connected clients
                    // await SendMessageAsync(message);
                }
                catch (WebSocketException ex)
                {
                    Console.Error.WriteLine($"WebSocket error: {ex.Message}");
                    break;
                }
            }

            // Clean up after the connection is closed
            _sockets.TryRemove(connectionId, out _);
        }
    }
}
