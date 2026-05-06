using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Models;
using UserMicroservice.Service;

namespace UserMicroservice.Controller;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterNewUser(CreateUserRequestModel request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.Register(request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Controller Error]: {ex.Message}");

            return StatusCode(500, new ApiResponseModel<UserViewModel>
            {
                Status = false,
                Message = "Lỗi xử lý yêu cầu tại Controller.",
                Response = null
            });
        }
    }
}
