using Microsoft.AspNetCore.Mvc;
using webchat.Models;
using webchat.Service;
using webchat.Utilities;
using WsChatApi.Service;

namespace webchat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;
        private readonly WebSocketConnectionManager _webSocketManager;
        private readonly UploadService _uploadService;

        public MessagesController(
            MessageService messageService,
            WebSocketConnectionManager webSocketManager,
            UploadService uploadService
        )
        {
            _messageService = messageService;
            _webSocketManager = webSocketManager;
            _uploadService = uploadService;
        }

        [HttpGet]
        public async Task<List<MessageClass>> Get()
        {
            List<MessageClass> messages = await _messageService.GetAllAsync();
            return messages;
        }

        [HttpPost]
        [Route("UploadMessageAsset")]
        public async Task<IActionResult> Post([FromBody] IFormFile file)
        {
            ApiResponseClass result;
            if (
                !Request.Headers.TryGetValue(
                    "messageId",
                    out Microsoft.Extensions.Primitives.StringValues messageId
                )
            )
            {
                result = new ApiResponseClass() { Success = false, Message = "Bad Upload Request" };
                return BadRequest(result);
            }

            var uploadFile = Request.Form.Files[0];
            result = await _uploadService.UploadMessageAsset(messageId, uploadFile);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
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
                // await _websocketService.AcceptWebSocketAsync(HttpContext);
                await _webSocketManager.BroadcastMessageAsync(actionPayload);
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
