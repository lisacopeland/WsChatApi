using Microsoft.AspNetCore.Mvc;
using webchat.Models;
using webchat.Service;
using webchat.Utilities;

namespace webchat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;
        private readonly WebSocketService _websocketService;

        public MessagesController(MessageService messageService, WebSocketService webSocketService)
        {
            _messageService = messageService;
            _websocketService = webSocketService;
        }

        [HttpGet]
        public async Task<List<MessageClass>> Get()
        {
           List<MessageClass> messages = await _messageService.GetAllAsync();
            return messages;
        } 

        [HttpPost]
        public async Task<IActionResult> Post(MessageClass newMessageClass)
        {
            ApiResponseClass result;
            try
            {
                await _messageService.CreateAsync(newMessageClass);
                Message payload = new Message();
                payload.MessageClass = newMessageClass;
                ActionPayload actionPayload = new ActionPayload();
                actionPayload.Action = WSConstants.userMessageCreatedAction;
                actionPayload.Payload = payload;
                await _websocketService.AcceptWebSocketAsync(HttpContext);
                await _websocketService.SendMessageAsync(actionPayload);
                result = new ApiResponseClass { Success = true };
                result.Message = "Message created successfully";
                result.Id = newMessageClass.Id;
                return Ok(result);
            }
            catch (Exception ex)
            {
                result = new ApiResponseClass { Success = false };
                result.Message = ex.Message;
                return BadRequest(result);
            }
        }

    }
}
