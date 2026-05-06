using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Models;
using UserMicroservice.Service;

namespace UserMicroservice.Controller;

[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    public UserController (UserService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost]
    
    public async Task<IActionResult> AddNew (UserDTO user)
    {
        await _userService.Register(user);
        return Ok("User created successfully");
    }
}
