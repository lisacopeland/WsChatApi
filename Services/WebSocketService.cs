using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
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
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    _cancellationTokenSource.Token
                );
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None
                    );
                }
            }
        }
    }
}
