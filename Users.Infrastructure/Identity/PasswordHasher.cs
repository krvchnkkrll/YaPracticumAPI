using System.Security.Cryptography;
using System.Text;
using Users.Application.Interfaces.Identity;

namespace Users.Infrastructure.Identity;

internal sealed class PasswordHasher  : IPasswordHasher
{
    public string GetPasswordHash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool IsPasswordValid(string password, string passwordHash)
    {
        var hashPassword = GetPasswordHash(password);
        return hashPassword == passwordHash;
    }
}