using Microsoft.AspNetCore.Mvc;
using webchat.Models;
using webchat.Service;

namespace webchat.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("LoggedInUsers")]
        public async Task<List<UserClass>> GetLoggedInUsers() =>
            await _userService.GetLoggedInUsersAsync();

        [HttpGet]
        public async Task<List<UserClass>> Get() => await _userService.GetAllAsync();

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserClass newUserClass)
        {
            ApiResponseClass result;
            try
            {

                result = await _userService.CreateAsync(newUserClass);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
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
