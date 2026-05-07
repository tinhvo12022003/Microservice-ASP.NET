using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Models;
using UserMicroservice.Service;

namespace UserMicroservice.Controller;

[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Received Email: '{request?.Email}' | PlainPassword: '{request?.PlainPassword}'");

            if (!ModelState.IsValid)
            {
                Console.WriteLine($"[DEBUG] ModelState Invalid: {string.Join(", ", ModelState.Values.SelectMany(x => x.Errors.Select(e => e.ErrorMessage)))}");
                return BadRequest(ModelState);
            }

            var result = await _authService.Login(request);

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


    [Authorize]
    [Route("test")]
    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Token hợp lệ");
    }

}