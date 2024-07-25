using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using webchat.Models;
using WsChatApi.Service;

namespace webchat.Service
{
    public class WebSocketService
    {
        private WebSocket _webSocket;
        private readonly IConfiguration _config;
        private WebSocketConnectionManager _manager;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public WebSocketService(IConfiguration config)
        {
            _config = config;
            _manager = new WebSocketConnectionManager();

        }
        public async Task AcceptWebSocketAsync(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                _webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
                await ReceiveMessagesAsync();
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024 * 4];
            while (_webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
            }
        }

        public async Task SendMessageAsync(ActionPayload payload)
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                var messageJson = JsonConvert.SerializeObject(payload);
                var bytes = Encoding.UTF8.GetBytes(messageJson);
                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                await _webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
            }
        }

        public async Task BroadcastMessageAsync(ActionPayload payload)
        {
            var messageJson = JsonConvert.SerializeObject(payload);
            var bytes = Encoding.UTF8.GetBytes(messageJson);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            foreach (var socket in _manager.GetAllSockets().Values)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                    await _webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                }
            }

        }

        public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
        {
            _manager.AddSocket(webSocket);

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                foreach (var socket in _manager.GetAllSockets().Values)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // await manager.RemoveSocket(webSocket);
        }
    }
}
