using System.Security.Cryptography;
using UserMicroservice.Config;
using UserMicroservice.Models;
using UserMicroservice.Repository;

namespace UserMicroservice.Service;

public class RefreshTokenService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly HashingConfig _hashing;

    public RefreshTokenService(UnitOfWork unitOfWork, HashingConfig hashing)
    {
        _unitOfWork = unitOfWork;
        _hashing = hashing;
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<RefreshTokenModel?> ValidateRefreshToken(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var hashToken = _hashing.HashRefreshToken(refreshToken);

        var query = await _unitOfWork.refreshTokenRepository.GetFilter(
            page: 1,
            limit: 1,
            x =>
                x.RefreshToken == hashToken
        );

        return query.Results.FirstOrDefault();
    }

}