
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using webchat.Models;

namespace webchat.Service
{
    public class WebSocketService
    {
        private WebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
    }
}
