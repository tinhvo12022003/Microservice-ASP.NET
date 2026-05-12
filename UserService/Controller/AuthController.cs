using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Models;
using UserMicroservice.Service;
using Microsoft.Extensions.Logging;
using System.Net;

namespace UserMicroservice.Controller;

[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthController> _logger;
    public AuthController(AuthService authService, IConfiguration config, ILogger<AuthController> logger)
    {
        _authService = authService;
        _config = config;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", request?.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for login request");

                return BadRequest(ModelState);
            }

            var result = await _authService.Login(request);

            if (!result.Status || result.Response == null)
            {
                return Unauthorized(result);
            }

            Response.Cookies.Append(
                "access_token",
                result.Response.Tokens.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(
                        int.Parse(
                            _config["Jwt:AccessTokenExpirationMinutes"] ?? "15"
                        )
                    )
                }
            );

            Response.Cookies.Append(
                "refresh_token",
                result.Response.Tokens.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(
                        int.Parse(
                            _config["Jwt:RefreshTokenExpirationDays"] ?? "7"
                        )
                    )
                }
            );


            return Ok(
                new
                {
                    result.Status,
                    result.Message,
                    result.Response.UserView
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing login request");

            return StatusCode(500,
                new ApiResponseModel<UserViewModel>
                {
                    Status = false,
                    Message = "Lỗi xử lý yêu cầu tại Controller.",
                    Response = null
                });
        }
    }


    [HttpPost("refresh")]
    // [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.Refresh(Request.Cookies["refresh_token"] ?? "");

            _logger.LogInformation("Refresh token successful");

            Response.Cookies.Append(
                "access_token",
                result.AccessToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(
                        int.Parse(
                            _config["Jwt:AccessTokenExpirationMinutes"] ?? "15"
                        )
                    )
                }
            );

            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(
                        int.Parse(
                            _config["Jwt:RefreshTokenExpirationDays"] ?? "7"
                        )
                    )
                }
            );

            return Ok(new ApiResponseModel<TokenResponseModel>
            {
                Status = true,
                Message = "Làm mới token thành công",
                Response = result
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                new ApiResponseModel<TokenResponseModel>
                {
                    Status = false,
                    Message = ex.Message,
                    Response = null
                }
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refresh token");

            return StatusCode(500,
                new ApiResponseModel<TokenResponseModel>
                {
                    Status = false,
                    Message = "Lỗi hệ thống khi xử lý refresh token",
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