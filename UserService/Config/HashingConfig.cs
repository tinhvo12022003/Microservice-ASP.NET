using Microsoft.AspNetCore.Identity;

namespace UserMicroservice.Config;

public class HashingConfig
{
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string plainPassword)
    {
        return _hasher.HashPassword(new object(), plainPassword);
    }

    public bool VerifyPassword(string inputPassword, string storedHash)
    {
        var result = _hasher.VerifyHashedPassword(
            new object(),
            storedHash,
            inputPassword);

        return result == PasswordVerificationResult.Success;
    }

    public string HashRefreshToken(string refreshToken)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(refreshToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyRefreshToken(string inputToken, string storedHash)
    {
        var inputHash = HashRefreshToken(inputToken);
        return inputHash == storedHash;
    }

}