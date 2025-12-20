using Microsoft.AspNetCore.Identity;

namespace FitnessReservation.Api.Auth;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(user: "u", password);

    public bool Verify(string hash, string password)
        => _hasher.VerifyHashedPassword("u", hash, password) == PasswordVerificationResult.Success;
}
