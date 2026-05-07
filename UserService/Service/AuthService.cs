using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using UserMicroservice.Config;
using UserMicroservice.Models;
using UserMicroservice.Repository;
using System.Security.Claims;


namespace UserMicroservice.Service;

public class AuthService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly HashingConfig _hashing;
    private readonly IMapper _mapper;
    private readonly IConfiguration _config;

    public AuthService(UnitOfWork unitOfWork, HashingConfig hashing, IMapper mapper, IConfiguration config)
    {
        _unitOfWork = unitOfWork;
        _hashing = hashing;
        _mapper = mapper;
        _config = config;
    }


    private string GenerateJSONWebToken(UserModel user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<ApiResponseModel<LoginResponseModel>> Login(LoginRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PlainPassword))
        {
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
            return new ApiResponseModel<LoginResponseModel>
            {
                Status = false,
                Message = "Thông tin đăng nhập sai",
                Response = null
            };
        }

        var token = GenerateJSONWebToken(user);

        return new ApiResponseModel<LoginResponseModel>
        {
            Status = true,
            Message = "Đăng nhập thành công",
            Response = new LoginResponseModel
            {
                UserView = _mapper.Map<UserModel, UserViewModel>(user),
                AccessToken = token
            }
        };
    }
}