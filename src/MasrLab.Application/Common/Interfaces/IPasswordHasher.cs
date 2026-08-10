namespace MasrLab.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string plainTextPassword);
    bool VerifyPassword(string plainTextPassword, string hashedPassword);
}
