using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using UserMicroservice.Config;
using UserMicroservice.Models;
using UserMicroservice.Repository;
using System.Security.Claims;
using Azure.Core;
using System.Net;



namespace UserMicroservice.Service;

public class AuthService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly HashingConfig _hashing;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UnitOfWork unitOfWork, HashingConfig hashing, IMapper mapper, IConfiguration config, RefreshTokenService refreshTokenService, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _hashing = hashing;
        _mapper = mapper;
        _config = config;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }


    public string GenerateJSONWebToken(UserModel user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "15")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
    {
        _logger.LogInformation("Processing login for user: {Email}", request.Email);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PlainPassword))
        {
            _logger.LogWarning("Login failed: Empty email or password for {Email}", request.Email);
            return new ApiResponseModel<LoginResponseModel>
            {
                Status = false,
                Message = "Dữ liệu không được trống",
                Response = null
            };
        }

        var userQuery = await _unitOfWork.userRepository.GetFilter(
            page: 1,
            limit: 1,
            x => x.Email == request.Email
        );

        if (userQuery.TotalItems == 0)
        {
            _logger.LogWarning("Login failed: User not found for {Email}", request.Email);
            return new ApiResponseModel<LoginResponseModel>
            {
                Status = false,
                Message = "Tài khoản không tồn tại",
                Response = null
            };
        }

        var user = userQuery.Results.First();
        if (!_hashing.VerifyPassword(request.PlainPassword, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for {Email}", request.Email);
            return new ApiResponseModel<LoginResponseModel>
            {
                Status = false,
                Message = "Thông tin đăng nhập sai",
                Response = null
            };
        }

        var access_token = GenerateJSONWebToken(user);
        var refresh_token = _refreshTokenService.GenerateRefreshToken();

        var refreshModel = new RefreshTokenModel
        {
            RefreshToken = _hashing.HashRefreshToken(refresh_token),
            Status = true,
            UserId = user.Id,
            ExpireTime = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")),
        };

        await _unitOfWork.refreshTokenRepository.Add(refreshModel);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Login successful for user: {Email}", request.Email);

        return new ApiResponseModel<LoginResponseModel>
        {
            Status = true,
            Message = "Đăng nhập thành công",
            Response = new LoginResponseModel
            {
                UserView = _mapper.Map<UserModel, UserViewModel>(user),
                Tokens = new TokenResponseModel
                {
                    AccessToken = access_token,
                    RefreshToken = refresh_token
                }
            }
        };
    }

    public async Task<TokenResponseModel> Refresh(string refreshToken)
    {
        var storedToken = await _refreshTokenService.ValidateRefreshToken(refreshToken);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        var user = (
            await _unitOfWork.userRepository.GetFilter(
                page: 1,
                limit: 1,
                x => x.Id == storedToken.UserId
            )
        ).Results.FirstOrDefault();

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (storedToken.ExpireTime >= DateTime.UtcNow)
        {
            return new TokenResponseModel
            {
                AccessToken = GenerateJSONWebToken(user),
                RefreshToken = refreshToken
            };
        } else
        {
             storedToken.Status = false;
            await _unitOfWork.refreshTokenRepository.Update(storedToken);
        }

        var newRawRefreshToken = _refreshTokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshTokenModel
        {
            RefreshToken = _hashing.HashRefreshToken(newRawRefreshToken),
            Status = true,
            UserId = user.Id,
            ExpireTime = DateTime.UtcNow.AddDays(
                int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")
            )
        };

        await _unitOfWork.refreshTokenRepository.Add(newRefreshToken);

        await _unitOfWork.CommitAsync();


        return new TokenResponseModel
        {
            AccessToken = GenerateJSONWebToken(user),
            RefreshToken = newRawRefreshToken
        };

    }

}