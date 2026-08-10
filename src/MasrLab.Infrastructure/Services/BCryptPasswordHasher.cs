using MasrLab.Application.Common.Interfaces;

namespace MasrLab.Infrastructure.Services;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string plainTextPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainTextPassword);
    }

    public bool VerifyPassword(string plainTextPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
    }
}
