
using Microsoft.AspNetCore.Mvc;
using webchat.Service;

namespace webchat.Controllers
{
    [ApiController]
    [Route("[controller]")]

        public class WebSocketController : ControllerBase
        {
            private readonly WebSocketService _webSocketService;

            public WebSocketController(WebSocketService webSocketService)
            {
                _webSocketService = webSocketService;
            }

            [HttpGet("/ws")]
            public async Task Get()
            {
                if (HttpContext.WebSockets.IsWebSocketRequest)
                {
                    await _webSocketService.AcceptWebSocketAsync(HttpContext);
                }
                else
                {
                    HttpContext.Response.StatusCode = 400;
                }
            }
        }
    }
